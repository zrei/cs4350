using System;
using UnityEngine;

public class Token
{
    [SerializeField] TokenSO m_TokenData;
    private int m_TokenId;
    private int m_Tier;
}

public class TauntToken : Token
{
    private Unit m_TauntedUnit;
}

/*
[System.Serializable]
public class Token :
    IStatChange,
    IStatus
{
    private static readonly Color InflictStatusColor = new(0, 0.8f, 0, 1);
    private static readonly Color StatChangeColor = new(0.5f, 0, 1, 1);
    private static readonly Color SupportEffectUpColor = new(1, 0.87f, 0, 1);

    [SerializeField] TokenSO m_TokenData;

    // represents different things for different token types

    public TokenType TokenType => m_TokenData.TokenType;

    public Sprite Icon => m_TokenData.m_Icon;
    public Color Color => TokenType switch
    {
        TokenType.INFLICT_STATUS => InflictStatusColor,
        TokenType.STAT_CHANGE => StatChangeColor,
        TokenType.SUPPORT_EFFECT_UP => SupportEffectUpColor,
        _ => Color.white,
    };
    public string DisplayAmount => !string.IsNullOrEmpty(m_TokenData.m_DisplayAmountFormat) 
        ? string.Format(m_TokenData.m_DisplayAmountFormat, m_Amount)
        : string.Empty;
    public string Name => m_TokenData.m_TokenName;
    public string Description => m_TokenData.m_Description;

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
        statusEffect = new StatusEffect(statusEffectTokenSO.m_StatusEffect, (int) m_Amount);
        return true;
    }
}
*/