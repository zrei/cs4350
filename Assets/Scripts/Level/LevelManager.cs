using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Input;
using Game.UI;
using Level;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerLevelSelectionState
{
    SELECTING_NODE,
    MOVING_NODE,
    RETRYING_NODE
}

public enum RewardType
{
    EXP,
    TIME,
    WEAPON
}

/// <summary>
/// Main driver class of the level, manages the overall level and player interactions
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Manager References")]
    // Graph Information
    [SerializeField] LevelNodeManager m_LevelNodeManager;
    [SerializeField] LevelNodeVisualManager m_LevelNodeVisualManager;
    [SerializeField] LevelTokenManager m_LevelTokenManager;
    
    // Level Timer
    [SerializeField] LevelTimerLogic m_LevelTimerLogic;
    
    // Unit Data
    [SerializeField] LevellingManager m_LevellingManager;

    [SerializeField] LevelCameraController m_LevelCameraController;
    
    [Header("Level Settings")]
    [SerializeField] private LevelSO m_LevelSO;
    [SerializeField] private NodeInternal m_StartNode;
    [SerializeField] private NodeInternal m_GoalNode;
    
    // UI
    IUIScreen m_CharacterManagementScreen;
    IUIScreen m_BattleNodeResultScreen;
    IUIScreen m_RewardNodeResultScreen;
    IUIScreen m_LevelUpResultScreen;
    IUIScreen m_LevelResultScreen;

    public delegate void LevelManagerEvent(LevelManager levelManager);
    public static LevelManagerEvent OnReady;

    #region Current State
    
    private List<PlayerCharacterData> m_CurrParty;
    
    private NodeInternal m_CurrSelectedNode;
    private PlayerLevelSelectionState m_CurrState = PlayerLevelSelectionState.SELECTING_NODE;
    
    private Dictionary<RewardType, int> m_PendingRewards = new ();
    private List<WeaponInstanceSO> m_PendingWeaponRewards = new ();
    
    #endregion
    
    #region Input and Selected Node
    
    private NodeInternal m_CurrTargetNode;
    private bool m_HasHitNode;
    
    #endregion
    
    #region Initialisation

    private void Start()
    {
        OnReady?.Invoke(this);
    }

    public void Initialise(List<PlayerCharacterData> partyMembers)
    {
        m_CurrParty = partyMembers;

        var levelNodes = FindObjectsOfType<NodeInternal>().ToList();
        var levelEdges = FindObjectsOfType<EdgeInternal>().ToList();
        var timeLimit = m_LevelSO.m_TimeLimit;
        
        // Initialise the internal graph representation of the level
        m_LevelNodeManager.Initialise(levelNodes, levelEdges, timeLimit);
        m_LevelNodeManager.SetGoalNode(m_GoalNode);
        
        // Initialise the timer
        m_LevelTimerLogic.Initialise(timeLimit);
        
        // Initialise the visuals of the level
        m_LevelNodeVisualManager.Initialise(levelNodes, levelEdges);
        
        // Initialise the player token
        m_LevelTokenManager.Initialise(m_CurrParty[0].GetBattleData(), 
            m_LevelNodeVisualManager.GetNodeVisual(m_StartNode));
        
        Debug.Log(m_LevelCameraController);
        Debug.Log(m_LevelTokenManager);
        // Set up level camera
        m_LevelCameraController.Initialise(m_LevelTokenManager.GetPlayerTokenTransform());
        
        m_LevelNodeManager.SetStartNode(m_StartNode);
        
        AddNodeEventCallbacks();

        // Preload UI Screens
        m_CharacterManagementScreen = UIScreenManager.Instance.CharacterManagementScreen;
        m_BattleNodeResultScreen = UIScreenManager.Instance.BattleNodeResultScreen;
        m_RewardNodeResultScreen = UIScreenManager.Instance.RewardNodeResultScreen;
        m_LevelUpResultScreen = UIScreenManager.Instance.LevelUpResultScreen;
        m_LevelResultScreen = UIScreenManager.Instance.LevelResultScreen;
        
        CameraManager.Instance.SetUpLevelCamera();
        
        GlobalEvents.Scene.LevelSceneLoadedEvent?.Invoke();
        
        StartPlayerPhase();
    }
    
    #endregion

    #region Player Phase
    
    private void StartPlayerPhase()
    {
        if (m_LevelNodeManager.IsGoalNodeCleared())
        {
            GlobalEvents.Level.LevelEndEvent?.Invoke(m_LevelSO, LevelResultType.SUCCESS);
        }
        else
        {
            DisplayMovableNodes();
            EnableLevelGraphInput();
        }
    }

    private void EndPlayerPhase()
    {
        DisableLevelGraphInput();
        DeselectNode();
        m_LevelNodeVisualManager.ClearMovableNodes();
        m_LevelCameraController.RecenterCamera();
    }
    
    public void DisplayMovableNodes()
    {
        var movableNodes = m_LevelNodeManager.GetCurrentMovableNodes();
        
        m_LevelNodeVisualManager.ClearMovableNodes();
        m_LevelNodeVisualManager.DisplayMovableNodes(movableNodes);
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
    
    private void OnTogglePartyMenu(IInput input)
    {
        if (!UIScreenManager.Instance.IsScreenOpen(m_CharacterManagementScreen))
        {
            Debug.Log("Opening Party Management Screen");
            GlobalEvents.UI.OpenPartyOverviewEvent?.Invoke(m_CurrParty);
            UIScreenManager.Instance.OpenScreen(m_CharacterManagementScreen);
        }
        else if (UIScreenManager.Instance.IsScreenActive(m_CharacterManagementScreen))
        {
            Debug.Log("Closing Party Management Screen");
            UIScreenManager.Instance.CloseScreen();
        }
    }
    
    private void EnableLevelGraphInput()
    {
        InputManager.Instance.PointerPositionInput.OnChangeEvent += OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent += OnPointerSelect;
        
        InputManager.Instance.TogglePartyMenuInput.OnPressEvent += OnTogglePartyMenu;
        
        m_LevelCameraController.EnableCameraMovement();
    }
    
    private void DisableLevelGraphInput()
    {
        InputManager.Instance.PointerPositionInput.OnChangeEvent -= OnPointerPosition;
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnPointerSelect;
        
        InputManager.Instance.TogglePartyMenuInput.OnPressEvent -= OnTogglePartyMenu;
        
        m_LevelCameraController.DisableCameraMovement();
    }

    #endregion

    #region Update Input State

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
        
        EndPlayerPhase();
        
        m_LevelTokenManager.MovePlayerToNode(m_LevelNodeVisualManager.GetNodeVisual(destNode), OnMovementComplete);
        
        return;

        void OnMovementComplete()
        {
            m_LevelNodeManager.MoveToNode(destNode, out var timeCost);
            
            m_LevelTimerLogic.AdvanceTimer(timeCost);
        
            if (m_LevelTimerLogic.TimeRemaining <= 0) return;

            if (m_LevelNodeManager.IsCurrentNodeCleared())
            {
                StartPlayerPhase();
            }
            else
            {
                m_LevelNodeManager.StartCurrentNodeEvent();
            }
        }
    }
    
    private void TryRetryNode()
    {
        if (m_LevelNodeManager.IsCurrentNodeCleared())
        {
            Debug.Log("Node Retry: Current Node is already cleared");
            return;
        }
        
        EndPlayerPhase();
        
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

    #region Callbacks
    
    public void OnDisable()
    {
        if (InputManager.Instance)
        {
            DisableLevelGraphInput();
        }
        
        RemoveNodeEventCallbacks();
    }
    
    private void AddNodeEventCallbacks()
    {
        GlobalEvents.Level.BattleNodeStartEvent += OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent += OnBattleNodeEnd;
        GlobalEvents.Level.RewardNodeStartEvent += OnRewardNodeStart;
        GlobalEvents.Level.DialogueNodeEndEvent += OnDialogueNodeEnd;
        GlobalEvents.Level.LevelEndEvent += OnLevelEnd;
    }
    
    private void RemoveNodeEventCallbacks()
    {
        GlobalEvents.Level.BattleNodeStartEvent -= OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent -= OnBattleNodeEnd;
        GlobalEvents.Level.RewardNodeStartEvent -= OnRewardNodeStart;
        GlobalEvents.Level.DialogueNodeEndEvent -= OnDialogueNodeEnd;
        GlobalEvents.Level.LevelEndEvent -= OnLevelEnd;
    }

    private void OnBattleNodeStart(BattleNode battleNode)
    {
        Debug.Log("LevelManager: Starting Battle Node");
        
        GameSceneManager.Instance.LoadBattleScene(battleNode.BattleSO, m_CurrParty.Select(x => x.GetBattleData()).ToList(), m_LevelSO.m_BiomeObject);
    }
    
    private void OnBattleNodeEnd(BattleNode battleNode, UnitAllegiance victor, int numTurns)
    {
        Debug.Log("LevelManager: Ending Battle Node");

        NodeVisual battleNodeVisual = m_LevelNodeVisualManager.GetNodeVisual(battleNode);
        
        if (victor == UnitAllegiance.PLAYER)
        {
            m_LevelTokenManager.PlayClearAnimation(battleNodeVisual, OnSuccessAnimComplete);
        }
        else
        {
            m_LevelTokenManager.PlayFailureAnimation(battleNodeVisual, OnFailureAnimComplete);
        }
        
        return;

        void OnSuccessAnimComplete()
        {
            m_LevelNodeManager.ClearCurrentNode();
        
            // Add exp reward to pending rewards
            m_PendingRewards[RewardType.EXP] = m_PendingRewards.GetValueOrDefault(RewardType.EXP, 0) 
                                              + battleNode.BattleSO.m_ExpReward;
            
            // Add time cost to pending rewards
            m_PendingRewards[RewardType.TIME] = m_PendingRewards.GetValueOrDefault(RewardType.TIME, 0) 
                                               - numTurns;
        
            UIScreenManager.Instance.OpenScreen(m_BattleNodeResultScreen);
            
            GlobalEvents.Level.CloseRewardScreenEvent += OnCloseRewardScreen;
        }

        void OnFailureAnimComplete()
        {
            GlobalEvents.Level.LevelEndEvent?.Invoke(m_LevelSO, LevelResultType.DEFEAT);
        }
    }
    
    private void OnRewardNodeStart(RewardNode rewardNode)
    {
        Debug.Log("LevelManager: Starting Reward Node");
        
        // Maybe insert open chest animation here
        
        // Update the level state
        m_LevelNodeManager.ClearCurrentNode();
        
        // Add reward to pending rewards
        if (rewardNode.RewardType == RewardType.TIME)
            m_PendingRewards[RewardType.TIME] = m_PendingRewards.GetValueOrDefault(RewardType.TIME, 0) + rewardNode.RationReward;
        else if (rewardNode.RewardType == RewardType.WEAPON)
            m_PendingWeaponRewards.Add(rewardNode.WeaponReward);
        
        UIScreenManager.Instance.OpenScreen(m_RewardNodeResultScreen);
        
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
    
    private void OnDialogueNodeEnd(DialogueNode dialogueNode)
    {
        Debug.Log("LevelManager: Ending Dialogue Node");
        
        m_LevelNodeManager.ClearCurrentNode();
        
        StartPlayerPhase();
    }

    private void OnLevelEnd(LevelSO levelSo, LevelResultType result)
    {
        UIScreenManager.Instance.OpenScreen(m_LevelResultScreen);

        if (result == LevelResultType.SUCCESS)
        {
            Debug.Log("Receiving Reward Characters");
            CharacterDataManager.Instance.ReceiveCharacters(levelSo.m_RewardCharacters);
            CharacterDataManager.Instance.UpdateCharacterData(m_CurrParty);
            
            foreach (var weaponReward in levelSo.m_RewardWeapons)
            {
                InventoryManager.Instance.ObtainWeapon(weaponReward);
            }
            InventoryManager.Instance.SaveWeapons();
            
            FlagManager.Instance.SetFlagValue($"Level{m_LevelSO.m_LevelId+1}Complete", true, FlagType.PERSISTENT);
        }
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

        if (m_PendingRewards.ContainsKey(RewardType.TIME))
        {
            if (m_PendingRewards[RewardType.TIME] > 0)
            {
                m_LevelTimerLogic.AddTime(m_PendingRewards[RewardType.TIME]);
            }
            else
            {
                m_LevelTimerLogic.AdvanceTimer(-m_PendingRewards[RewardType.TIME]);
            }
            m_PendingRewards[RewardType.TIME] = 0;
            
            if (m_LevelTimerLogic.TimeRemaining <= 0)
            {
                hasEvent = true;
                return;
            }
        }
        
        if (m_PendingRewards.ContainsKey(RewardType.EXP))
        {
            AddCharacterExp(m_PendingRewards[RewardType.EXP], out var levelledUpCharacters);
            
            m_PendingRewards[RewardType.EXP] = 0;
            
            if (levelledUpCharacters.Count > 0)
            {
                GlobalEvents.Level.MassLevellingEvent?.Invoke(levelledUpCharacters);
                UIScreenManager.Instance.OpenScreen(m_LevelUpResultScreen);
                GlobalEvents.Level.CloseLevellingScreenEvent += OnCloseLevellingScreen;
                hasEvent = true;
            }
        }

        if (m_PendingWeaponRewards.Count > 0)
        {
            foreach (var weapon in m_PendingWeaponRewards)
            {
                InventoryManager.Instance.ObtainWeapon(weapon);
            }
            
            m_PendingWeaponRewards.Clear();
        }
    }
    
    private void AddCharacterExp(int expAmount, out List<LevelUpSummary> levelledUpCharacters)
    {
        levelledUpCharacters = new List<LevelUpSummary>();
            
        // Add exp points to each character
        foreach (var characterData in m_CurrParty)
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
