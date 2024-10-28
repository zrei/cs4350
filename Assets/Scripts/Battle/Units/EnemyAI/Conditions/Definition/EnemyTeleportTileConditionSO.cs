using UnityEngine;

public abstract class EnemyTeleportTileConditionSO : ScriptableObject
{
    public abstract bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair teleportTargetTile, CoordPair initialTarget);
}
