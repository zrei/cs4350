using UnityEngine;

[CreateAssetMenu(fileName = "TargetOpposingSideRuleSO", menuName = "ScriptableObject/Classes/ActiveSkills/TargetRules/TargetOpposingSideRuleSO")]
public class TargetOpposingSideRuleSO : SkillTargetRuleSO
{
    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return GridHelper.IsOpposingSide(attackingUnit.UnitAllegiance, targetGridType);
    }
}
