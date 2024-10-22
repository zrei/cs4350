using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MovableTileWithinColConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/MoveTileConditions/MovableTileWithinColConditionSO")]
public class MovableTileWithinColConditionSO : EnemyMoveTileConditionSO
{
    public List<int> m_Cols;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile)
    {
        return m_Cols.Contains(targetTile.m_Col);
    }
}
