using UnityEngine;

[System.Serializable]
public class Token : IStatChange
{
    [SerializeField] TokenSO m_TokenData;

    // represents different things for different token types
    [SerializeField] float m_Amount;

    public TokenType TokenType => m_TokenData.TokenType;

    public float GetFlatStatChange(StatType statType)
    {
        if (m_TokenData.TokenType != TokenType.STAT_CHANGE)
            return 0;

        StatChangeTokenSO statChangeTokenSO = (StatChangeTokenSO) m_TokenData;

        if (statChangeTokenSO.m_StatChangeType != StatChangeType.FLAT)
            return 0;
        
        if (statChangeTokenSO.m_AffectedStat != statType)
            return 0;
        
        return m_Amount;
    }

    public float GetMultStatChange(StatType statType)
    {
         if (m_TokenData.TokenType != TokenType.STAT_CHANGE)
            return 1;

        StatChangeTokenSO statChangeTokenSO = (StatChangeTokenSO) m_TokenData;

        if (statChangeTokenSO.m_StatChangeType != StatChangeType.MULT)
            return 1;
        
        if (statChangeTokenSO.m_AffectedStat != statType)
            return 1;
        
        return m_Amount;
    }

    public bool ContainsConsumptionType(ConsumeType consumeType)
    {
        return m_TokenData.ContainsConsumptionType(consumeType);
    }

    public bool TryGetInflictedStatusEffect(out StatusEffect statusEffect)
    {
        if (m_TokenData.TokenType != TokenType.INFLICT_STATUS)
        {
            statusEffect = null;
            return false;
        }

        StatusEffectTokenSO statusEffectTokenSO = (StatusEffectTokenSO) m_TokenData;
        statusEffect = new StatusEffect(statusEffectTokenSO.m_StatusEffect.m_StatusEffect, statusEffectTokenSO.m_StatusEffect.m_Stack);
        return true;
    }
}
