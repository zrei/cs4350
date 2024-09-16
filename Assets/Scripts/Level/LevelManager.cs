using System.Collections.Generic;
using System.Linq;
using Game.Input;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Graph Information
    [SerializeField] LevelNodeManager m_LevelNodeManager;
    
    // Graphics
    [SerializeField] LevelGraphicsManager m_LevelGraphicsManager;
    
    #region Input and Selected Node
    private NodeInternal m_CurrSelectedNode;
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
        var levelEdges = GetComponentsInChildren<LevelEdge>().ToList();
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
    }
    
    private void OnPointerSelect(IInput input)
    {
        TryPerformAction();
    }

    #endregion

    #region Perform Action

    private void TryPerformAction()
    {
        TrySelectNode();
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

    #endregion
}
