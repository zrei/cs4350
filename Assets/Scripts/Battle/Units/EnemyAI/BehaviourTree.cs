// can change to game object later
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BehaviourTree : EnemyAction
{
    [Tooltip("Priority is just in the order you put it")]
    public List<BehaviourTreeAction> m_Actions;
    [Tooltip("Conditions that can disrupt pre-set priority. It will be checked in this order.")]
    public List<EnemyActionCondition> m_PriorityChangeConditions;

    public override IConcreteAction GenerateConcreteAction()
    {
        return new BehaviourTreeRuntimeInstance(this);
    }
}

[System.Serializable]
public struct BehaviourTreeAction 
{
    public EnemyAction m_Action;
    [Tooltip("Additional Conditions that need to be fulfilled for this action to be allowed")]
    public List<ActionConditionSO> m_AdditionalConditions;
}

public struct ConcreteBehaviourTreeAction
{
    public IConcreteAction m_Action;
    public List<ActionConditionSO> m_AdditionalConditions;

    public ConcreteBehaviourTreeAction(IConcreteAction concreteAction, List<ActionConditionSO> additionalConditions)
    {
        m_Action = concreteAction;
        m_AdditionalConditions = additionalConditions;
    }

    public bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_AdditionalConditions.Any(x => !x.IsConditionMet(enemyUnit, mapLogic)) || m_Action.ShouldBreakOut(enemyUnit, mapLogic); 
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BehaviourTree))]
public class BehaviourTreeEditor : Editor
{
    BehaviourTree m_Target;

    private void OnEnable()
    {
        m_Target = (BehaviourTree) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20f);

        for (int i = 0; i < m_Target.m_PriorityChangeConditions.Count; ++i)
        {
            EnemyActionCondition enemyActionCondition = m_Target.m_PriorityChangeConditions[i];
            if (enemyActionCondition.m_Condition == null)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label($"{i + 1}. Condition not set");
            }
            else if (enemyActionCondition.m_ActionIndex < 0 || enemyActionCondition.m_ActionIndex >= m_Target.m_Actions.Count || m_Target.m_Actions[enemyActionCondition.m_ActionIndex].m_Action == null)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label($"{i + 1}. Condition {enemyActionCondition.m_Condition.name} applied to an invalid action index");
            }
            else
            {
                GUI.contentColor = Color.black;
                GUILayout.Label($"{i + 1}. Condition {enemyActionCondition.m_Condition.name} applied to {m_Target.m_Actions[enemyActionCondition.m_ActionIndex].m_Action.name}");
            }
        }
    }
}
#endif

public class BehaviourTreeRuntimeInstance : IConcreteAction
{
    private List<ConcreteBehaviourTreeAction> m_IndividualLeaves;
    private List<EnemyActionCondition> m_PriorityChangeConditions;

    private int m_CurrActionIndex = -1;

    private int m_PrevActionIndex = -1;

    public BehaviourTreeRuntimeInstance(BehaviourTree behaviourTree)
    {
        Debug.Log(behaviourTree == null);
        m_IndividualLeaves = behaviourTree.m_Actions.Select(x => new ConcreteBehaviourTreeAction(x.m_Action.GenerateConcreteAction(), x.m_AdditionalConditions)).ToList();
        m_PriorityChangeConditions = behaviourTree.m_PriorityChangeConditions;
    }

    public EnemyActionWrapper GetActionToBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_CurrActionIndex == -1 || m_IndividualLeaves[m_CurrActionIndex].m_Action.IsCompleted(enemyUnit, mapLogic) || m_IndividualLeaves[m_CurrActionIndex].ShouldBreakOut(enemyUnit, mapLogic))
        {
            m_PrevActionIndex = m_CurrActionIndex;
            m_CurrActionIndex = ChooseAction(enemyUnit, mapLogic);
        }
        return m_IndividualLeaves[m_CurrActionIndex].m_Action.GetActionToBePerformed(enemyUnit, mapLogic);
    }

    public bool IsCompleted(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_CurrActionIndex != -1 && m_IndividualLeaves[m_CurrActionIndex].m_Action.IsCompleted(enemyUnit, mapLogic);
    }

    public void Reset()
    {
        if (m_PrevActionIndex != -1 && m_PrevActionIndex != m_CurrActionIndex)
        {
            m_IndividualLeaves[m_PrevActionIndex].m_Action.Reset();
        }
        m_PrevActionIndex = -1;
    }

    public void Run(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent onCompleteEvent)
    { 
        Reset();
        if (m_CurrActionIndex == -1)
            GetActionToBePerformed(enemyUnit, mapLogic);
        m_IndividualLeaves[m_CurrActionIndex].m_Action.Run(enemyUnit, mapLogic, onCompleteEvent);
        m_PrevActionIndex = m_CurrActionIndex;
    }

    public bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_IndividualLeaves.All(x => x.ShouldBreakOut(enemyUnit, mapLogic));
    }

    private int ChooseAction(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        foreach (EnemyActionCondition enemyActionCondition in m_PriorityChangeConditions)
        {
            // check if action can be performed also! Maybe make a hashset... to avoid running this so many times
            if (enemyActionCondition.IsConditionMet(enemyUnit, mapLogic) && !m_IndividualLeaves[enemyActionCondition.m_ActionIndex].ShouldBreakOut(enemyUnit, mapLogic))
            {
                return enemyActionCondition.m_ActionIndex;
            }
        }

        for (int i = 0; i < m_IndividualLeaves.Count; ++i)
        {
            if (!m_IndividualLeaves[i].ShouldBreakOut(enemyUnit, mapLogic))
                return i;
        }

        return 0;
    }

    public HashSet<ActiveSkillSO> GetNestedActiveSkills()
    {
        HashSet<ActiveSkillSO> nestedActiveSkills = new();
        foreach (ConcreteBehaviourTreeAction concreteAction in m_IndividualLeaves)
        {
            nestedActiveSkills = new HashSet<ActiveSkillSO>(nestedActiveSkills.Union(concreteAction.m_Action.GetNestedActiveSkills()));
        }
        return nestedActiveSkills;
    }
}

[System.Serializable]
public struct EnemyActionCondition
{
    public ActionConditionSO m_Condition;
    [Tooltip("The index of the tree this condition corresponds to")]
    public int m_ActionIndex;

    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic) => m_Condition.IsConditionMet(enemyUnit, mapLogic);
}
