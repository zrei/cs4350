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

    private WorldMapPlayerToken m_PlayerTokenInstance = null;
    private WorldMapNode m_CurrTargetNode = null;

    private int m_CurrUnlockedLevel;
    private int m_CurrSelectedLevel;

    private const float TOKEN_MOVE_DELAY = 0.3f;

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Level.ReturnFromLevelEvent += OnReturnFromLevel;
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent += OnBeginLoadLevel;
        GlobalEvents.Scene.LevelSceneLoadedEvent += OnLevelSceneLoaded;

        HandleDependencies();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Level.ReturnFromLevelEvent -= OnReturnFromLevel;
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent -= OnBeginLoadLevel;
        GlobalEvents.Scene.LevelSceneLoadedEvent -= OnLevelSceneLoaded;
        
        DisableAllControls();
    }

    private void HandleDependencies()
    {
        if (!CharacterDataManager.IsReady)
        {
            CharacterDataManager.OnReady += HandleDependencies;
            return;
        }

        CharacterDataManager.OnReady -= HandleDependencies;

        Initialise();
    }

    private void Initialise()
    {
        if (!SaveManager.Instance.TryLoadCurrentLevel(out m_CurrUnlockedLevel))
        {
            m_CurrUnlockedLevel = m_StartingLevel;
        }
        m_CurrSelectedLevel = m_CurrUnlockedLevel;

        // initialise the all unlocked nodes 
        for (int i = 0; i < m_CurrSelectedLevel; ++i)
        {
            bool isCurrLevel = i == m_CurrSelectedLevel - 1;
            m_WorldMapRegions[i].m_LevelNode.Initialise(isCurrLevel ? LevelState.UNLOCKED : LevelState.CLEARED, isCurrLevel);
            m_WorldMapRegions[i].m_LevelNode.gameObject.SetActive(true);
            m_WorldMapRegions[i].m_FogFade.gameObject.SetActive(false);
        }

        // initialise the locked nodes
        for (int i = m_CurrSelectedLevel; i < m_WorldMapRegions.Count; ++i)
        {
            m_WorldMapRegions[i].m_LevelNode.Initialise(LevelState.LOCKED, false);
            m_WorldMapRegions[i].m_LevelNode.gameObject.SetActive(false);
            m_WorldMapRegions[i].m_FogFade.gameObject.SetActive(true);
        }

        // instantiate the player token
        m_PlayerTokenInstance = Instantiate(m_PlayerToken, Vector3.zero, Quaternion.identity);
        if (CharacterDataManager.Instance.TryRetrieveLordCharacterData(out PlayerCharacterData lordData))
            // retrieve data from character data manager
            m_PlayerTokenInstance.Initialise(lordData.m_BaseData, lordData.CurrClass, lordData.GetWeaponInstanceSO());
        
        // place the player on the current node
        WorldMapNode currNode = GetWorldMapNode(m_CurrSelectedLevel);
        currNode.PlacePlayerToken(m_PlayerTokenInstance);

        // initialise the camera
        m_CameraController.Initialise(m_PlayerTokenInstance.transform);
        
        EnableAllControls();

        // initialise the UI
        GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(currNode.LevelSO, false));
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
    private void OnLevelSceneLoaded()
    {
        m_WorldMap.SetActive(false);
    }

    private void OnBeginLoadLevel()
    {
        m_CameraController.RecenterCamera();
        DisableAllControls();
    }

    private void OnReturnFromLevel()
    {
        m_WorldMap.SetActive(true);

        m_CameraController.RecenterCamera();

        if (FlagManager.Instance.GetFlagValue(Flag.WIN_LEVEL_FLAG))
        {
            UnlockLevel();
        }
        else
        {
            GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(GetWorldMapNode(m_CurrUnlockedLevel).LevelSO, false));
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
    private void UnlockLevel()
    {
        WorldMapNode currNode = GetWorldMapNode(m_CurrUnlockedLevel);
        // TODO: need to handle final level case
        WorldMapNode nextNode = GetWorldMapNode(m_CurrUnlockedLevel + 1);

        FogFader nextNodeFog = GetWorldMapFog(m_CurrUnlockedLevel + 1);
        nextNodeFog.gameObject.SetActive(false);

        LevelSO levelSO = currNode.LevelSO;

        m_CurrUnlockedLevel += 1;
        m_CurrSelectedLevel = m_CurrUnlockedLevel;

        SaveManager.Instance.SetCurrentLevel(m_CurrUnlockedLevel);
        SaveManager.Instance.Save(PostSave);

        void PostSave()
        {
            if (currNode.HasPostCutscene)
            {
                m_CutsceneManager.ShowCutscene(currNode.PostCutscene, () => PostLevelEndCutscene(currNode, nextNode, m_WorldMapRegions[m_CurrSelectedLevel - 1].m_FogFade));
            }
            else
            {
                PostLevelEndCutscene(currNode, nextNode, m_WorldMapRegions[m_CurrSelectedLevel - 1].m_FogFade);
            }
        }
    }

    private void PostLevelEndCutscene(WorldMapNode currNode, WorldMapNode nextNode, FogFader nextRegionFog)
    {
        FlagManager.Instance.SetFlagValue(currNode.LevelSO.PostDialogueFlag, true, FlagType.PERSISTENT);

        nextRegionFog.gameObject.SetActive(false);

        currNode.ToggleCurrLevel(false);

        // start path animation
        currNode.UnlockPath(PostUnlockPath);

        // start moving unit
        m_PlayerTokenInstance.MoveAlongSpline(TOKEN_MOVE_DELAY, Quaternion.LookRotation(-currNode.InitialSplineForwardDirection, m_PlayerTokenInstance.transform.up), currNode.Spline, Quaternion.LookRotation(currNode.transform.forward, m_PlayerTokenInstance.transform.up), PostMovement);

        // have the next node pop up
        void PostUnlockPath()
        {
            nextNode.gameObject.SetActive(true);
            nextNode.UnlockNode();
        }

        // re-enable world map controls
        void PostMovement()
        {
            if (nextNode.HasPreCutscene)
            {
                m_CutsceneManager.ShowCutscene(nextNode.PreCutscene, () => PostLevelBeginCutscene(nextNode));
            }
            else
            {
                PostLevelBeginCutscene(nextNode);
            }
        }
    }

    private void PostLevelBeginCutscene(WorldMapNode nextNode)
    {
        FlagManager.Instance.SetFlagValue(nextNode.LevelSO.PostDialogueFlag, true, FlagType.PERSISTENT);
        nextNode.ToggleCurrLevel(true);
        EnableAllControls();
        GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(nextNode.LevelSO, false));
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
        if (m_CurrSelectedLevel != m_CurrUnlockedLevel)
        {
            StartCoroutine(MoveToLevel(m_CurrSelectedLevel, GetWorldMapNode(m_CurrUnlockedLevel)));
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
        if (m_CurrSelectedLevel >= m_CurrUnlockedLevel)
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
        GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(newWorldMapNode.LevelSO, newWorldMapNode.LevelState == LevelState.CLEARED));
        
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
            GlobalEvents.UI.OpenPartyOverviewEvent?.Invoke(CharacterDataManager.Instance.RetrieveAllCharacterData(), false);
            UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.CharacterManagementScreen);
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
    #endregion

#if UNITY_EDITOR
    public void SetStartingLevel(int startingLevel)
    {
        m_StartingLevel = startingLevel;
    }
#endif
}
