using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public delegate void IntEvent(int _);
public delegate void VoidEvent();
public delegate void FloatEvent(float _);
public delegate void Vector3Event(Vector3 _);

public static class GlobalEvents {

    public static class UI {
        public delegate void PartyEvent(List<PlayerCharacterData> _, bool inLevel);
        public static PartyEvent OpenPartyOverviewEvent;
        public static VoidEvent OnClosePartyOverviewEvent;
    }

    public static class Battle {
        public delegate void UnitEvent(Unit _);
        public delegate void TurnOrderEvent(List<Unit> _);
        public delegate void PhaseEvent(PlayerTurnState _);
        public delegate void BattleOutcomeEvent(UnitAllegiance _, int numTurns);
        public delegate void AttackEvent(ActiveSkillSO activeSkill, Unit attacker, List<Unit> target);
        public static UnitEvent UnitDefeatedEvent;
        public static TurnOrderEvent TurnOrderUpdatedEvent;
        public static PhaseEvent PlayerPhaseUpdateEvent;
        public static VoidEvent PlayerUnitSetupStartEvent;
        public static VoidEvent PlayerUnitSetupEndEvent;
        public static VoidEvent PlayerTurnStartEvent;
        public static VoidEvent EnemyTurnStartEvent;
        public static BattleOutcomeEvent BattleEndEvent;
        public static UnitEvent PreviewUnitEvent;
        public static UnitEvent PreviewCurrentUnitEvent;
        public static AttackEvent AttackAnimationEvent; 
        public static VoidEvent CompleteAttackAnimationEvent;
        public static VoidEvent ReturnFromBattleEvent;
    }

    public static class Level
    {
        public delegate void NodeEvent(NodeInternal _);
        public delegate void NodeTransitionEvent(NodeInternal _1, NodeInternal _2);
        public delegate void BattleNodeEvent(BattleNode _);
        public delegate void BattleNodeResultEvent(BattleNode battleNode, UnitAllegiance victor, int numTurns);
        public delegate void RewardNodeEvent(RewardNode _);
        public delegate void DialogueNodeEvent(DialogueNode _);
        public delegate void MassLevelUpEvent(List<LevelUpSummary> _);
        public delegate void LevelResultEvent(LevelSO levelSo, LevelResultType _);
        public static FloatEvent TimeRemainingUpdatedEvent;
        public static NodeEvent NodeEnteredEvent;
        public static NodeEvent NodeClearedEvent;
        public static NodeEvent NodeExitedEvent;
        public static NodeEvent NodeSelectedEvent;
        public static NodeEvent NodeDeselectedEvent;
        public static NodeTransitionEvent NodeMovementEvent;
        public static BattleNodeEvent BattleNodeStartEvent;
        public static BattleNodeResultEvent BattleNodeEndEvent;
        public static RewardNodeEvent RewardNodeStartEvent;
        public static VoidEvent CloseRewardScreenEvent;
        public static DialogueNodeEvent DialogueNodeEndEvent;
        public static MassLevelUpEvent MassLevellingEvent;
        public static VoidEvent CloseLevellingScreenEvent;
        /// <summary>
        /// This event is called the moment the level ends
        /// </summary>
        public static VoidEvent LevelEndEvent;
        /// <summary>
        /// This event is called to pass out the results
        /// </summary>
        public static LevelResultEvent LevelResultsEvent;
        public static VoidEvent ReturnFromLevelEvent;
        public static NodeEvent NodeHoverStartEvent;
        public static BattleNodeEvent BattleNodeHoverStartEvent;
        public static VoidEvent NodeHoverEndEvent;
    }

    public static class Scene
    {
        public static VoidEvent BattleSceneLoadedEvent;
        public static VoidEvent LevelSceneLoadedEvent;
        public static VoidEvent WorldMapSceneLoadedEvent;
        public static VoidEvent MainMenuSceneLoadedEvent;
    }

    public static class Dialogue
    {
        public static VoidEvent DialogueStartEvent;
        public static VoidEvent DialogueEndEvent;
    }
    
    public static class Morality
    {
        public static IntEvent MoralityChangeEvent;
        public static IntEvent MoralitySetEvent;
    }

    public static class Flags
    {
        public delegate void FlagEvent(string flag, bool value, FlagType flagType);
        public static FlagEvent SetFlagEvent;
    }

    public static class WorldMap 
    {
        public delegate void LevelEvent(LevelData levelData);
        public delegate void LevelSOEvent(LevelSO levelSO);
        public static LevelEvent OnGoToLevel;
        public static VoidEvent OnBeginLoadLevelEvent;
        public static LevelSOEvent OnPartySelectEvent;
    }

    public static class MainMenu
    {
        public static VoidEvent OnBeginLoadWorldMap;
        public static VoidEvent OnReturnToMainMenu;
    }

    public static class Save
    {
        public static VoidEvent OnBeginSaveEvent;
        public static VoidEvent OnCompleteSaveEvent;
    }
}
