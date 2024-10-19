public interface ITargetRule
{
    public bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType);
}

public interface IAttackerRule
{
    public bool IsValidAttackerTile(CoordPair unitPosition);
}
