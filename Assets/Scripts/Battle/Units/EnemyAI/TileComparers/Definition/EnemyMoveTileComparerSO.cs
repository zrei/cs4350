using UnityEngine;

public abstract class EnemyMoveTileComparerSO : ScriptableObject
{
    public abstract float GetTileValue(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile);
}
