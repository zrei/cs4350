using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Input;
using Game.UI;
using Level.Nodes;
using UnityEngine;
using UnityEngine.Serialization;

public enum PlayerLevelSelectionState
{
    SELECTING_NODE,
    MOVING_NODE,
    RETRYING_NODE
}

public enum RewardType
{
    EXP,
    RATION,
    WEAPON
}

/// <summary>
/// Main driver class of the level, manages the overall level and player interactions
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    [Header("Manager References")]
    // Graph Information
    [SerializeField] LevelNodeManager m_LevelNodeManager;
    [SerializeField] LevelNodeVisualManager m_LevelNodeVisualManager;
    [SerializeField] LevelTokenManager m_LevelTokenManager;
    
    // Level Timer
    [FormerlySerializedAs("levelRationsManager")] [FormerlySerializedAs("m_LevelTimerLogic")] [SerializeField] LevelRationsManager m_LevelRationsManager;

    [SerializeField] PlaneCameraController m_LevelCameraController;
    
    [Header("Level Settings")]
    [SerializeField] private LevelSO m_LevelSO;
    [SerializeField] private LevelNode m_StartNode;
    [SerializeField] private LevelNode m_GoalNode;

    #region BGM
    private int? m_LevelBGM = null;
    #endregion
    
    #region Current State Information
    
    private List<PlayerCharacterData> m_CurrParty;
    public List<PlayerCharacterData> CurrParty => m_CurrParty;
    
    private LevelNode m_CurrInputTargetNode;
    private LevelNode m_CurrSelectedNode;
    private PlayerLevelSelectionState m_CurrInputState = PlayerLevelSelectionState.SELECTING_NODE;

    private LevelNode m_DestNode;
    
    private BattleSO m_CurrBattleSO;
    private bool m_IsBattleSkipped;
    private UnitAllegiance m_PendingVictor;
    private int m_PendingNumTurns;
    
    private Dictionary<RewardType, int> m_PendingRewards = new ();
    private List<WeaponInstanceSO> m_PendingWeaponRewards = new ();
    
    private LevelNode m_CurrNode => m_LevelNodeManager.CurrentNode;
    private LevelResultType m_PendingLevelResult;
    
    #endregion

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Scene.OnBeginSceneChange += OnSceneChange;
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
        
        FlagManager.Instance.SetFlagValue(Flag.SKIP_BATTLE_FLAG, false, FlagType.SESSION);

        GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;
        
        GlobalEvents.Level.NodeHoverStartEvent -= OnNodeHoverStart;
        GlobalEvents.Level.NodeHoverEndEvent -= OnNodeHoverEnd;
        GlobalEvents.CharacterManagement.OnLordUpdate -= OnLordUpdate;
        
        if (InputManager.Instance)
        {
            DisableLevelGraphInput();
        }

        if (CurrentLevelState == LevelState.BATTLE_NODE)
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoadAfterBattle;
        }
    }

    public void Initialise(List<PlayerCharacterData> partyMembers)
    {
        m_CurrParty = partyMembers;

        var levelNodes = FindObjectsOfType<LevelNode>().ToList();
        var levelEdges = FindObjectsOfType<EdgeInternal>().ToList();
        
        // Initialise the internal graph representation of the level
        m_LevelNodeManager.Initialise(levelNodes, levelEdges);
        m_LevelNodeManager.SetStartNode(m_StartNode);
        m_LevelNodeManager.SetGoalNode(m_GoalNode);
        
        // Initialise the visuals of the level
        m_LevelNodeVisualManager.Initialise(levelNodes, levelEdges);
        
        // Initialise the timer
        m_LevelRationsManager.Initialise(m_LevelSO.m_StartingRations);
        
        // Initialise the player token
        m_LevelTokenManager.Initialise(m_CurrParty[0].GetBattleData(), 
            m_LevelNodeVisualManager.GetNodeVisual(m_StartNode));
        
        // Set up level camera
        m_LevelCameraController.Initialise(m_LevelTokenManager.GetPlayerTokenTransform());
        CameraManager.Instance.SetUpLevelCamera();
        
        // Set up BGM
        m_LevelBGM = SoundManager.Instance.PlayWithFadeIn(m_LevelSO.m_LevelBGM);
        
        // Set up callbacks
        GlobalEvents.Level.NodeHoverStartEvent += OnNodeHoverStart;
        GlobalEvents.Level.NodeHoverEndEvent += OnNodeHoverEnd;
        GlobalEvents.CharacterManagement.OnLordUpdate += OnLordUpdate;

        // Start at start node's pre-dialogue stage
        CurrentLevelState = LevelState.PRE_DIALOGUE;
    }
    
    #endregion
    
    #region Level FSM

    public enum LevelState
    {
        NODE_SELECTION,
        MOVEMENT_TO_NODE,
        PRE_DIALOGUE,
        PRE_TUTORIAL,
        DIALOGUE_NODE,
        BATTLE_NODE,
        POST_BATTLE,
        REWARD_NODE,
        ASSIGN_REWARDS,
        LEVELLING,
        POST_TUTORIAL,
        POST_DIALOGUE,
        PRE_SELECTION_TUTORIAL,
        LEVEL_END
    }
    
    private LevelState m_CurrentLevelState;

    public LevelState CurrentLevelState
    {
        get => m_CurrentLevelState;
        set
        {
            m_CurrentLevelState = value;
            OnLevelStateChanged();
        }
    }

    private void OnLevelStateChanged()
    {
        switch (CurrentLevelState)
        {
            case LevelState.NODE_SELECTION:
                OnStateNodeSelection();
                break;
            case LevelState.MOVEMENT_TO_NODE:
                OnStateMovementToNode();
                break;
            case LevelState.PRE_DIALOGUE:
                OnStatePreDialogue();
                break;
            case LevelState.PRE_TUTORIAL:
                OnStatePreTutorial();
                break;
            case LevelState.DIALOGUE_NODE:
                OnStateDialogueNode();
                break;
            case LevelState.BATTLE_NODE:
                OnStateBattleNode();
                break;
            case LevelState.POST_BATTLE:
                OnStatePostBattle();
                break;
            case LevelState.REWARD_NODE:
                OnStateRewardNode();
                break;
            case LevelState.ASSIGN_REWARDS:
                OnStateAssignRewards();
                break;
            case LevelState.LEVELLING:
                OnStateLevelling();
                break;
            case LevelState.POST_TUTORIAL:
                OnStatePostTutorial();
                break;
            case LevelState.POST_DIALOGUE:
                OnStatePostDialogue();
                break;
            case LevelState.PRE_SELECTION_TUTORIAL:
                OnStatePreSelectionTutorial();
                break;
            case LevelState.LEVEL_END:
                OnStateLevelEnd();
                break;
            default:
                Debug.Log("LevelManager: Invalid State");
                GameSceneManager.Instance.UnloadLevelScene(m_LevelSO.m_LevelId);
                break;
        }
    }

    /// <summary>
    /// This state handles the player's node selection phase logic, including moving around the map and
    /// previewing nodes.
    /// Transitions to MOVEMENT_TO_NODE state when a neighbouring node is selected to move to, or
    /// to the appropriate node event state if retrying the current node.
    /// </summary>
    private void OnStateNodeSelection()
    {
        DisplayMovableNodes();
        EnableLevelGraphInput();
    }

    /// <summary>
    /// This state handles the movement of the player token to the selected node.
    /// Transitions to PRE_DIALOGUE state after the movement is complete.
    /// </summary>
    private void OnStateMovementToNode()
    {
        var destNode = m_DestNode;
        var pathSpline = m_LevelNodeManager.GetEdgeToNode(destNode).GetPathSplineTo(destNode);
        
        if (pathSpline == null)
        {
            Debug.LogError("Node Movement: Path Spline is null");
            CurrentLevelState = LevelState.NODE_SELECTION;
            return;
        }
        
        m_LevelTokenManager.MovePlayerToNode(pathSpline, m_LevelNodeVisualManager.GetNodeVisual(destNode), OnMovementComplete);
        
        return;

        void OnMovementComplete()
        {
            m_LevelNodeManager.MoveToNode(destNode, out var rationCost);
            GlobalEvents.Rations.RationsChangeEvent?.Invoke(-rationCost);
            m_DestNode = null;

            CurrentLevelState = m_LevelNodeManager.IsCurrentNodeCleared() ? LevelState.NODE_SELECTION : LevelState.PRE_DIALOGUE;
        }
    }
    
    /// <summary>
    /// This state plays the pre-dialogue of the current node if any.
    /// Transitions to PRE_TUTORIAL state immediately if there are no dialogues, otherwise
    /// after the dialogue is finished.
    /// </summary>
    private void OnStatePreDialogue()
    {
        PlayDialogue(m_CurrNode.NodeData.GetPreDialogueToPlay(), () => CurrentLevelState = LevelState.PRE_TUTORIAL);
    }

    /// <summary>
    /// This state plays the pre-tutorial of the current node if any.
    /// Transitions to the appropriate node event state immediately if there are no tutorials, otherwise
    /// after the pre-tutorial screen is closed.
    /// </summary>
    private void OnStatePreTutorial()
    {
        PlayTutorial(m_CurrNode.NodeData.preTutorial, EnterNodeEventState);
    }
    
    /// <summary>
    /// This state handles the Dialogue type of node event.
    /// Transitions to POST_TUTORIAL state after the dialogue is finished.
    /// </summary>
    private void OnStateDialogueNode()
    {
        if (m_CurrNode.NodeData is not DialogueNodeDataSO dialogueNodeData)
        {
            Debug.LogError($"{m_CurrNode.name}: Node Data is not of Dialogue type");
            CurrentLevelState = LevelState.POST_TUTORIAL;
            return;
        }
        
        var mainDialogue = dialogueNodeData.GetMainDialogueToPlay();
        PlayDialogue(mainDialogue, () => StartCoroutine(TransitionToPostTutorialAfterDelay()));
        
        IEnumerator TransitionToPostTutorialAfterDelay()
        {
            // Add small delay for morality and ration animation changes
            yield return new WaitForSeconds(1.0f);
            m_LevelNodeManager.ClearCurrentNode();
            CurrentLevelState = LevelState.POST_TUTORIAL;
        }
    }

    /// <summary>
    /// This state handles the Battle type of node event.
    /// Transitions to POST_BATTLE state after the battle is over and return to level.
    /// </summary>
    private void OnStateBattleNode()
    {
        if (m_CurrNode.NodeData is not BattleNodeDataSO battleNodeData)
        {
            Debug.LogError($"{m_CurrNode.name}: Node Data is not of Battle type");
            CurrentLevelState = LevelState.POST_TUTORIAL;
            return;
        }
        
        m_CurrBattleSO = battleNodeData.battleSO;
        
        if (FlagManager.Instance.GetFlagValue(Flag.SKIP_BATTLE_FLAG))
        {
            Debug.Log($"{m_CurrNode.name}: Skipping Battle");
            m_IsBattleSkipped = true;
            CurrentLevelState = LevelState.POST_BATTLE;
            return;
        }
        
        SoundManager.Instance.FadeOutAndStop(m_LevelBGM.Value);
        m_LevelBGM = null;
        
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        
        GameSceneManager.Instance.LoadBattleScene(m_CurrBattleSO, 
            m_CurrBattleSO.m_OverrideCharacters ? m_CurrBattleSO.m_TutorialCharacters.Select(x => x.GetBattleData()).ToList() : m_CurrParty.Select(x => x.GetBattleData()).ToList(),
            m_CurrBattleSO.m_OverrideBattleMap ? m_CurrBattleSO.m_OverriddenBattleMapType : m_LevelSO.m_BiomeName, 
            m_LevelRationsManager.GetInflictedTokens());
    }

    /// <summary>
    /// This state handles the Post-Battle cleanup.
    /// Transitions to LEVELLING state after the battle result screen is closed.
    /// </summary>
    private void OnStatePostBattle()
    {
        if (!m_CurrBattleSO)
        {
            Debug.LogError($"{m_CurrNode.name}: No battle in progress to end");
            CurrentLevelState = LevelState.POST_TUTORIAL;
            return;
        }

        Debug.Log("LevelManager: Ending Battle");

        LevelNodeVisual levelNodeVisual = m_LevelNodeVisualManager.GetNodeVisual(m_CurrNode);

        if (m_IsBattleSkipped)
        {
            if (m_CurrNode.NodeData is not BattleNodeDataSO battleNodeDataSo)
            {
                Debug.LogError($"{m_CurrNode.name}: Node Data is not of Battle type");
                CurrentLevelState = LevelState.POST_TUTORIAL;
                return;
            }
            
            m_PendingRewards[RewardType.EXP] = m_PendingRewards.GetValueOrDefault(RewardType.EXP, 0) 
                                               + battleNodeDataSo.SkipBattleExpReward;
            m_LevelTokenManager.PlayBattleSkipAnimation(levelNodeVisual, OnSkipBattleAnimComplete);
            
            void OnSkipBattleAnimComplete()
            {
                m_LevelNodeManager.ClearCurrentNode();
            
                var battleNodeUIResults = new BattleResultUIData(m_CurrBattleSO, m_PendingVictor, true, battleNodeDataSo.SkipBattleExpReward, m_PendingNumTurns);
            
                IUIScreen battleNodeResultScreen = UIScreenManager.Instance.NodeBattleResultScreen;
                battleNodeResultScreen.OnHideDone += OnCloseBattleResultScreen;
                UIScreenManager.Instance.OpenScreen(battleNodeResultScreen, false, battleNodeUIResults);
            
                CleanupBattle();
            }
        }
        else if (m_PendingVictor == UnitAllegiance.PLAYER)
        {
            m_PendingRewards[RewardType.EXP] = m_PendingRewards.GetValueOrDefault(RewardType.EXP, 0) 
                                               + m_CurrBattleSO.m_ExpReward;
            m_LevelTokenManager.PlayClearAnimation(levelNodeVisual, OnSuccessAnimComplete);
            
            void OnSuccessAnimComplete()
            {
                m_LevelNodeManager.ClearCurrentNode();
            
                var battleNodeUIResults = new BattleResultUIData(m_CurrBattleSO, m_PendingVictor, false, m_CurrBattleSO.m_ExpReward, m_PendingNumTurns);
            
                IUIScreen nodeBattleResultScreen = UIScreenManager.Instance.NodeBattleResultScreen;
                nodeBattleResultScreen.OnHideDone += OnCloseBattleResultScreen;
                UIScreenManager.Instance.OpenScreen(nodeBattleResultScreen, false, battleNodeUIResults);
            
                CleanupBattle();
            }
        }
        else
        {
            m_LevelTokenManager.PlayFailureAnimation(levelNodeVisual, OnFailureAnimComplete, !m_LevelSO.m_FailOnDefeat);
            
            void OnFailureAnimComplete()
            {
                if (m_LevelSO.m_FailOnDefeat)
                {
                    m_PendingLevelResult = LevelResultType.DEFEAT;
                    CurrentLevelState = LevelState.LEVEL_END;
                }
                else
                {
                    CurrentLevelState = LevelState.NODE_SELECTION;
                }
                CleanupBattle();
            }
        }
        
        return;
        
        void OnCloseBattleResultScreen(IUIScreen screen)
        {
            screen.OnHideDone -= OnCloseBattleResultScreen;
            CurrentLevelState = LevelState.LEVELLING;
        }
        
        void CleanupBattle()
        {
            m_CurrBattleSO = null;
            m_IsBattleSkipped = false;
            m_PendingVictor = UnitAllegiance.NONE;
            m_PendingNumTurns = 0;
        }
    }

    /// <summary>
    /// This state handles the Reward type of node event.
    /// Transitions to CALCULATE_REWARDS state after the reward screen is closed.
    /// </summary>
    private void OnStateRewardNode()
    {
        if (m_CurrNode.NodeData is not RewardNodeDataSO rewardNodeData)
        {
            Debug.LogError($"{m_CurrNode.name}: Node Data is not of Reward type");
            CurrentLevelState = LevelState.POST_TUTORIAL;
            return;
        }
        
        m_LevelNodeManager.ClearCurrentNode();
        
        CurrentLevelState = LevelState.ASSIGN_REWARDS;
    }

    /// <summary>
    /// This state processes the pending rewards (Ration, Weapons) and apply them to the player.
    /// Transitions to LEVELLING state if there are EXP rewards to process, else transitions to POST_TUTORIAL state. 
    /// </summary>
    private void OnStateAssignRewards()
    {
        var nodeReward = m_CurrNode.NodeData.GetNodeReward();
        
        if (nodeReward.IsEmpty())
        {
            Debug.Log($"{m_CurrNode.name}: No rewards");
            CurrentLevelState = LevelState.POST_TUTORIAL;
            return;
        }
        
        IUIScreen rewardNodeResultScreen = UIScreenManager.Instance.NodeRewardResultScreen;
        rewardNodeResultScreen.OnHideDone += OnCloseRewardScreen;
        UIScreenManager.Instance.OpenScreen(rewardNodeResultScreen, false, nodeReward);
        
        void OnCloseRewardScreen(IUIScreen screen)
        {
            screen.OnHideDone -= OnCloseRewardScreen;
            ProcessNodeRewards(nodeReward);
            CurrentLevelState = LevelState.POST_TUTORIAL;
        }
    }

    /// <summary>
    /// This state processes the pending EXP rewards and apply them to the player characters.
    /// Will display EXP gain summaries and level up summaries.
    /// Transitions to POST_TUTORIAL state after EXP gain and level up screens are closed. 
    /// </summary>
    private void OnStateLevelling()
    {
        AddCharacterExp(m_PendingRewards[RewardType.EXP], out var expGainSummaries, out var levelledUpCharacters);
        m_PendingRewards[RewardType.EXP] = 0;
            
        IUIScreen expScreen = UIScreenManager.Instance.ExpScreen;
        expScreen.OnHideDone += levelledUpCharacters.Count > 0 ? ShowLevelUpScreen : OnCloseLevellingScreen;
        UIScreenManager.Instance.OpenScreen(expScreen, false, expGainSummaries);
        
        void ShowLevelUpScreen(IUIScreen expScreen)
        {
            expScreen.OnHideDone -= ShowLevelUpScreen;
            IUIScreen levelUpResultScreen = UIScreenManager.Instance.LevelUpResultScreen;
            levelUpResultScreen.OnHideDone += OnCloseLevellingScreen;
            UIScreenManager.Instance.OpenScreen(levelUpResultScreen, false, levelledUpCharacters);
        }
        
        void OnCloseLevellingScreen(IUIScreen screen)
        {
            screen.OnHideDone -= OnCloseLevellingScreen;
            CurrentLevelState = LevelState.ASSIGN_REWARDS;
        }
    }
    
    /// <summary>
    /// This state plays the post-tutorial of the current node if any.
    /// Transitions to POST_DIALOGUE state immediately if there are no tutorials, otherwise
    /// after the post-tutorial screen is closed.
    /// </summary>
    private void OnStatePostTutorial()
    {
        PlayTutorial(m_CurrNode.NodeData.postTutorial, () => CurrentLevelState = LevelState.POST_DIALOGUE);
    }
    
    /// <summary>
    /// This state plays the post-dialogue of the current node if any.
    /// Transitions to PRE_SELECTION_TUTORIAL state immediately if there are no dialogues, otherwise
    /// after the dialogue is finished.
    /// </summary>
    private void OnStatePostDialogue()
    {
        PlayDialogue(m_CurrNode.NodeData.GetPostDialogueToPlay(), () => CurrentLevelState = LevelState.PRE_SELECTION_TUTORIAL);
    }
    
    /// <summary>
    /// This state plays the tutorial before resuming player node selection, if any.
    /// Transitions to the next state immediately if there are no tutorials, otherwise
    /// after the post-tutorial screen is closed.
    /// The next state is either LEVEL_END if the goal node is cleared, or NODE_SELECTION otherwise.
    /// </summary>
    private void OnStatePreSelectionTutorial()
    {
        FlagManager.Instance.SetFlagValue(Flag.SKIP_BATTLE_FLAG, false, FlagType.SESSION);
        
        PlayTutorial(m_CurrNode.NodeData.preSelectionTutorial, () =>
        {
            if (m_LevelNodeManager.IsGoalNodeCleared())
            {
                m_PendingLevelResult = LevelResultType.SUCCESS;
                CurrentLevelState = LevelState.LEVEL_END;
            }
            else
            {
                CurrentLevelState = LevelState.NODE_SELECTION;
            }
        });
    }
    
    /// <summary>
    /// This state handles the end of the level, processing level rewards, performing post-level clean up and
    /// displaying the level results screen.
    /// This is a terminal state and will not transition to any other states.
    /// </summary>
    private void OnStateLevelEnd()
    {
        GlobalEvents.Level.LevelEndEvent?.Invoke();

        IUIScreen levelResultScreen = UIScreenManager.Instance.LevelResultScreen;
        levelResultScreen.OnHideDone += OnCloseLevelResultScreen;
        UIScreenManager.Instance.OpenScreen(levelResultScreen, false, new LevelResultUIData(m_LevelSO, m_PendingLevelResult));

        FlagManager.Instance.SetFlagValue(m_PendingLevelResult == LevelResultType.SUCCESS ? Flag.WIN_LEVEL_FLAG : Flag.LOSE_LEVEL_FLAG, true, FlagType.SESSION);

        if (m_PendingLevelResult == LevelResultType.SUCCESS)
        {
            Debug.Log("Receiving Reward Characters");
            CharacterDataManager.Instance.PostLevelBlanketLevelUp();
            CharacterDataManager.Instance.ReceiveCharacters(m_LevelSO.m_RewardCharacters);
            
            foreach (var weaponReward in m_LevelSO.m_RewardWeapons)
            {
                InventoryManager.Instance.ObtainWeapon(weaponReward);
            }
            // InventoryManager.Instance.SaveWeapons();
            
            FlagManager.Instance.SetFlagValue($"Level{m_LevelSO.m_LevelId+1}Complete", true, FlagType.PERSISTENT);
        }

        if (m_LevelBGM != null) SoundManager.Instance.FadeOutAndStop(m_LevelBGM.Value);
        m_LevelBGM = null;
        GlobalEvents.Level.LevelResultsEvent?.Invoke(m_LevelSO, m_PendingLevelResult);
        
        void OnCloseLevelResultScreen(IUIScreen screen)
        {
            screen.OnHideDone -= OnCloseLevelResultScreen;
            GameSceneManager.Instance.UnloadLevelScene(m_LevelSO.m_LevelId);
        }
    }

    private void EnterNodeEventState()
    {
        switch (m_CurrNode.NodeType)
        {
            case NodeType.DIALOGUE:
                CurrentLevelState = LevelState.DIALOGUE_NODE;
                break;
            case NodeType.BATTLE:
                CurrentLevelState = LevelState.BATTLE_NODE;
                break;
            case NodeType.REWARD:
                CurrentLevelState = LevelState.REWARD_NODE;
                break;
            case NodeType.EMPTY:
                m_LevelNodeManager.ClearCurrentNode();
                CurrentLevelState = LevelState.POST_TUTORIAL;
                break;
        }
    }

    #endregion

    #region Player Phase

    private void EndPlayerPhase()
    {
        DisableLevelGraphInput();
        DeselectNode();
        m_LevelNodeVisualManager.ClearMovableNodes();
        m_LevelCameraController.RecenterCamera();
        
        GlobalEvents.Level.EndPlayerPhaseEvent?.Invoke();
    }
    
    private void DisplayMovableNodes()
    {
        var movableNodes = m_LevelNodeManager.GetCurrentMovableNodes();
        
        m_LevelNodeVisualManager.ClearMovableNodes();
        m_LevelNodeVisualManager.DisplayMovableNodes(movableNodes);
    }

    #endregion

    #region Inputs
    
    private void EnableLevelGraphInput()
    {
        InputManager.Instance.PointerSelectInput.OnPressEvent += OnPointerSelect;
        
        InputManager.Instance.TogglePartyMenuInput.OnPressEvent += OnTogglePartyMenu;
        
        m_LevelCameraController.EnableCameraMovement();
    }
    
    private void DisableLevelGraphInput()
    {
        InputManager.Instance.PointerSelectInput.OnPressEvent -= OnPointerSelect;
        
        InputManager.Instance.TogglePartyMenuInput.OnPressEvent -= OnTogglePartyMenu;
        
        m_LevelCameraController.DisableCameraMovement();
    }
    
    private void OnNodeHoverStart(LevelNode node)
    {
        m_CurrInputTargetNode = node;
        
        UpdateInputState();
    }
    
    private void OnNodeHoverEnd()
    {
        m_CurrInputTargetNode = null;
        
        UpdateInputState();
    }
    
    private void OnPointerSelect(IInput input)
    {
        TryPerformAction();
        
        UpdateInputState();
    }
    
    private void OnTogglePartyMenu(IInput input)
    {
        IUIScreen characterManagementScreen = UIScreenManager.Instance.CharacterManagementScreen;
        if (!UIScreenManager.Instance.IsScreenOpen(characterManagementScreen))
        {
            Debug.Log("Opening Party Management Screen");
            UIScreenManager.Instance.OpenScreen(characterManagementScreen, false, m_CurrParty);
        }
        else if (UIScreenManager.Instance.IsScreenActive(characterManagementScreen))
        {
            Debug.Log("Closing Party Management Screen");
            UIScreenManager.Instance.CloseScreen();
        }
    }

    #endregion

    #region Update Input State

    private void UpdateInputState()
    {
        if (m_CurrSelectedNode && m_CurrSelectedNode == m_CurrInputTargetNode)
        {
            m_CurrInputState = m_CurrSelectedNode != m_LevelNodeManager.CurrentNode 
                ? PlayerLevelSelectionState.MOVING_NODE
                : PlayerLevelSelectionState.RETRYING_NODE;
        }
        else
        {
            m_CurrInputState = PlayerLevelSelectionState.SELECTING_NODE;
        }
    }

    #endregion

    #region Perform Action

    private void TryPerformAction()
    {
        switch (m_CurrInputState)
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
        if (m_CurrInputTargetNode)
        {
            if (m_CurrSelectedNode && m_CurrSelectedNode != m_CurrInputTargetNode)
            {
                GlobalEvents.Level.NodeDeselectedEvent(m_CurrSelectedNode);
            }

            SelectNode(m_CurrInputTargetNode);
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
        
        m_DestNode = destNode;
        CurrentLevelState = LevelState.MOVEMENT_TO_NODE;
    }
    
    private void TryRetryNode()
    {
        if (m_LevelNodeManager.IsCurrentNodeCleared())
        {
            Debug.Log("Node Retry: Current Node is already cleared");
            return;
        }
        
        EndPlayerPhase();
        
        EnterNodeEventState();
    }
    
    private void SelectNode(LevelNode node)
    {
        if (m_CurrSelectedNode)
        {
            DeselectNode();
        }
        
        m_CurrSelectedNode = node;
        GlobalEvents.Level.NodeSelectedEvent(m_CurrInputTargetNode);
    }
    
    private void DeselectNode()
    {
        var selectedNode = m_CurrSelectedNode;
        m_CurrSelectedNode = null;
        GlobalEvents.Level.NodeDeselectedEvent(selectedNode);
    }

    #endregion

    #region Callbacks
    
    void OnBattleEnd(UnitAllegiance victor, int numTurns)
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        
        if (!m_CurrBattleSO)
        {
            Debug.LogError($"{m_CurrNode.name}: No battle in progress to end");
            return;
        }

        // Save the battle result
        m_PendingVictor = victor;
        m_PendingNumTurns = numTurns;
            
        // Add exp reward to pending rewards
        m_PendingRewards[RewardType.EXP] = m_PendingRewards.GetValueOrDefault(RewardType.EXP, 0) 
                                           + m_CurrBattleSO.m_ExpReward;
            
        // Add time cost to pending rewards
        m_PendingRewards[RewardType.RATION] = m_PendingRewards.GetValueOrDefault(RewardType.RATION, 0) 
                                              - m_PendingNumTurns;
            
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoadAfterBattle;
    }
    
    void OnSceneLoadAfterBattle(SceneEnum fromScene, SceneEnum toScene)
    {
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoadAfterBattle;
        
        // If the scene did not transition from Battle to Level after battle ended, the results will be nullified
        if (fromScene != SceneEnum.BATTLE && toScene != SceneEnum.LEVEL)
        {
            Debug.LogWarning("LevelManager.OnSceneLoadAfterBattle: Scene did not transition from Battle to Level");
            m_PendingRewards.Clear();
            return;
        }
        
        if (!m_CurrBattleSO)
        {
            Debug.LogError($"{m_CurrNode.name}: No battle in progress to end");
            return;
        }
        
        m_LevelBGM = SoundManager.Instance.PlayWithFadeIn(m_LevelSO.m_LevelBGM);
        
        CurrentLevelState = LevelState.POST_BATTLE;
    }

    private void OnSceneChange(SceneEnum _, SceneEnum _2)
    {
        if (m_LevelBGM.HasValue)
        {
            SoundManager.Instance.FadeOutAndStop(m_LevelBGM.Value);
            m_LevelBGM = null;
        }
    }
    
    private void OnLordUpdate()
    {
        m_LevelTokenManager.UpdateAppearance(m_CurrParty[0].GetBattleData());
    }
    
    #endregion

    #region Tutorial

    private void PlayTutorial(TutorialSO tutorial, VoidEvent postEvent)
    {
        if (tutorial == null)
        {
            postEvent?.Invoke();
        } 
        else
        {
            IUIScreen tutorialScreen = UIScreenManager.Instance.TutorialScreen;
            tutorialScreen.OnHideDone += PostTutorial;
            UIScreenManager.Instance.OpenScreen(tutorialScreen, false, tutorial.m_TutorialPages);
        }

        void PostTutorial(IUIScreen screen)
        {
            screen.OnHideDone -= PostTutorial;
            postEvent?.Invoke();
        }
    }

    #endregion

    #region Dialogue

    private void PlayDialogue(Dialogue dialogue, VoidEvent postEvent)
    {
        if (dialogue == null)
        {
            postEvent?.Invoke();
        } 
        else
        {
            GlobalEvents.Dialogue.DialogueEndEvent += PostDialogue;
            DialogueDisplay.Instance.StartDialogue(dialogue);
        }

        void PostDialogue()
        {
            GlobalEvents.Dialogue.DialogueEndEvent -= PostDialogue;
            postEvent?.Invoke();
        }
    }

    #endregion

    #region Reward Handling
    
    private void ProcessNodeRewards(NodeReward nodeReward)
    {
        // Process ration rewards
        if (nodeReward.rationReward != 0)
        {
            GlobalEvents.Rations.RationsChangeEvent?.Invoke(nodeReward.rationReward);
        }
        
        // Process weapon rewards
        if (nodeReward.weaponRewards is { Length: > 0 })
        {
            foreach (var weapon in nodeReward.weaponRewards)
            {
                InventoryManager.Instance.ObtainWeapon(weapon);
            }
        }
    }
    
    private void AddCharacterExp(int expAmount, out List<ExpGainSummary> expGainSummaries, out List<LevelUpSummary> levelledUpCharacters)
    {
        levelledUpCharacters = new List<LevelUpSummary>();
        expGainSummaries = new List<ExpGainSummary>();
            
        // Add exp points to each character
        foreach (var characterData in m_CurrParty)
        {
            var initialLevel = characterData.m_CurrLevel;
                
            LevellingManager.Instance.LevelCharacter(characterData, expAmount,
                out var levelledUp, out var totalStatGrowth);
                
            expGainSummaries.Add(new ExpGainSummary(characterData, initialLevel, expAmount));
            if (levelledUp)
            {
                levelledUpCharacters.Add(new LevelUpSummary(characterData, initialLevel, totalStatGrowth));
            }
        }
    }

    #endregion
}
