using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinRangeOfTargetRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinRangeOfTargetRuleSO")]
public class TeleportTargetWithinRangeOfTargetRuleSO : TeleportRuleSO
{
    public RangeDefinition m_AllowedRange;

    public override bool IsValidTeleportTile(CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return m_AllowedRange.IsWithinRange(initialTarget, targetTile);
    }
}
