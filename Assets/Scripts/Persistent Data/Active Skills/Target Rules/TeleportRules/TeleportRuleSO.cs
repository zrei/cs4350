using UnityEngine;

public abstract class TeleportRuleSO : ScriptableObject
{
    public abstract bool IsValidTeleportTile(CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit);
}
