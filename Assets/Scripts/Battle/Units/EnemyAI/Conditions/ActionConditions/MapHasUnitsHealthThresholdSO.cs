using UnityEngine;

// this one's not implemented yet! supposed to be has any unit beating the threshold
public class MapHasUnitsHealthThresholdSO : EnemyActionConditionSO
{
    public GridType m_GridType;
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
