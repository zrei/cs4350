using System.Collections;
using System.Collections.Generic;
using Game.Input;
using Game.UI;
using UnityEngine;

[System.Serializable]
public struct WorldMapRegion
{
    public WorldMapNode m_LevelNode;
    public FogFader m_FogFade;
}

public class WorldMapManager : Singleton<WorldMapManager>
{
    [Header("Level")]
    [SerializeField] private int m_StartingLevel = 1;

    [Header("Camera")]
    [SerializeField] private PlaneCameraController m_CameraController;

    [Header("Objects")]
    [SerializeField] private WorldMapPlayerToken m_PlayerToken;
    [Tooltip("World map nodes in order of level")]
    [SerializeField] private List<WorldMapRegion> m_WorldMapRegions;
    [SerializeField] private GameObject m_WorldMap;

    [Header("Cutscenes")]
    [SerializeField] private WorldMapCutsceneManager m_CutsceneManager;

    [Header("Tutorial")]
    [SerializeField] private List<TutorialPageUIData> m_Tutorial;

    //[Header("FadingFog")]
    //[SerializeField] private float m_FadeDuration = 1.0f;

    private WorldMapPlayerToken m_PlayerTokenInstance = null;
    private WorldMapNode m_CurrTargetNode = null;

    private int m_CurrUnlockedLevel;
    private int m_CurrSelectedLevel;

    private const float TOKEN_MOVE_DELAY = 0.3f;

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Scene.OnSceneTransitionEvent += OnSceneTransition;
        GlobalEvents.Scene.OnBeginSceneChange += OnBeginSceneChange;
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;

        StartCoroutine(Initialise());
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Scene.OnSceneTransitionEvent -= OnSceneTransition;
        GlobalEvents.Scene.OnBeginSceneChange -= OnBeginSceneChange;
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;
        
        DisableAllControls();
    }

    protected override void AddDependencies()
    {
        AddDependency<CharacterDataManager>();
        AddDependency<FlagManager>();
    }

    private IEnumerator Initialise()
    {
        yield return null;
        if (!SaveManager.Instance.TryLoadCurrentLevel(out m_CurrUnlockedLevel))
        {
            m_CurrUnlockedLevel = m_StartingLevel;
        }

        m_CurrSelectedLevel = GetFinalUnlockedLevel();

        // check which level to initialise up to - if the post cutscene of the previous level
        // has not been registered as seen, we only want to initialise up to the previous level
        int levelToInitialiseUpTo = m_CurrSelectedLevel;
        bool playPostCutsceneOfPrevLevel = false;
        
        // check for existing post-cutscene of the previous level that was not seen
        if (m_CurrSelectedLevel > 1)
        {
            if (!FlagManager.Instance.GetFlagValue(GetWorldMapNode(m_CurrSelectedLevel - 1).LevelSO.PostDialogueFlag))
            {
                levelToInitialiseUpTo = m_CurrSelectedLevel - 1;
                playPostCutsceneOfPrevLevel = true;
            }
        }

        // initialise all the unlocked nodes
        for (int i = 0; i < levelToInitialiseUpTo; ++i)
        {
            bool isCurrLevel = i == levelToInitialiseUpTo - 1;
            m_WorldMapRegions[i].m_LevelNode.Initialise(isCurrLevel ? LevelState.UNLOCKED : LevelState.CLEARED, isCurrLevel);
            m_WorldMapRegions[i].m_LevelNode.gameObject.SetActive(true);
            if (m_WorldMapRegions[i].m_FogFade != null)
                m_WorldMapRegions[i].m_FogFade.gameObject.SetActive(false);
        }

        // initialise the locked nodes
        for (int i = levelToInitialiseUpTo; i < m_WorldMapRegions.Count; ++i)
        {
            m_WorldMapRegions[i].m_LevelNode.Initialise(LevelState.LOCKED, false);
            m_WorldMapRegions[i].m_LevelNode.gameObject.SetActive(false);
            if (m_WorldMapRegions[i].m_FogFade != null)
                m_WorldMapRegions[i].m_FogFade.gameObject.SetActive(true);
        }

        // instantiate the player token
        m_PlayerTokenInstance = Instantiate(m_PlayerToken, Vector3.zero, Quaternion.identity);
        if (CharacterDataManager.Instance.TryRetrieveLordCharacterData(out PlayerCharacterData lordData))
            // retrieve data from character data manager
            m_PlayerTokenInstance.Initialise(lordData.m_BaseData, lordData.CurrClass, lordData.GetWeaponInstanceSO());
        
        // place the player on the current node
        WorldMapNode currNode = GetWorldMapNode(levelToInitialiseUpTo);
        currNode.PlacePlayerToken(m_PlayerTokenInstance);

        // initialise the camera
        m_CameraController.Initialise(m_PlayerTokenInstance.transform);
 
        if (playPostCutsceneOfPrevLevel)
        {
            CutsceneSequence(levelToInitialiseUpTo);
        }
        else if (!FlagManager.Instance.GetFlagValue(currNode.LevelSO.PreDialogueFlag))
        {
            PreLevelCutscene(m_CurrSelectedLevel);
        }
        else
        {
            ShowTutorial(() => SelectLevel(m_CurrSelectedLevel));
        }
    }
    #endregion

    #region Tutorial
    private void ShowTutorial(VoidEvent postTutorialShown)
    {
        if (!FlagManager.Instance.GetFlagValue(Flag.HAS_VISITED_WORLD_MAP))
        {
            FlagManager.Instance.SetFlagValue(Flag.HAS_VISITED_WORLD_MAP, true, FlagType.PERSISTENT);
            IUIScreen tutorialScreen = UIScreenManager.Instance.TutorialScreen;
            tutorialScreen.OnHideDone += PostTutorial;
            UIScreenManager.Instance.OpenScreen(tutorialScreen, false, m_Tutorial);
        }
        else
        {
            PostTutorial(null);
        }

        void PostTutorial(IUIScreen tutorialScreen)
        {
            if (tutorialScreen != null)
                tutorialScreen.OnHideDone -= PostTutorial;
            postTutorialShown?.Invoke();
        }
    }
    #endregion

    #region Selection Controls
    private void EnableSelection()
    {
        InputManager.Instance.PointerSelectInput.OnPressEvent += OnSelectInput;
        InputManager.Instance.PointerPositionInput.OnChangeEvent += OnPointerPosition;
    }

    private void DisableSelection()
    {
        if (!InputManager.IsReady) return;

        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnSelectInput;
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
    }

    private void OnSelectInput(IInput input)
    {
        if (m_CurrTargetNode != null && m_CurrTargetNode.LevelNum != m_CurrSelectedLevel)
        {
            StartCoroutine(MoveToLevel(m_CurrSelectedLevel, m_CurrTargetNode));
        }
    }

    private void OnPointerPosition(IInput input)
    {
        var inputVector = input.GetValue<Vector2>();
        Vector3 mousePos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        TryRetrieveNode(Camera.main.ScreenPointToRay(mousePos), out m_CurrTargetNode);
    }

    private bool TryRetrieveNode(Ray ray, out WorldMapNode node)
    {
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask.GetMask("WorldMap"));
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.white, 100f, false); 
        foreach (RaycastHit raycastHit in raycastHits)
        {
            node = raycastHit.collider.gameObject.GetComponentInParent<WorldMapNode>();

            if (node)
                return true;
        }
        node = default;
        
        return false;
    }
    #endregion

    #region Level Loading
    private void OnSceneTransition(SceneEnum sceneEnum)
    {
        if (sceneEnum != SceneEnum.LEVEL)
            return;

        m_WorldMap.SetActive(false);
    }

    private void OnBeginSceneChange(SceneEnum fromScene, SceneEnum toScene)
    {
        if (fromScene != SceneEnum.WORLD_MAP || toScene == SceneEnum.MAIN_MENU)
            return;

        m_CameraController.RecenterCamera();
        DisableAllControls();
    }

    private void OnSceneLoad(SceneEnum fromScene, SceneEnum toScene)
    {
        if (toScene != SceneEnum.WORLD_MAP || fromScene == SceneEnum.MAIN_MENU)
            return;

        m_WorldMap.SetActive(true);

        m_CameraController.RecenterCamera();

        if (FlagManager.Instance.GetFlagValue(Flag.WIN_LEVEL_FLAG))
        {
            m_CurrUnlockedLevel += 1;
            SaveManager.Instance.SetCurrentLevel(m_CurrUnlockedLevel);
            SaveManager.Instance.Save(() => CutsceneSequence(m_CurrSelectedLevel));
        }
        else
        {
            GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(GetWorldMapNode(m_CurrSelectedLevel).LevelSO, false));
            EnableAllControls();
        }

        // reset flags
        FlagManager.Instance.SetFlagValue(Flag.WIN_LEVEL_FLAG, false, FlagType.SESSION);
        FlagManager.Instance.SetFlagValue(Flag.LOSE_LEVEL_FLAG, false, FlagType.SESSION);
        FlagManager.Instance.SetFlagValue(Flag.QUIT_LEVEL_FLAG, false, FlagType.SESSION);

        UpdateCharacterToken();
    }
    #endregion

    #region Unlock Level
    private void PostLevelCutscene(int levelNum, VoidEvent additionalCallback = null)
    {
        WorldMapNode node = GetWorldMapNode(levelNum);

        m_CutsceneManager.ShowCutscene(node.PostCutscene, PostCutscene);

        void PostCutscene()
        {
            FlagManager.Instance.SetFlagValue(node.LevelSO.PostDialogueFlag, true, FlagType.PERSISTENT);
            additionalCallback?.Invoke();
        }
    }

    private void PreLevelCutscene(int levelNum)
    {
        WorldMapNode node = GetWorldMapNode(levelNum);

        m_CutsceneManager.ShowCutscene(node.PreCutscene, PostCutscene);

        void PostCutscene()
        {
            // saving always occurs after pre-cutscene is played
            FlagManager.Instance.SetFlagValue(node.LevelSO.PreDialogueFlag, true, FlagType.PERSISTENT);
            ShowTutorial(PostTutorial);
        }

        void PostTutorial()
        {
            SaveManager.Instance.Save(() => SelectLevel(levelNum));
        }
    }

    private void CutsceneSequence(int levelNum)
    {
        PostLevelCutscene(levelNum, PostCutscene);

        void PostCutscene()
        {
            if (levelNum + 1 >= m_WorldMapRegions.Count || (GlobalSettings.IsDemo && levelNum + 1 > GlobalSettings.FinalDemoLevel))
            {
                SaveManager.Instance.Save(() => UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.DemoEndScreen));
            }
            else
            {
                UnlockLevelAnimation(levelNum);
            }
        }
    }

    /// <summary>
    /// Pre level cutscene of next level always plays after unlock level animation
    /// </summary>
    /// <param name="levelNum"></param>
    /// <param name="additionalCallback"></param>
    private void UnlockLevelAnimation(int levelNum)
    {
        GlobalEvents.WorldMap.OnBeginLevelAnimationEvent?.Invoke();

        WorldMapNode currLevel = GetWorldMapNode(levelNum);
        WorldMapNode nextLevel = GetWorldMapNode(levelNum + 1);
        FogFader nextRegionFog = GetWorldMapFog(levelNum + 1);

        if (nextRegionFog != null)
            nextRegionFog.gameObject.SetActive(false);

        currLevel.ToggleCurrLevel(false);

        // start path animation
        currLevel.UnlockPath(PostUnlockPath);

        // start moving unit
        m_PlayerTokenInstance.MoveAlongSpline(TOKEN_MOVE_DELAY, Quaternion.LookRotation(-currLevel.InitialSplineForwardDirection, m_PlayerTokenInstance.transform.up), currLevel.Spline, currLevel.PositioningOffset, Quaternion.LookRotation(currLevel.transform.forward, m_PlayerTokenInstance.transform.up), PostMovement);

        // have the next node pop up
        void PostUnlockPath()
        {
            nextLevel.gameObject.SetActive(true);
            nextLevel.UnlockNode();
        }

        void PostMovement()
        {
            GlobalEvents.WorldMap.OnEndLevelAnimationEvent?.Invoke();
            PreLevelCutscene(levelNum + 1);
        }
    }

    private void SelectLevel(int levelNum)
    {
        m_CurrSelectedLevel = levelNum;
        WorldMapNode node = GetWorldMapNode(levelNum);
        node.ToggleCurrLevel(true);
        EnableAllControls();
        GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(node.LevelSO, levelNum < m_CurrUnlockedLevel));
    }
    #endregion

    #region Navigating
    private void EnableNavigation()
    {
        InputManager.Instance.NavigateLevelAction.OnChangeEvent += OnNavigateLevel;
        InputManager.Instance.ReturnToCurrLevelAction.OnPressEvent += OnFocusCurrentLevel;
        InputManager.Instance.TogglePartyMenuInput.OnPressEvent += TogglePartyManagement;
    }

    private void DisableNavigation()
    {
        if (InputManager.IsReady)
        {
            InputManager.Instance.NavigateLevelAction.OnChangeEvent -= OnNavigateLevel;
            InputManager.Instance.ReturnToCurrLevelAction.OnPressEvent -= OnFocusCurrentLevel;
            InputManager.Instance.TogglePartyMenuInput.OnPressEvent -= TogglePartyManagement;
        }
    }

    private void OnFocusCurrentLevel(IInput input)
    {
        if (m_CurrSelectedLevel != GetFinalUnlockedLevel())
        {
            StartCoroutine(MoveToLevel(m_CurrSelectedLevel, GetWorldMapNode(GetFinalUnlockedLevel())));
        }
        else
        {
            m_CameraController.RecenterCamera();
        }   
    }

    private void OnNavigateLevel(IInput input)
    {
        float navigateSide = input.GetValue<float>();

        if (navigateSide < 0)
            NavigateToPrev();
        else if (navigateSide > 0)
            NavigateToNext();
    }

    private void NavigateToPrev()
    {
        if (m_CurrSelectedLevel <= 1)
            return;

        StartCoroutine(MoveToLevel(m_CurrSelectedLevel, GetWorldMapNode(m_CurrSelectedLevel - 1)));
    }

    private void NavigateToNext()
    {
        if (m_CurrSelectedLevel >= GetFinalUnlockedLevel())
            return;

        StartCoroutine(MoveToLevel(m_CurrSelectedLevel, GetWorldMapNode(m_CurrSelectedLevel + 1)));
    }

    private IEnumerator MoveToLevel(int currLevelNumber, WorldMapNode newWorldMapNode)
    {
        // disable controls
        DisableAllControls();

        // fade the unit
        m_PlayerTokenInstance.FadeMesh(0f, 0.2f);
        yield return new WaitForSeconds(0.2f);

        // stop the current level animation
        GetWorldMapNode(currLevelNumber).ToggleCurrLevel(false);

        // fade unit back and change position
        newWorldMapNode.PlacePlayerToken(m_PlayerTokenInstance);
        m_PlayerTokenInstance.FadeMesh(1f, 0.2f);
        
        // update the current node
        m_CurrSelectedLevel = newWorldMapNode.LevelNum;
        newWorldMapNode.ToggleCurrLevel(true);
        GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(newWorldMapNode.LevelSO, newWorldMapNode.LevelNum < m_CurrUnlockedLevel));
        
        // re-enable controls
        EnableAllControls();
    }
    #endregion

    #region Overall Controls
    private void DisableAllControls()
    {
        m_CameraController.DisableCameraMovement();
        DisableSelection();
        DisableNavigation();

        GlobalEvents.CharacterManagement.OnLordUpdate -= UpdateCharacterToken;
    }

    private void EnableAllControls()
    {
        m_CameraController.EnableCameraMovement();
        EnableSelection();
        EnableNavigation();

        GlobalEvents.CharacterManagement.OnLordUpdate += UpdateCharacterToken;
    }
    #endregion

    #region Party Management
    private void TogglePartyManagement(IInput _)
    {
        if (!UIScreenManager.Instance.IsScreenOpen(UIScreenManager.Instance.CharacterManagementScreen))
        {
            Debug.Log("Opening Party Management Screen");
            UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.CharacterManagementScreen, false, CharacterDataManager.Instance.RetrieveAllCharacterData(new List<int>()));
        }
        else if (UIScreenManager.Instance.IsScreenActive(UIScreenManager.Instance.CharacterManagementScreen))
        {
            Debug.Log("Closing Party Management Screen");
            UIScreenManager.Instance.CloseScreen();
        }
    }

    private void UpdateCharacterToken()
    {
        if (CharacterDataManager.Instance.TryRetrieveLordCharacterData(out PlayerCharacterData lordData))
            // retrieve data from character data manager
            m_PlayerTokenInstance.UpdateAppearance(lordData.m_BaseData, lordData.CurrClass, lordData.GetWeaponInstanceSO());
    }
    #endregion

    #region Helper
    /// <summary>
    /// Helper to get the world map node based on the 1-indexed level number
    /// </summary>
    /// <param name="levelNumber"></param>
    /// <returns></returns>
    private WorldMapNode GetWorldMapNode(int levelNumber)
    {
        return m_WorldMapRegions[levelNumber - 1].m_LevelNode;
    }

    private FogFader GetWorldMapFog(int levelNumber)
    {
        return m_WorldMapRegions[levelNumber - 1].m_FogFade;
    }

    private int GetFinalUnlockedLevel()
    {
        return Mathf.Min(m_CurrUnlockedLevel, GlobalSettings.IsDemo ? GlobalSettings.FinalDemoLevel : m_WorldMapRegions.Count);
    }
    #endregion

#if UNITY_EDITOR
    public void SetStartingLevel(int startingLevel)
    {
        m_StartingLevel = startingLevel;
    }
#endif
}
