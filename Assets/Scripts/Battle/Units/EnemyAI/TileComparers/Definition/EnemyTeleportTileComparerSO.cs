using UnityEngine;

public abstract class EnemyTeleportTileComparerSO : ScriptableObject
{
    public abstract float GetTileValue(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair teleportTargetTile, CoordPair teleportStartTile, GridType teleportTargetGrid);
}
