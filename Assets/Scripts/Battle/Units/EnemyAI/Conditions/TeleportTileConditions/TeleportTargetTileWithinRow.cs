using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetTileWithinRow", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/TeleportTileConditions/TeleportTargetTileWithinRow")]
public class TeleportTargetTileWithinRow : EnemyTeleportTileConditionSO
{
    public List<int> m_Rows;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair teleportTargetTile, CoordPair initialTarget)
    {
        return m_Rows.Contains(teleportTargetTile.m_Row);
    }
}
