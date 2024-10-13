using UnityEngine;

[CreateAssetMenu(fileName = "TargetRangeRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetRangeRuleSO")]
public class TargetRangeRuleSO : TargetLocationRuleSO
{
    public int m_AllowedTargetRange;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return GridHelper.IsSameSide(attackingUnit.UnitAllegiance, targetGridType) && attackingUnit.CurrPosition.GetDistanceToPoint(targetTile) <= m_AllowedTargetRange;
    }
}
