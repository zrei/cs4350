using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSequence : EnemyAction 
{
    public List<SequenceAction> m_SequenceActions;
    [Tooltip("Conditions that will result in this sequence being broken early")]
    public List<ActionConditionSO> m_DefaultBreakConditions;

    public override IConcreteAction GenerateConcreteAction()
    {
        return new ActionSequenceRuntimeInstance(this);
    }
}

[System.Serializable]
public struct SequenceAction 
{
    public EnemyAction m_Action;
    [Tooltip("Conditions that will be checked when the sequence is at this stage that can result in the whole sequence being broken early")]
    public List<ActionConditionSO> m_BreakConditions;
}

public struct ConcreteSequenceAction
{
    public IConcreteAction m_Action;
    public List<ActionConditionSO> m_BreakConditions;

    public ConcreteSequenceAction(IConcreteAction concreteAction, List<ActionConditionSO> breakConditions)
    {
        m_Action = concreteAction;
        m_BreakConditions = breakConditions;
    }

    public bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_BreakConditions.Any(x => x.IsConditionMet(enemyUnit, mapLogic)))
            return true;

        if (m_Action.ShouldBreakOut(enemyUnit, mapLogic))
            return true;

        return false;
    }
}

public class ActionSequenceRuntimeInstance : IConcreteAction
{
    private List<ConcreteSequenceAction> m_Sequence;
    private List<ActionConditionSO> m_DefaultBreakConditions;

    private int m_Index = 0;

    public ActionSequenceRuntimeInstance(ActionSequence actionSequence)
    {
        m_Sequence = actionSequence.m_SequenceActions.Select(x => new ConcreteSequenceAction(x.m_Action.GenerateConcreteAction(), x.m_BreakConditions)).ToList();
        m_DefaultBreakConditions = actionSequence.m_DefaultBreakConditions;
    }

    public EnemyActionWrapper GetActionToBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_Index >= m_Sequence.Count)
            m_Index = 0;

        return m_Sequence[m_Index].m_Action.GetActionToBePerformed(enemyUnit, mapLogic);
    }

    public bool IsCompleted(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_Index >= m_Sequence.Count;
    }

    public void Reset()
    {
        if (m_Index < m_Sequence.Count)
            m_Sequence[m_Index].m_Action.Reset();
        m_Index = 0;
    }

    public void Run(EnemyUnit enemyUnit, MapLogic mapLogic, BoolEvent completeActionEvent)
    {
        m_Sequence[m_Index].m_Action.Run(enemyUnit, mapLogic, OnComplete);

        void OnComplete(bool canExtendTurn)
        {
            if (m_Sequence[m_Index].m_Action.IsCompleted(enemyUnit, mapLogic))
            {
                ++m_Index;   
            }

            completeActionEvent?.Invoke(canExtendTurn);
        }
    }

    public bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_DefaultBreakConditions.Any(x => x.IsConditionMet(enemyUnit, mapLogic)))
            return true;

        if (!IsCompleted(enemyUnit, mapLogic) && m_Sequence[m_Index].ShouldBreakOut(enemyUnit, mapLogic))
            return true;

        return false;
    }

    public HashSet<ActiveSkillSO> GetNestedActiveSkills()
    {
        HashSet<ActiveSkillSO> nestedActiveSkills = new();
        foreach (ConcreteSequenceAction concreteAction in m_Sequence)
        {
            nestedActiveSkills = new HashSet<ActiveSkillSO>(nestedActiveSkills.Union(concreteAction.m_Action.GetNestedActiveSkills()));
        }
        return nestedActiveSkills;
    }
}
