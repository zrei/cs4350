using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetTileWithinAttackerRange", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/TeleportTileConditions/TeleportTargetTileWithinAttackerRange")]
public class TeleportTargetTileWithinAttackerRange : EnemyTeleportTileConditionSO
{
    public RangeDefinition m_AllowedRange;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair teleportTargetTile, CoordPair initialTarget)
    {
        return m_AllowedRange.IsWithinRange(enemyUnit.CurrPosition, teleportTargetTile);
    }
}
