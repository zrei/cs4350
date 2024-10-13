using UnityEngine;

[CreateAssetMenu(fileName = "LockToSelfTargetRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/LockToSelfTargetRuleSO")]
public class LockToSelfTargetRuleSO : TargetSideLimitRuleSO
{
    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return GridHelper.IsSameSide(attackingUnit.UnitAllegiance, targetGridType) && attackingUnit.CurrPosition.Equals(targetTile);
    }
}
