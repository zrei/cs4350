using System.Collections.Generic;
using UnityEngine;

public delegate void IntEvent(int _);
public delegate void VoidEvent();
public delegate void FloatEvent(float _);
public delegate void Vector3Event(Vector3 _);

public static class GlobalEvents {

    public static class UI {

    }

    public static class Battle {
        public delegate void UnitEvent(Unit _);
        public delegate void TurnOrderEvent(List<Unit> _);
        public delegate void PhaseEvent(PlayerTurnState _);
        public delegate void UnitAllegianceEvent(UnitAllegiance _);
        public delegate void AttackEvent(ActiveSkillSO activeSkill, Unit attacker, List<Unit> target);
        public static UnitEvent UnitDefeatedEvent;
        public static TurnOrderEvent TurnOrderUpdatedEvent;
        public static PhaseEvent PlayerPhaseUpdateEvent;
        public static VoidEvent PlayerUnitSetupStartEvent;
        public static VoidEvent PlayerUnitSetupEndEvent;
        public static VoidEvent PlayerTurnStartEvent;
        public static VoidEvent EnemyTurnStartEvent;
        public static UnitAllegianceEvent BattleEndEvent;
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
        public delegate void BattleNodeResultEvent(BattleNode battleNode, UnitAllegiance victor);
        public delegate void RewardNodeEvent(RewardNode _);
        public delegate void MassLevelUpEvent(List<LevelUpSummary> _);
        public delegate void LevelResultEvent(LevelResultType _);
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
        public static MassLevelUpEvent MassLevellingEvent;
        public static VoidEvent CloseLevellingScreenEvent;
        public static LevelResultEvent LevelEndEvent;
        public static VoidEvent ReturnFromLevelEvent;
        public static NodeEvent NodeHoverStartEvent;
        public static BattleNodeEvent BattleNodeHoverStartEvent;
        public static VoidEvent NodeHoverEndEvent;
    }

    public static class Scene
    {
        public delegate void BattleManagerEvent(BattleManager _);
        public static BattleManagerEvent BattleSceneLoadedEvent;
    }
}