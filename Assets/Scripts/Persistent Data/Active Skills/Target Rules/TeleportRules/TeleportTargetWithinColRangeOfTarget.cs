using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinColRangeOfTargetRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinColRangeOfTargetRuleSO")]
public class TeleportTargetWithinColRangeOfTargetRuleSO : TeleportRuleSO
{
    public int m_ColDistance;

    public override bool IsValidTeleportTile(GridType targetGridType, CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return GridDistanceHelper.CalculateColDistance(targetGridType, targetGridType, attackingUnit.CurrPosition, targetTile) <= m_ColDistance;
    }
}
