using UnityEngine;

[System.Serializable]
public class Token : IStatChange
{
    public TokenSO m_TokenData;

    // represents different things for different token types
    public float m_Amount;

    public float GetFlatStatChange(StatType statType)
    {
        if (m_TokenData.m_TokenType == TokenType.FLAT_STAT_CHANGE && ((StatChangeTokenSO) m_TokenData).m_AffectedStat == statType)
            return m_Amount;
        else
            return 0;
    }

    public float GetMultStatChange(StatType statType)
    {
        if (m_TokenData.m_TokenType == TokenType.FLAT_STAT_CHANGE && ((StatChangeTokenSO) m_TokenData).m_AffectedStat == statType)
            return m_Amount;
        else
            return 1;
    }

    /*
    public bool TryGetInflictedStatusEffect(out StatusEffect statusEffect)
    {
        if (m_TokenData.m_TokenType != TokenType.INFLICT_STATUS)
        {
            statusEffect = null;
            return false;
        }

        statusEffect = new StatusEffect();
    }
    */
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
    public ConsumeType[] m_Consumption;
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