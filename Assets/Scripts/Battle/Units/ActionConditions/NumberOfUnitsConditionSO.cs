using UnityEngine;

[CreateAssetMenu(fileName = "NumberOfUnitsConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/NumberOfUnitsConditionSO")]
public class NumberOfUnitsConditionSO : ActionConditionSO 
{
    public Threshold m_NumberUnitsThreshold;
    public GridType m_GridType;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ m_NumberUnitsThreshold.IsSatisfied(mapLogic.GetNumberOfUnitsOnGrid(m_GridType));
    }
}
