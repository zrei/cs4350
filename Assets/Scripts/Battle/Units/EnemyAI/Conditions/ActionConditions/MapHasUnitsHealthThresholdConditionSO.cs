using UnityEngine;

[CreateAssetMenu(fileName = "MapHasUnitsHealthThresholdConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/MapHasUnitsHealthThresholdConditionSO")]
public class MapHasUnitsHealthThresholdConditionSO : EnemyActionConditionSO
{
    public GridType m_GridType;
    public Threshold m_HealthThreshold;
    [Tooltip("Whether this is checking the flat health amounts or not")]
    public bool m_IsFlat;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return mapLogic.HasAnyUnitWithHealthThreshold(m_GridType, m_HealthThreshold, m_IsFlat);
    }
}
