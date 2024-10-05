public interface IInflictStatus
{
    public bool TryGetInflictedStatusEffect(out StatusEffect statusEffect);
}

public interface ICritModifier
{
    public float GetFinalCritProportion();
}

public interface IFlatStatChange
{
    public float GetFlatStatChange(StatType statType);
}

public interface IMultStatChange {
    public float GetMultStatChange(StatType statType);
}

public interface IStat
{
    public float GetTotalStat(StatType statType, float baseModifier = 1f);
}

public interface ITauntTarget
{
    public bool IsTaunted(out Unit forceTarget);
}
