using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinRowRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinRowRuleSO")]
public class TeleportTargetWithinRowRuleSO : TeleportRuleSO
{
    public List<int> m_Rows;

    public override bool IsValidTeleportTile(CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return m_Rows.Contains(targetTile.m_Row);
    }
}
