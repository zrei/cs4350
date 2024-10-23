using UnityEngine;

[CreateAssetMenu(fileName = "MoveTileColComparisonSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/TileComparers/MoveTileComparers/MoveTileColComparisonSO")]
public class MoveTileColComparisonSO : EnemyMoveTileComparerSO
{
    public bool m_IsDescending = false;

    public override float GetTileValue(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile)
    {
        return m_IsDescending ? -targetTile.m_Col : targetTile.m_Col;
    }
}
