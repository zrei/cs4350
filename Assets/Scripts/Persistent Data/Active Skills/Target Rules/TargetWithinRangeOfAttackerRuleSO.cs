using UnityEngine;

[CreateAssetMenu(fileName = "TargetWithinRangeOfAttackerRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetWithinRangeOfAttackerRuleSO")]
public class TargetWithinRangeOfAttackerRuleSO : TargetLocationRuleSO
{
    [Header("NOTE THAT THIS IS ONLY VALID FOR SAME-SIDE TARGETTING RULES")]
    public RangeDefinition m_AllowedRange;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return m_AllowedRange.IsWithinRange(targetTile, attackingUnit.CurrPosition);
    }
}
