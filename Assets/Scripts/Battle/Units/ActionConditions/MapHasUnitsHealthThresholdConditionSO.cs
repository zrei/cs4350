using UnityEngine;

[CreateAssetMenu(fileName = "MapHasUnitsHealthThresholdConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/MapHasUnitsHealthThresholdConditionSO")]
public class MapHasUnitsHealthThresholdConditionSO : ActionConditionSO
{
    public GridType m_GridType;
    public Threshold m_HealthThreshold;
    [Tooltip("Whether this is checking the flat health amounts or not")]
    public bool m_IsFlat;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ mapLogic.HasAnyUnitWithHealthThreshold(m_GridType, m_HealthThreshold, m_IsFlat);
    }
}
