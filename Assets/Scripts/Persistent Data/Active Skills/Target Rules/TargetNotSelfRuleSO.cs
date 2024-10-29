using UnityEngine;

[CreateAssetMenu(fileName = "TargetNotSelfRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetNotSelfRuleSO")]
public class TargetNotSelfRuleSO : TargetLocationRuleSO
{
    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return !targetTile.Equals(attackingUnit.CurrPosition);
    }
}
