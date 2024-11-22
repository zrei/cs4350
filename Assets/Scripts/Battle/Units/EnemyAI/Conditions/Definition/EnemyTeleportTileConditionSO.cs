using UnityEngine;

public abstract class EnemyTeleportTileConditionSO : ScriptableObject
{
    public abstract bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, GridType targetGridType, CoordPair teleportTargetTile, CoordPair initialTarget);
}
