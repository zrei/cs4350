using System.Collections.Generic;
using System.Linq;
using Game.Input;
using UnityEngine;

public enum PlayerLevelSelectionState
{
    SELECTING_NODE,
    MOVING_NODE
}

/// <summary>
/// Main driver class of the level, manages the overall level and player interactions
/// </summary>
public class LevelManager : MonoBehaviour
{
    // Camera
    [SerializeField] private Camera m_LevelCamera;
    
    // Graph Information
    [SerializeField] LevelNodeManager m_LevelNodeManager;
    [SerializeField] LevelNodeVisualManager m_LevelNodeVisualManager;
    
    // Level Timer
    [SerializeField] LevelTimerLogic m_LevelTimerLogic;
    [SerializeField] LevelTimerVisual m_LevelTimerVisual;

    #region Current State
    private NodeInternal m_CurrSelectedNode;
    private PlayerLevelSelectionState m_CurrState = PlayerLevelSelectionState.SELECTING_NODE;
    #endregion
    
    #region Input and Selected Node
    private NodeInternal m_CurrTargetNode;
    private bool m_HasHitNode;
    #endregion
    
    #region Test
    
    [Header("Test Settings")]
    [SerializeField] private LevelSO m_TestLevel;
    [SerializeField] private NodeInternal testStartNodeInternal;
    [SerializeField] private List<CharacterBattleData> m_TestPlayer;
    
    public void Start()
    {
        Initialise();

        GlobalEvents.Level.NodeEnteredEvent(testStartNodeInternal);
        
        InputManager.Instance.PointerPositionInput.OnChangeEvent += OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent += OnPointerSelect;
        
        GlobalEvents.Level.BattleNodeStartEvent += OnBattleNodeStart;
        GlobalEvents.Battle.ReturnFromBattleEvent += OnBattleNodeEnd;
    }

    public void OnDisable()
    {
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnPointerSelect;
        
        GlobalEvents.Level.BattleNodeStartEvent -= OnBattleNodeStart;
        GlobalEvents.Battle.ReturnFromBattleEvent += OnBattleNodeEnd;
    }
    
    public void DisplayMovableNodes()
    {
        m_LevelNodeVisualManager.ClearMovableNodes();
        m_LevelNodeVisualManager.DisplayMovableNodes(m_LevelNodeManager.CurrentNode);
    }

    #endregion
    
    #region Initialisation

    public void Initialise()
    {
        var levelNodes = FindObjectsOfType<NodeInternal>().ToList();
        var levelEdges = FindObjectsOfType<EdgeInternal>().ToList();
        var timeLimit = m_TestLevel.m_TimeLimit;
        
        // Initialise the internal graph representation of the level
        m_LevelNodeManager.Initialise(levelNodes, levelEdges, timeLimit);
        
        // Initialise the timer
        m_LevelTimerLogic.Initialise(timeLimit);
        
        // Initialise the visuals of the level
        m_LevelNodeVisualManager.Initialise(levelNodes, levelEdges);
        m_LevelTimerVisual.Initialise(m_LevelTimerLogic);
        
        m_LevelNodeManager.SetStartNode(testStartNodeInternal);
        
        DisplayMovableNodes();
    }

    #endregion

    #region Inputs

    private void OnPointerPosition(IInput input)
    {
        var inputVector = input.GetValue<Vector2>();
        Vector3 mousePos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        m_HasHitNode = m_LevelNodeManager.TryRetrieveNode(Camera.main.ScreenPointToRay(mousePos), out m_CurrTargetNode);
        
        UpdateState();
    }
    
    private void OnPointerSelect(IInput input)
    {
        TryPerformAction();
        
        UpdateState();
    }

    #endregion

    #region Update State

    private void UpdateState()
    {
        if (m_CurrSelectedNode && m_HasHitNode && m_CurrSelectedNode == m_CurrTargetNode)
        {
            m_CurrState = PlayerLevelSelectionState.MOVING_NODE;
        }
        else
        {
            m_CurrState = PlayerLevelSelectionState.SELECTING_NODE;
        }
    }

    #endregion

    #region Perform Action

    private void TryPerformAction()
    {
        switch (m_CurrState)
        {
            case PlayerLevelSelectionState.SELECTING_NODE:
                Debug.Log("Player Action: Selecting Node");
                TrySelectNode();
                break;
            case PlayerLevelSelectionState.MOVING_NODE:
                Debug.Log("Player Action: Moving to Node");
                TryMoveToNode();
                break;
            default:
                Debug.Log("Player Action: Invalid State");
                break;
        }
    }

    private void TrySelectNode()
    {
        if (m_HasHitNode)
        {
            if (m_CurrSelectedNode && m_CurrSelectedNode != m_CurrTargetNode)
            {
                GlobalEvents.Level.NodeDeselectedEvent(m_CurrSelectedNode);
            }

            SelectNode(m_CurrTargetNode);
        }
        else if (m_CurrSelectedNode)
        {
            DeselectNode();
        }
    }

    private void TryMoveToNode()
    {
        var destNode = m_CurrSelectedNode;
        
        if (m_LevelNodeManager.IsCurrentNodeCleared() == false)
        {
            Debug.Log("Node Movement: Current Node is not yet cleared");
            return;
        }
        
        if (m_LevelNodeManager.CanMoveToNode(destNode) == false)
        {
            Debug.Log("Node Movement: Node is not reachable");
            return;
        }
        
        DeselectNode();
        m_LevelNodeVisualManager.ClearMovableNodes();

        m_LevelNodeManager.MoveToNode(destNode, out var timeCost);
        
        m_LevelTimerLogic.AdvanceTimer(timeCost);

        if (m_LevelNodeManager.IsCurrentNodeCleared())
        {
            DisplayMovableNodes();
        }
        else
        {
            m_LevelNodeManager.StartCurrentNodeEvent();
        }
    }
    
    private void SelectNode(NodeInternal node)
    {
        if (m_CurrSelectedNode)
        {
            DeselectNode();
        }
        
        m_CurrSelectedNode = node;
        GlobalEvents.Level.NodeSelectedEvent(m_CurrTargetNode);
    }
    
    private void DeselectNode()
    {
        var selectedNode = m_CurrSelectedNode;
        m_CurrSelectedNode = null;
        GlobalEvents.Level.NodeDeselectedEvent(selectedNode);
    }

    #endregion

    #region Callbacks

    private void OnBattleNodeStart(BattleNode battleNode)
    {
        Debug.Log("Starting Battle Node");
        
        // Disable inputs
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnPointerSelect;
        
        m_LevelCamera.gameObject.SetActive(false);
        GameSceneManager.Instance.LoadBattleScene(battleNode.BattleSO, m_TestPlayer);
    }
    
    private void OnBattleNodeEnd()
    {
        Debug.Log("Ending Battle Node");
        
        m_LevelCamera.gameObject.SetActive(true);
        
        GameSceneManager.Instance.UnloadBattleScene();
        
        // Update the level state
        m_LevelNodeManager.ClearCurrentNode();
        
        DisplayMovableNodes();
        
        // Enable inputs
        InputManager.Instance.PointerPositionInput.OnChangeEvent += OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent += OnPointerSelect;
    }
    

    #endregion
}
