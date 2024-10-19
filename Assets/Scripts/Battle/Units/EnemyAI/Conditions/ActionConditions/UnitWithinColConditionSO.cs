using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitWithinColConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/UnitWithinColConditionSO")]
public class UnitWithinColConditionSO : EnemyActionConditionSO
{
    public List<int> m_Cols;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_Cols.Contains(enemyUnit.CurrPosition.m_Col);
    }
}
