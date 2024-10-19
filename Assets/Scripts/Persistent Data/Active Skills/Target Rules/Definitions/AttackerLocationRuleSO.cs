public abstract class AttackerLocationRuleSO : LocationTargetRuleSO, IAttackerRule {
    public abstract bool IsValidAttackerTile(CoordPair attackerPosition);
}
