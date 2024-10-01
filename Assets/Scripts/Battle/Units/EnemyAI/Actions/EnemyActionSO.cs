using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyActionSO : ScriptableObject
{
    public abstract bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic);

    public abstract void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent);
}

[System.Serializable]
public struct EnemyAction
{
    public EnemyActionSO m_EnemyAction;
    // public float m_InitialWeight;
    public List<EnemyActionCondition> m_Conditions;

    public bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic) => m_EnemyAction.CanActionBePerformed(enemyUnit, mapLogic);

    /*
    public float GetFinalWeight(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        float finalWeight = m_InitialWeight;
        foreach (EnemyActionCondition condition in m_WeightedConditions)
        {
        if (condition.IsConditionMet(enemyUnit, mapLogic))
            finalWeight *= condition.m_MultProportion;
        }
        return finalWeight;
    }
    */
}

[System.Serializable]
public struct EnemyActionCondition
{
    public EnemyActionConditionSO m_Condition;
    [Tooltip("The higher the number the greater priority this condition takes")]
    public int m_Priority;

    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic) => m_Condition.IsConidtionMet(enemyUnit, mapLogic);
}
