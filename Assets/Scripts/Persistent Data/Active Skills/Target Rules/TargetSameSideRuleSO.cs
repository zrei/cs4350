using UnityEngine;

[CreateAssetMenu(fileName = "TargetSameSideRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetSameSideRuleSO")]
public class TargetSameSideRuleSO : SkillTargetRuleSO
{
    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return GridHelper.IsSameSide(attackingUnit.UnitAllegiance, targetGridType);
    }
}
