using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetTileWithinCol", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/TeleportTileConditions/TeleportTargetTileWithinCol")]
public class TeleportTargetTileWithinCol : EnemyTeleportTileConditionSO
{
    public List<int> m_Cols;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair teleportTargetTile, CoordPair initialTarget)
    {
        return m_Cols.Contains(teleportTargetTile.m_Col);
    }
}
