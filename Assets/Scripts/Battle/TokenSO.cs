using UnityEngine;

public struct Token 
{
    public TokenSO m_TokenData;

    // represents different things for different token types
    public float m_Amount;
}

public enum TokenType
{
    INFLICT_STATUS,
    FLAT_STAT_CHANGE,
    MULT_STAT_CHANGE,
    SUPPORT_EFFECT_UP
}

public enum ConsumeType
{
    CONSUME_ON_SUPPORT,
    CONSUME_ON_MAG_ATTACK,
    CONSUME_ON_PHYS_ATTACK,
    CONSUME_ON_MAG_DEFEND,
    CONSUME_ON_PHYS_DEFEND
}

public abstract class TokenSO : ScriptableObject
{
    public string m_TokenName;
    public string m_Description;
    public Sprite m_Icon;
    public TokenType m_TokenType;
    [Tooltip("When to consume this token")]
    public ConsumeType m_Consumption;
}

public class StatusEffectTokenSO : TokenSO
{
    public StatusEffectType m_StatusToCast;
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

public class StatChangeTokenSO : TokenSO
{
    public StatType m_AffectedStat;
}