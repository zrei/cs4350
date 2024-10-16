using UnityEngine;

// this one's flat
public class UnitManaThresholdConditionSO : EnemyActionConditionSO
{
    public float m_ManaThreshold;
    [Tooltip("Whether to check if the unit's health is greater or less than the threshold")]
    public bool m_GreaterThan;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_GreaterThan)
        {
            return enemyUnit.CurrentMana > m_ManaThreshold;
        }
        else
        {
            return enemyUnit.CurrentMana < m_ManaThreshold;
        }
    }
}
