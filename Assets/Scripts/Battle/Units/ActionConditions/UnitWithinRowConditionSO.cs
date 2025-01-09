using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitWithinRowConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/UnitWithinRowConditionSO")]
public class UnitWithinRowConditionSO : ActionConditionSO
{
    public List<int> m_Rows;
    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ m_Rows.Contains(unit.CurrPosition.m_Row);
    }
}
