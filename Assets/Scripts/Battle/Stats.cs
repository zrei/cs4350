/// <summary>
/// Helps to store stats for different purposes.
/// </summary>
[System.Serializable]
public struct Stats
{
    // temporary, move to some character data class
    public string m_name;
    public string m_class;

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

    public float GetStat(StatType stat)
    {
        return stat switch
        {
            StatType.HEALTH => m_Health,
            StatType.MANA => m_Mana,
            StatType.PHYS_ATTACK => m_PhysicalAttack,
            StatType.MAG_ATTACK => m_MagicAttack,
            StatType.PHYS_DEFENCE => m_PhysicalDefence,
            StatType.MAG_DEFENCE => m_MagicDefence,
            StatType.SPEED => m_Speed,
            StatType.MOVEMENT_RANGE => m_MovementRange,
            _ => -1,
        };
    }

    public Stats(string name, string char_class, float health, float mana, float physicalAttack, float magicalAttack, float physicalDefence, float magicDefence, float speed, int movementRange, TileType[] traversableTileTypes, bool canSwapTiles)
    {
        m_name = name;
        m_class = char_class;
        m_Health = health;
        m_Mana = mana;
        m_PhysicalAttack = physicalAttack;
        m_MagicAttack = magicalAttack;
        m_PhysicalDefence = physicalDefence;
        m_MagicDefence = magicDefence;
        m_Speed = speed;
        m_MovementRange = movementRange;
        m_TraversableTileTypes = traversableTileTypes;
        m_CanSwapTiles = canSwapTiles;
    }
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

public interface IStat
{
    public float GetTotalStat(StatType statType);
}