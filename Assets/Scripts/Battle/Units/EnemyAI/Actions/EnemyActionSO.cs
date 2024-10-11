using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyActionSO : ScriptableObject
{
    public abstract EnemyActionWrapper GetWrapper(int priority);
}

[System.Serializable]
public struct EnemyTileCondition
{
    public EnemyTileConditionSO m_Condition;
    public float m_MultProportion;
    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile) => m_Condition.IsConditionMet(enemyUnit, mapLogic, targetTile);
}

public abstract class EnemyTargetActionSO : EnemyActionSO
{
    public List<EnemyTileCondition> m_TargetConditions;

    public float GetFinalWeightProportionForTile(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair target)
    {
        float finalNodeWeight = 1f;

        foreach (EnemyTileCondition targetCondition in m_TargetConditions)
        {
            if (targetCondition.IsConditionMet(enemyUnit, mapLogic, target))
                finalNodeWeight *= targetCondition.m_MultProportion;
        }

        return finalNodeWeight;
    }

}

[System.Serializable]
public struct EnemyAction
{
    public EnemyActionSO m_EnemyAction;
    // public float m_InitialWeight;
    public List<EnemyActionCondition> m_Conditions;
    [Tooltip("This priority is taken into account when no condition is met at all. Higher number will mean higher priority.")]
    public int m_BasePriority;

    public EnemyActionWrapper EnemyActionWrapper => m_EnemyAction.GetWrapper(m_BasePriority);

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
