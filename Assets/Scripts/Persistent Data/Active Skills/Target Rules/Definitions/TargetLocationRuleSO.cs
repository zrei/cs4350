public abstract class TargetLocationRuleSO : LocationTargetRuleSO, ITargetRule {
    public abstract bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType);
}
