using UnityEngine;

[CreateAssetMenu(fileName = "UnitHealthThresholdConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/UnitHealthThresholdConditionSO")]
public class UnitHealthThresholdConditionSO : ActionConditionSO
{
    public Threshold m_HealthThreshold;
    [Tooltip("Whether this is checking the flat health amounts or not")]
    public bool m_ChecksFlat;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ m_HealthThreshold.IsSatisfied(m_ChecksFlat ? unit.CurrentHealth : unit.CurrentHealthProportion);
    }
}
