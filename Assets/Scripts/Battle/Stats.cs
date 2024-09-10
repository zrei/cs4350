/// <summary>
/// Helps to store stats for different purposes.
/// </summary>
[System.Serializable]
public struct Stats
{
    public float m_Health;
    public float m_Mana;
    public float m_PhysicalAttack;
    public float m_MagicAttack;
    public float m_PhysicalDefence;
    public float m_MagicDefence;
    public float m_Speed;
    public int m_MovementRange;
    public TileType[] m_TraversableTileTypes;
    public bool m_CanSwapTiles;
}

public enum StatChangeType
{
    FLAT,
    MULT
}

public enum StatType
{
    HEALTH,
    MANA,
    PHYS_ATTACK,
    MAG_ATTACK,
    PHYS_DEFENCE,
    MAG_DEFENCE,
    SPEED,
    MOVEMENT_RANGE
}

public interface IStatChange
{
    public float GetFlatStatChange(StatType statType);

    public float GetMultStatChange(StatType statType);
}