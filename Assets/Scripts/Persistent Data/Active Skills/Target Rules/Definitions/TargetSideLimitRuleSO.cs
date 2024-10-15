public abstract class TargetSideLimitRuleSO : SkillTargetRuleSO, ITargetRule {
    public abstract bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType);
}
