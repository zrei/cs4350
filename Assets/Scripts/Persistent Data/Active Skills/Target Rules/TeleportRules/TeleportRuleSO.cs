using UnityEngine;

public abstract class TeleportRuleSO : ScriptableObject
{
    public abstract bool IsValidTeleportTile(GridType targetGridType, CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit);
}
