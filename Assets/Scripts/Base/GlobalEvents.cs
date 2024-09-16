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
        public static VoidEvent PlayerTurnStartEvent;
        public static VoidEvent EnemyTurnStartEvent;
        public static UnitAllegianceEvent BattleEndEvent;
        public static UnitEvent PreviewUnitEvent;
        public static UnitEvent PreviewCurrentUnitEvent;
        public static AttackEvent AttackAnimationEvent; 
        public static VoidEvent CompleteAttackAnimationEvent;
    }
}