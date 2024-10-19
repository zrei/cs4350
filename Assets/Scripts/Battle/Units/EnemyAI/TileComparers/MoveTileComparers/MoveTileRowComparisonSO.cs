using UnityEngine;

[CreateAssetMenu(fileName = "MoveTileRowComparisonSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/TileComparers/MoveTileComparers/MoveTileRowComparisonSO")]
public class MoveTileRowComparisonSO : EnemyMoveTileComparerSO
{
    public bool m_IsDescending = false;

    public override float GetTileValue(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile)
    {
        return m_IsDescending ? -targetTile.m_Row : targetTile.m_Row;
    }
}
