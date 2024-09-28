using UnityEngine;

[CreateAssetMenu(fileName = "LockToSelfTargetRuleSO", menuName = "ScriptableObject/Classes/ActiveSkills/TargetRules/LockToSelfTargetRuleSO")]
public class LockToSelfTargetRuleSO : SkillTargetRuleSO
{
    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return GridHelper.IsSameSide(attackingUnit.UnitAllegiance, targetGridType) && attackingUnit.CurrPosition.Equals(targetTile);
    }
}
