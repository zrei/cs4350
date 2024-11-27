using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinColRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinColRuleSO")]
public class TeleportTargetWithinColRuleSO : TeleportRuleSO
{
    public List<int> m_Cols;

    public override bool IsValidTeleportTile(GridType targetGridType, CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return m_Cols.Contains(targetTile.m_Col);
    }
}
