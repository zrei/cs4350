// can change to game object later
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviourTreeSO", menuName = "ScriptableObject/BehaviourTreeSO")]
public class BehaviourTreeSO : ActionSO
{
    [Tooltip("Priority is just in the order you put it")]
    public List<ActionSO> m_Actions;
    [Tooltip("Conditions that can affect priority, listed in order of priority. It will be checked in this order.")]
    public List<EnemyActionCondition> m_PriorityChangeConditions;

    public override IConcreteAction GenerateConcreteAction()
    {
        return new BehaviourTree(this);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BehaviourTreeSO))]
public class BehaviourTreeSOEditor : Editor
{
    BehaviourTreeSO m_Target;

    private void OnEnable()
    {
        m_Target = (BehaviourTreeSO) target;
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
            else if (enemyActionCondition.m_ActionIndex < 0 || enemyActionCondition.m_ActionIndex >= m_Target.m_Actions.Count)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label($"{i + 1}. Condition {enemyActionCondition.m_Condition.name} applied to an invalid action index");
            }
            else
            {
                GUI.contentColor = Color.black;
                GUILayout.Label($"{i + 1}. Condition {enemyActionCondition.m_Condition.name} applied to {m_Target.m_Actions[enemyActionCondition.m_ActionIndex].name}");
            }
        }
    }
}
#endif

public class BehaviourTree : IConcreteAction
{
    private List<IConcreteAction> m_IndividualLeaves;
    private List<EnemyActionCondition> m_PriorityChangeConditions;

    private int m_CurrActionIndex = -1;

    private int m_PrevActionIndex = -1;

    public BehaviourTree(BehaviourTreeSO behaviourTreeSO)
    {
        m_IndividualLeaves = behaviourTreeSO.m_Actions.Select(x => x.GenerateConcreteAction()).ToList();
        m_PriorityChangeConditions = behaviourTreeSO.m_PriorityChangeConditions;
    }

    public EnemyActionWrapper GetActionToBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_CurrActionIndex == -1 || m_IndividualLeaves[m_CurrActionIndex].IsCompleted() || m_IndividualLeaves[m_CurrActionIndex].ShouldBreakOut(enemyUnit, mapLogic))
        {
            m_PrevActionIndex = m_CurrActionIndex;
            m_CurrActionIndex = ChooseAction(enemyUnit, mapLogic);
        }
        return m_IndividualLeaves[m_CurrActionIndex].GetActionToBePerformed(enemyUnit, mapLogic);
    }

    public bool IsCompleted()
    {
        return m_CurrActionIndex != -1 && m_IndividualLeaves[m_CurrActionIndex].IsCompleted();
    }

    public void Reset()
    {
        if (m_PrevActionIndex != -1 && m_PrevActionIndex != m_CurrActionIndex)
        {
            m_IndividualLeaves[m_PrevActionIndex].Reset();
        }
        m_PrevActionIndex = -1;
    }

    public void Run(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent onCompleteEvent)
    { 
        Reset();
        m_IndividualLeaves[m_CurrActionIndex].Run(enemyUnit, mapLogic, onCompleteEvent);
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
        foreach (IConcreteAction concreteAction in m_IndividualLeaves)
        {
            nestedActiveSkills = new HashSet<ActiveSkillSO>(nestedActiveSkills.Union(concreteAction.GetNestedActiveSkills()));
        }
        return nestedActiveSkills;
    }
}

[System.Serializable]
public struct EnemyActionCondition
{
    public EnemyActionConditionSO m_Condition;
    [Tooltip("The index of the tree this condition corresponds to")]
    public int m_ActionIndex;

    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic) => m_Condition.IsConditionMet(enemyUnit, mapLogic);
}
