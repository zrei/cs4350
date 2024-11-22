using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinRowRangeOfAttackerRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinRowRangeOfAttackerRuleSO")]
public class TeleportTargetWithinRowRangeOfAttackerRuleSO : TeleportRuleSO
{
    public int m_RowDistance;

    public override bool IsValidTeleportTile(GridType targetGridType, CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return GridDistanceHelper.CalculateRowDistance(GridHelper.GetSameSide(attackingUnit.UnitAllegiance), targetGridType, attackingUnit.CurrPosition, targetTile) <= m_RowDistance;
    }
}
