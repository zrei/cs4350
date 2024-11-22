using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinColRangeOfAttackerRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinColRangeOfAttackerRuleSO")]
public class TeleportTargetWithinColRangeOfAttackerRuleSO : TeleportRuleSO
{
    public int m_ColDistance;

    public override bool IsValidTeleportTile(GridType targetGridType, CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return GridDistanceHelper.CalculateColDistance(GridHelper.GetSameSide(attackingUnit.UnitAllegiance), targetGridType, attackingUnit.CurrPosition, targetTile) <= m_ColDistance;
    }
}
