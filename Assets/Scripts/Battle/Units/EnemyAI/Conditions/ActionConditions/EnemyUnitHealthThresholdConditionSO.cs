using UnityEngine;

[CreateAssetMenu(fileName = "EnemyUnitHealthThresholdConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/EnemyUnitHealthThresholdConditionSO")]
public class EnemyUnitHealthThresholdConditionSO : EnemyActionConditionSO
{
    public Threshold m_HealthThreshold;
    [Tooltip("Whether this is checking the flat health amounts or not")]
    public bool m_ChecksFlat;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_HealthThreshold.IsSatisfied(m_ChecksFlat ? enemyUnit.CurrentHealth : enemyUnit.CurrentHealthProportion);
    }
}
