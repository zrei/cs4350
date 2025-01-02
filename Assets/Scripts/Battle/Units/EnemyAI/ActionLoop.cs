using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionLoop : EnemyAction 
{
    public EnemyAction m_Action;
    [Tooltip("Conditions that cause the loop to break early")]
    public List<ActionConditionSO> m_BreakConditions;
    [Tooltip("Conditions that cause the loop to be fulfilled")]
    public List<ActionConditionSO> m_LoopConditionsToFulfill;

    public override IConcreteAction GenerateConcreteAction()
    {
        return new ActionLoopRuntimeInstance(this);
    }
}

public class ActionLoopRuntimeInstance : IConcreteAction
{
    private IConcreteAction m_Action;
    private List<ActionConditionSO> m_BreakConditions;
    private List<ActionConditionSO> m_LoopConditionsToFulfill;

    public ActionLoopRuntimeInstance(ActionLoop actionLoop)
    {
        m_Action = actionLoop.m_Action.GenerateConcreteAction();
        m_BreakConditions = actionLoop.m_BreakConditions;
        m_LoopConditionsToFulfill = actionLoop.m_LoopConditionsToFulfill;
    }

    public EnemyActionWrapper GetActionToBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_Action.GetActionToBePerformed(enemyUnit, mapLogic);
    }

    public bool IsCompleted(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_LoopConditionsToFulfill.All(x => x.IsConditionMet(enemyUnit, mapLogic));
    }

    public void Reset()
    {
        // pass
    }

    public void Run(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        m_Action.Run(enemyUnit, mapLogic, completeActionEvent);
    }

    public bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_BreakConditions.Any(x => x.IsConditionMet(enemyUnit, mapLogic)))
            return true;

        if (m_Action.ShouldBreakOut(enemyUnit, mapLogic))
            return true;

        return false;
    }

    public HashSet<ActiveSkillSO> GetNestedActiveSkills()
    {
        return m_Action.GetNestedActiveSkills();
    }
}
