using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinRangeOfTargetRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinRangeOfTargetRuleSO")]
public class TeleportTargetWithinRangeOfTargetRuleSO : TeleportRuleSO
{
    public RangeDefinition m_AllowedRange;

    public override bool IsValidTeleportTile(GridType targetGridType, CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return m_AllowedRange.IsWithinRange(targetGridType, targetGridType, initialTarget, targetTile);
    }
}
