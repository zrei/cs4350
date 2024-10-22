using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MovableTileWithinRowConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/MoveTileConditions/MovableTileWithinRowConditionSO")]
public class MovableTileWithinRowConditionSO : EnemyMoveTileConditionSO
{
    public List<int> m_Rows;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile)
    {
        return m_Rows.Contains(targetTile.m_Row);
    }
}
