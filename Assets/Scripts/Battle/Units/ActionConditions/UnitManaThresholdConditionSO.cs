using UnityEngine;

[CreateAssetMenu(fileName = "UnitManaThresholdConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/UnitManaThresholdConditionSO")]
public class UnitManaThresholdConditionSO : ActionConditionSO
{
    public Threshold m_ManaThreshold;
    [Tooltip("Whether this is checking the flat mana amounts or not")]
    public bool m_ChecksFlat;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ m_ManaThreshold.IsSatisfied(m_ChecksFlat ? unit.CurrentMana : unit.CurrentManaProportion);
    }
}
