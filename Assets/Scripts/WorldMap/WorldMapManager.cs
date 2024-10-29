using System.Collections;
using System.Collections.Generic;
using Game.Input;
using UnityEngine;

public class WorldMapManager : Singleton<WorldMapManager>
{
    [Header("Camera")]
    [SerializeField] private PlaneCameraController m_CameraController;

    [Header("Objects")]
    [SerializeField] private WorldMapPlayerToken m_PlayerToken;
    [Tooltip("World map nodes in order of level")]
    [SerializeField] private List<WorldMapNode> m_LevelNodes;

    [Space]
    // TODO: Grab information from the data instead
    [SerializeField] private PlayerCharacterSO m_Character;
    [SerializeField] private WeaponInstanceSO m_EquippedWeapon;
    [SerializeField] private PlayerClassSO m_PlayerClass;

    private WorldMapPlayerToken m_PlayerTokenInstance = null;
    private WorldMapNode m_CurrTargetNode = null;

    private int m_CurrLevel;

    private const float TOKEN_MOVE_DELAY = 0.3f;

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Level.ReturnFromLevelEvent += OnReturnFromLevel;
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent += OnBeginLoadLevel;

        EnableSelection();

        HandleDependencies();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Level.ReturnFromLevelEvent -= OnReturnFromLevel;
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent -= OnBeginLoadLevel;
        
        DisableSelection();
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;

        Initialise();
    }

    private void Initialise()
    {
        m_CurrLevel = SaveManager.Instance.LoadCurrentLevel();

        // initialise the all unlocked nodes 
        for (int i = 0; i < m_CurrLevel; ++i)
        {
            bool isCurrLevel = i == m_CurrLevel - 1;
            m_LevelNodes[i].Initialise(isCurrLevel ? LevelState.UNLOCKED : LevelState.CLEARED, isCurrLevel);
            m_LevelNodes[i].gameObject.SetActive(true);
        }

        // initialise the locked nodes
        for (int i = m_CurrLevel; i < m_LevelNodes.Count; ++i)
        {
            m_LevelNodes[i].Initialise(LevelState.LOCKED, false);
            m_LevelNodes[i].gameObject.SetActive(false);
        }

        // instantiate the player token
        m_PlayerTokenInstance = Instantiate(m_PlayerToken, Vector3.zero, Quaternion.identity);
        m_PlayerTokenInstance.Initialise(m_Character, m_PlayerClass, m_EquippedWeapon);
        
        // place the player on the current node
        WorldMapNode currNode = m_LevelNodes[m_CurrLevel - 1];
        currNode.PlacePlayerToken(m_PlayerTokenInstance);

        // initialise the camera
        m_CameraController.Initialise(m_PlayerTokenInstance.transform);
        m_CameraController.EnableCameraMovement();

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
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnSelectInput;
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
    }

    private void OnSelectInput(IInput input)
    {
        if (m_CurrTargetNode != null && m_CurrTargetNode.LevelNum != m_CurrLevel)
        {
            StartCoroutine(MoveToLevel(m_CurrLevel, m_CurrTargetNode));
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
    private void OnBeginLoadLevel()
    {
        m_CameraController.RecenterCamera();
        m_CameraController.DisableCameraMovement();
        DisableSelection();
    }

    private void OnReturnFromLevel()
    {
        m_CameraController.RecenterCamera();

        if (FlagManager.Instance.GetFlagValue(Flag.WIN_LEVEL_FLAG))
        {
            UnlockLevel();
        }
        else
        {
            GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(m_LevelNodes[m_CurrLevel - 1].LevelSO, false));
        }

        // reset flags
        FlagManager.Instance.SetFlagValue(Flag.WIN_LEVEL_FLAG, false, FlagType.SESSION);
        FlagManager.Instance.SetFlagValue(Flag.LOSE_LEVEL_FLAG, false, FlagType.SESSION);
    }
    #endregion

    #region Unlock Level
    private void UnlockLevel()
    {
        WorldMapNode currNode = m_LevelNodes[m_CurrLevel - 1];
        // TODO: need to handle final level case
        WorldMapNode nextNode = m_LevelNodes[m_CurrLevel];

        // reset curr level animation
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
            nextNode.ToggleCurrLevel(true);
            m_CameraController.EnableCameraMovement();
            EnableSelection();
            GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(nextNode.LevelSO, false));
        }

        m_CurrLevel += 1;
    }
    #endregion

    #region Navigating
    private IEnumerator MoveToLevel(int currLevelNumber, WorldMapNode newWorldMapNode)
    {
        // disable controls
        m_CameraController.DisableCameraMovement();
        DisableSelection();

        // fade the unit
        m_PlayerTokenInstance.FadeMesh(0f, 0.2f);
        yield return new WaitForSeconds(0.2f);

        // stop the current level animation
        m_LevelNodes[currLevelNumber - 1].ToggleCurrLevel(false);

        // fade unit back and change position
        newWorldMapNode.PlacePlayerToken(m_PlayerTokenInstance);
        m_PlayerTokenInstance.FadeMesh(1f, 0.2f);
        
        // update the current node
        m_CurrLevel = newWorldMapNode.LevelNum;
        newWorldMapNode.ToggleCurrLevel(true);
        GlobalEvents.WorldMap.OnGoToLevel?.Invoke(new LevelData(newWorldMapNode.LevelSO, newWorldMapNode.LevelState == LevelState.CLEARED));
        
        // re-enable controls
        m_CameraController.EnableCameraMovement();
        EnableSelection();
    }
    #endregion
}
