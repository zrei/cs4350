using UnityEngine;

[CreateAssetMenu(fileName = "TargetWithinColRangeOfAttackerRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetWithinColRangeOfAttackerRuleSO")]
public class TargetWithinColRangeOfAttackerRuleSO : TargetLocationRuleSO
{
    [Header("This is valid for both types of targetting - it will work by counting the col distance by visual")]
    public int m_ColDistance;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return GridDistanceHelper.CalculateColDistance(GridHelper.GetSameSide(attackingUnit.UnitAllegiance), targetGridType, attackingUnit.CurrPosition, targetTile) <= m_ColDistance;
    }
}
