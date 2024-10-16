using UnityEngine;

// this one's flat
public class UnitHealthThresholdConditionSO : EnemyActionConditionSO
{
    public float m_HealthThreshold;
    [Tooltip("Whether to check if the unit's health is greater or less than the threshold")]
    public bool m_GreaterThan;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_GreaterThan)
        {
            return enemyUnit.CurrentHealth > m_HealthThreshold;
        }
        else
        {
            return enemyUnit.CurrentHealth < m_HealthThreshold;
        }
    }
}
