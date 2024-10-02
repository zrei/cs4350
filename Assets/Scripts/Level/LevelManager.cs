using System.Collections.Generic;
using System.Linq;
using Game.Input;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PlayerLevelSelectionState
{
    SELECTING_NODE,
    MOVING_NODE,
    RETRYING_NODE
}

public enum RewardType
{
    EXP,
    GOLD
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
    
    // Unit Data
    [SerializeField] PlayerUnit m_PlayerUnit;
    [SerializeField] LevellingManager m_LevellingManager;

    #region Current State
    
    private NodeInternal m_CurrSelectedNode;
    private PlayerLevelSelectionState m_CurrState = PlayerLevelSelectionState.SELECTING_NODE;

    private PlayerUnit m_PlayerUnitToken;
    
    private Dictionary<RewardType, int> m_PendingReward = new ();
    
    #endregion
    
    #region Input and Selected Node
    private NodeInternal m_CurrTargetNode;
    private bool m_HasHitNode;
    #endregion

    #region Test
    
    [Header("Test Settings")]
    [SerializeField] private LevelSO m_TestLevel;
    [SerializeField] private NodeInternal testStartNodeInternal;
    [SerializeField] private NodeInternal testGoalNodeInternal;
    
    // should be sent in in the future
    [SerializeField] private List<CharacterData> m_TestCharacterData;
    
    public void Start()
    {
        Initialise();
        
        StartPlayerPhase();
    }

    public void OnDisable()
    {
        if (InputManager.Instance)
        {
            DisableLevelGraphInput();
        }
        
        RemoveNodeEventCallbacks();
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
        m_LevelNodeManager.SetGoalNode(testGoalNodeInternal);
        
        // Initialise the timer
        m_LevelTimerLogic.Initialise(timeLimit);
        
        // Initialise the visuals of the level
        m_LevelNodeVisualManager.Initialise(levelNodes, levelEdges);
        m_LevelTimerVisual.Initialise(m_LevelTimerLogic);
        
        m_LevelNodeManager.SetStartNode(testStartNodeInternal);

        SetUpPlayerToken();
        
        AddNodeEventCallbacks();
    }
    
    private void StartPlayerPhase()
    {
        if (m_LevelNodeManager.IsGoalNodeCleared())
        {
            GlobalEvents.Level.LevelEndEvent?.Invoke(LevelResultType.SUCCESS);
        }
        else
        {
            DisplayMovableNodes();
            EnableLevelGraphInput();
        }
    }

    private void SetUpPlayerToken()
    {
        // TODO: Create specialised controller for unit tokens
        m_PlayerUnitToken = Instantiate(m_PlayerUnit);
        m_PlayerUnitToken.Initialise(m_TestCharacterData[0].GetBattleData());
        var tokenTransform = m_PlayerUnitToken.gameObject.transform;
        tokenTransform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        tokenTransform.position = m_LevelNodeManager.CurrentNode.transform.position + Vector3.up * 0.1f;
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
    
    private void EnableLevelGraphInput()
    {
        InputManager.Instance.PointerPositionInput.OnChangeEvent += OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent += OnPointerSelect;
    }
    
    private void DisableLevelGraphInput()
    {
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnPointerSelect;
    }

    #endregion

    #region Update State

    private void UpdateState()
    {
        if (m_CurrSelectedNode && m_HasHitNode && m_CurrSelectedNode == m_CurrTargetNode)
        {
            m_CurrState = m_CurrSelectedNode != m_LevelNodeManager.CurrentNode 
                ? PlayerLevelSelectionState.MOVING_NODE
                : PlayerLevelSelectionState.RETRYING_NODE;
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
            case PlayerLevelSelectionState.RETRYING_NODE:
                Debug.Log("Player Action: Retrying Node");
                TryRetryNode();
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
        
        DisableLevelGraphInput();
        DeselectNode();
        m_LevelNodeVisualManager.ClearMovableNodes();

        m_LevelNodeManager.MoveToNode(destNode, out var timeCost);
        
        MovePlayerTokenToNode(destNode);
        
        m_LevelTimerLogic.AdvanceTimer(timeCost);

        if (m_LevelNodeManager.IsCurrentNodeCleared())
        {
            StartPlayerPhase();
        }
        else
        {
            m_LevelNodeManager.StartCurrentNodeEvent();
        }
    }
    
    private void TryRetryNode()
    {
        if (m_LevelNodeManager.IsCurrentNodeCleared())
        {
            Debug.Log("Node Retry: Current Node is already cleared");
            return;
        }
        
        DisableLevelGraphInput();
        DeselectNode();
        m_LevelNodeVisualManager.ClearMovableNodes();
        
        m_LevelNodeManager.StartCurrentNodeEvent();
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

    #region Player Token

    private void MovePlayerTokenToNode(NodeInternal node)
    {
        var tokenTransform = m_PlayerUnitToken.transform;
        tokenTransform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        tokenTransform.position = node.transform.position + Vector3.up * 0.1f;
        tokenTransform.rotation = Quaternion.identity;
    }
    
    private void MovePlayerTokenToBattleNode(BattleNode battleNode)
    {
        // TODO: Remove direct reference to BattleNodeVisual
        var battleNodeVisual = battleNode.GetComponent<BattleNodeVisual>();
        
        battleNodeVisual.SetPlayerToken(m_PlayerUnitToken.gameObject);
    }

    #endregion

    #region Callbacks
    
    private void AddNodeEventCallbacks()
    {
        GlobalEvents.Level.BattleNodeStartEvent += OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent += OnBattleNodeEnd;
        GlobalEvents.Level.RewardNodeStartEvent += OnRewardNodeStart;
    }
    
    private void RemoveNodeEventCallbacks()
    {
        GlobalEvents.Level.BattleNodeStartEvent -= OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent -= OnBattleNodeEnd;
        GlobalEvents.Level.RewardNodeStartEvent -= OnRewardNodeStart;
    }

    private void OnBattleNodeStart(BattleNode battleNode)
    {
        Debug.Log("LevelManager: Starting Battle Node");
        
        // Disable inputs
        DisableLevelGraphInput();
        
        m_LevelCamera.gameObject.SetActive(false);
        GameSceneManager.Instance.LoadBattleScene(battleNode.BattleSO, m_TestCharacterData.Select(x => x.GetBattleData()).ToList(), m_TestLevel.m_BiomeObject);
    }
    
    private void OnBattleNodeEnd(BattleNode battleNode, UnitAllegiance victor)
    {
        Debug.Log("LevelManager: Ending Battle Node");
        
        GameSceneManager.Instance.UnloadBattleScene();
        m_LevelCamera.gameObject.SetActive(true);
        
        if (victor == UnitAllegiance.PLAYER)
        {
            m_LevelNodeManager.ClearCurrentNode();
            
            // Add reward to pending rewards
            m_PendingReward[RewardType.EXP] = m_PendingReward.GetValueOrDefault(RewardType.EXP, 0) 
                                              + battleNode.BattleSO.m_ExpReward;
            
            // Wait for reward screen to close
            GlobalEvents.Level.CloseRewardScreenEvent += OnCloseRewardScreen;
        }
        else
        {
            // No reward screen
            
            // Set player token to facing off on the battle node
            MovePlayerTokenToBattleNode(battleNode);
            
            StartPlayerPhase();
        }
    }
    
    private void OnRewardNodeStart(RewardNode rewardNode)
    {
        Debug.Log("LevelManager: Starting Reward Node");
        
        // Disable inputs
        DisableLevelGraphInput();
        
        // Maybe insert open chest animation here
        
        // Update the level state
        m_LevelNodeManager.ClearCurrentNode();
        
        // Add reward to pending rewards
        m_PendingReward[RewardType.GOLD] = m_PendingReward.GetValueOrDefault(RewardType.GOLD, 0) + rewardNode.GoldReward;
        
        // Wait for reward screen to close
        GlobalEvents.Level.CloseRewardScreenEvent += OnCloseRewardScreen;
    }
    
    private void OnCloseRewardScreen()
    {
        GlobalEvents.Level.CloseRewardScreenEvent -= OnCloseRewardScreen;
        
        ProcessReward(out var hasEvent);

        // If there are no further events, resume player phase
        if (!hasEvent)
        {
            StartPlayerPhase();
        }
    }
    
    private void OnCloseLevellingScreen()
    {
        GlobalEvents.Level.CloseLevellingScreenEvent -= OnCloseLevellingScreen;
        
        StartPlayerPhase();
    }
    
    #endregion

    #region Reward Handling

    /// <summary>
    /// Process the pending rewards (EXP, Gold) and apply them to the player
    /// </summary>
    /// <param name="hasEvent"> whether there are further events like levelling up </param>
    private void ProcessReward(out bool hasEvent)
    {
        hasEvent = false;
        
        if (m_PendingReward.ContainsKey(RewardType.EXP))
        {
            AddCharacterExp(m_PendingReward[RewardType.EXP], out var levelledUpCharacters);
            
            m_PendingReward[RewardType.EXP] = 0;
            
            if (levelledUpCharacters.Count > 0)
            {
                GlobalEvents.Level.MassLevellingEvent?.Invoke(levelledUpCharacters);
                GlobalEvents.Level.CloseLevellingScreenEvent += OnCloseLevellingScreen;
                hasEvent = true;
            }
        }

        if (m_PendingReward.ContainsKey(RewardType.GOLD))
        {
            // Add gold to player
        }
    }
    
    private void AddCharacterExp(int expAmount, out List<LevelUpSummary> levelledUpCharacters)
    {
        levelledUpCharacters = new List<LevelUpSummary>();
            
        // Add exp points to each character
        foreach (var characterData in m_TestCharacterData)
        {
            var initialLevel = characterData.m_CurrLevel;
                
            m_LevellingManager.LevelCharacter(characterData, expAmount,
                out var levelledUp, out var totalStatGrowth);
                
            if (levelledUp)
            {
                levelledUpCharacters.Add(new LevelUpSummary(characterData, initialLevel, totalStatGrowth));
            }
        }
    }

    #endregion
}
