using UnityEngine;

[CreateAssetMenu(fileName = "MapHasUnitsManaThresholdConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/MapHasUnitsManaThresholdConditionSO")]
public class MapHasUnitsManaThresholdConditionSO : ActionConditionSO
{
    public GridType m_GridType;
    public Threshold m_ManaThreshold;
    [Tooltip("Whether this is checking the flat mana amounts or not")]
    public bool m_IsFlat;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ mapLogic.HasAnyUnitWithManaThreshold(m_GridType, m_ManaThreshold, m_IsFlat);
    }
}
