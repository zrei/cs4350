using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinRowRangeOfTargetRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinRowRangeOfTargetRuleSO")]
public class TeleportTargetWithinRowRangeOfTargetRuleSO : TeleportRuleSO
{
    public int m_RowDistance;

    public override bool IsValidTeleportTile(GridType targetGridType, CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return GridDistanceHelper.CalculateRowDistance(targetGridType, targetGridType, attackingUnit.CurrPosition, targetTile) <= m_RowDistance;
    }
}
