using System.Collections.Generic;
using System.Linq;
using Game.Input;
using UnityEngine;

public enum PlayerLevelSelectionState
{
    SELECTING_NODE,
    MOVING_NODE
}

public class LevelManager : MonoBehaviour
{
    // Graph Information
    [SerializeField] LevelNodeManager m_LevelNodeManager;
    
    // Graphics
    [SerializeField] LevelGraphicsManager m_LevelGraphicsManager;

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
    }

    public void OnDestroy()
    {
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnPointerSelect;
    }

    #endregion
    
    #region Initialisation

    public void Initialise()
    {
        var levelNodes = GetComponentsInChildren<NodeInternal>().ToList();
        var levelEdges = GetComponentsInChildren<EdgeInternal>().ToList();
        var timeLimit = m_TestLevel.m_TimeLimit;
        
        // Initialise the internal graph representation of the level
        m_LevelNodeManager.Initialise(levelNodes, levelEdges, timeLimit);
        
        // Initialise the graphics of the level
        m_LevelGraphicsManager.Initialise(levelNodes, levelEdges);
        
        m_LevelNodeManager.SetStartNode(testStartNodeInternal);
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
                TrySelectNode();
                Debug.Log("Selecting Node");
                break;
            case PlayerLevelSelectionState.MOVING_NODE:
                TryMoveToNode();
                Debug.Log("Moving Node");
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

            m_CurrSelectedNode = m_CurrTargetNode;
            GlobalEvents.Level.NodeSelectedEvent(m_CurrTargetNode);
        }
        else if (m_CurrSelectedNode)
        {
            GlobalEvents.Level.NodeDeselectedEvent(m_CurrSelectedNode);
            m_CurrSelectedNode = null;
        }
    }

    private void TryMoveToNode()
    {
        var currNode = m_LevelNodeManager.CurrentNode;
        var destNode = m_CurrSelectedNode;
        
        m_LevelNodeManager.MoveToNode(destNode);
        
        GlobalEvents.Level.NodeExitedEvent(currNode);
        GlobalEvents.Level.NodeMovementEvent(currNode, destNode);
        GlobalEvents.Level.NodeEnteredEvent(destNode);
    }

    #endregion
}
