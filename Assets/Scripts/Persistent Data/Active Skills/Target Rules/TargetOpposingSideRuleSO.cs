using UnityEngine;

[CreateAssetMenu(fileName = "TargetOpposingSideRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetOpposingSideRuleSO")]
public class TargetOpposingSideRuleSO : TargetSideLimitRuleSO
{
    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return GridHelper.IsOpposingSide(attackingUnit.UnitAllegiance, targetGridType);
    }
}
