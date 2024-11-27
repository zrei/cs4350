using UnityEngine;

[CreateAssetMenu(fileName = "TargetWithinRowRangeOfAttackerRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetWithinRowRangeOfAttackerRuleSO")]
public class TargetWithinRowRangeOfAttackerRuleSO : TargetLocationRuleSO
{
    [Header("This is valid for both types of targetting - it will work by counting the row distance by visual")]
    public int m_RowDistance;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return GridDistanceHelper.CalculateRowDistance(GridHelper.GetSameSide(attackingUnit.UnitAllegiance), targetGridType, attackingUnit.CurrPosition, targetTile) <= m_RowDistance;
    }
}
