using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ActionSequenceSO", menuName = "ScriptableObject/ActionSequenceSO")]
public class ActionSequenceSO : ActionSO 
{
    public List<SequenceAction> m_SequenceActions;

    public List<EnemyActionConditionSO> m_DefaultBreakConditions;

    public override IConcreteAction GenerateConcreteAction()
    {
        return new ActionSequence(this);
    }
}

[System.Serializable]
public struct SequenceAction 
{
    public ActionSO m_Action;
    public List<EnemyActionConditionSO> m_BreakConditions;
}

public struct ConcreteSequenceAction
{
    public IConcreteAction m_Action;
    public List<EnemyActionConditionSO> m_BreakConditions;

    public ConcreteSequenceAction(IConcreteAction concreteAction, List<EnemyActionConditionSO> breakConditions)
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

public class ActionSequence : IConcreteAction
{
    private List<ConcreteSequenceAction> m_Sequence;
    private List<EnemyActionConditionSO> m_DefaultBreakConditions;

    private int m_Index = 0;

    public ActionSequence(ActionSequenceSO actionSequenceSO)
    {
        m_Sequence = actionSequenceSO.m_SequenceActions.Select(x => new ConcreteSequenceAction(x.m_Action.GenerateConcreteAction(), x.m_BreakConditions)).ToList();
        m_DefaultBreakConditions = actionSequenceSO.m_DefaultBreakConditions;
    }

    public EnemyActionWrapper GetActionToBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_Index >= m_Sequence.Count)
            m_Index = 0;

        return m_Sequence[m_Index].m_Action.GetActionToBePerformed(enemyUnit, mapLogic);
    }

    public bool IsCompleted()
    {
        return m_Index >= m_Sequence.Count;
    }

    public void Reset()
    {
        if (m_Index < m_Sequence.Count)
            m_Sequence[m_Index].m_Action.Reset();
        m_Index = 0;
    }

    public void Run(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        m_Sequence[m_Index].m_Action.Run(enemyUnit, mapLogic, completeActionEvent);
        if (m_Sequence[m_Index].m_Action.IsCompleted())
        {
            ++m_Index;   
        }
    }

    public bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_DefaultBreakConditions.Any(x => x.IsConditionMet(enemyUnit, mapLogic)))
            return true;

        if (!IsCompleted() && m_Sequence[m_Index].ShouldBreakOut(enemyUnit, mapLogic))
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
