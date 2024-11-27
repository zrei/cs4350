using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetTileWithinTargetRange", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/TeleportTileConditions/TeleportTargetTileWithinTargetRange")]
public class TeleportTargetTileWithinTargetRange : EnemyTeleportTileConditionSO
{
    public RangeDefinition m_AllowedRange;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, GridType targetGridType, CoordPair teleportTargetTile, CoordPair initialTarget)
    {
        return m_AllowedRange.IsWithinRange(targetGridType, targetGridType, initialTarget, teleportTargetTile);
    }
}
