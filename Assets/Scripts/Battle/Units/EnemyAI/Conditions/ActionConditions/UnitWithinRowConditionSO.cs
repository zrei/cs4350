using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitWithinRowConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/UnitWithinRowConditionSO")]
public class UnitWithinRowConditionSO : EnemyActionConditionSO
{
    public List<int> m_Rows;
    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_Rows.Contains(enemyUnit.CurrPosition.m_Row);
    }
}
