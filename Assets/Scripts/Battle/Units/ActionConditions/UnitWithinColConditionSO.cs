using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitWithinColConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/UnitWithinColConditionSO")]
public class UnitWithinColConditionSO : ActionConditionSO
{
    public List<int> m_Cols;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ m_Cols.Contains(unit.CurrPosition.m_Col);
    }
}
