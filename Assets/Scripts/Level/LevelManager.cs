using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Input;
using UnityEngine;
using UnityEngine.Serialization;

public enum PlayerLevelSelectionState
{
    SELECTING_NODE,
    MOVING_NODE,
    STARTING_NODE_EVENT
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
    [SerializeField] private LevelSO m_TestLevel;
    [SerializeField] private NodeInternal testStartNodeInternal;
    [SerializeField] private List<Unit> m_TestPlayerUnits;
    
    public void Start()
    {
        Initialise();

        GlobalEvents.Level.NodeEnteredEvent(testStartNodeInternal);
        
        InputManager.Instance.PointerPositionInput.OnChangeEvent += OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent += OnPointerSelect;
        
        GlobalEvents.Level.BattleNodeStartEvent += OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent += OnBattleNodeEnd;
    }

    public void OnDestroy()
    {
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnPointerSelect;
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
            m_CurrState = m_CurrSelectedNode == m_LevelNodeManager.CurrentNode 
                ? PlayerLevelSelectionState.STARTING_NODE_EVENT 
                : PlayerLevelSelectionState.MOVING_NODE;
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
            case PlayerLevelSelectionState.STARTING_NODE_EVENT:
                Debug.Log("Player Action: Starting Node Event");
                TryStartNodeEvent();
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

        m_LevelNodeManager.MoveToNode(destNode, out var timeCost);
        
        m_LevelTimerLogic.AdvanceTimer(timeCost);
    }

    private void TryStartNodeEvent()
    {
        var currNode = m_LevelNodeManager.CurrentNode;

        if (currNode.IsCleared)
        {
            Debug.Log("Node Event: Node is already cleared");
            return;
        }
        
        DeselectNode();
        
        currNode.StartNodeEvent();
    }
    
    private void SelectNode(NodeInternal node)
    {
        m_CurrSelectedNode = node;
        GlobalEvents.Level.NodeSelectedEvent(m_CurrTargetNode);
    }
    
    private void DeselectNode()
    {
        var selectedNode = m_CurrSelectedNode;
        m_CurrSelectedNode = null;
        GlobalEvents.Level.NodeSelectedEvent(selectedNode);
    }

    #endregion

    #region Callbacks

    private void OnBattleNodeStart(BattleNode battleNode)
    {
        Debug.Log("Starting Battle Node");
        
        // Disable inputs
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnPointerSelect;
        
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd(battleNode);
        
        m_LevelCamera.gameObject.SetActive(false);
        GameSceneManager.Instance.LoadBattleScene();
    }
    
    private GlobalEvents.Battle.UnitAllegianceEvent OnBattleEnd(BattleNode battleNode)
    {
        return (victoriousSide) =>
        {
            Debug.Log("Ending Battle in 5 seconds");
            
            // Insert some delay here?
            
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd(battleNode);
            GlobalEvents.Level.BattleNodeEndEvent?.Invoke(battleNode);
        };
    }
    
    private void OnBattleNodeEnd(BattleNode battleNode)
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
