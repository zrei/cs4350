using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InflictedToken : IStatus
{
    public TokenTierSO m_TokenTierData;
    [Tooltip("Which tier should be inflicted")]
    public int m_Tier = 1;
    [Tooltip("Number of this token to inflict at once")]
    public int m_Number = 1;
    [HideInInspector]
    public bool m_IsPermanent = false;

    public int Id => m_TokenTierData.m_Id;
    public TokenType TokenType => m_TokenTierData.TokenType;

    #region IStatus
    public Sprite Icon => m_TokenTierData.m_Icon;
    public Color Color => m_TokenTierData.m_Color;
    public string DisplayTier => TokenUtil.NumToRomanNumeral(m_Tier);
    public string DisplayStacks => $"<size=50%>x</size>{m_Number}<sprite name=\"Stack\">";
    public string Name => m_TokenTierData.m_TokenName;
    public string Description => m_TokenTierData.m_Description;
    public List<int> NumStacksPerTier => null;
    public int CurrentHighestTier => m_Tier;
    #endregion

    public override string ToString()
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(m_TokenTierData.m_Color)}>{m_Number}x {m_TokenTierData}{TokenUtil.NumToRomanNumeral(m_Tier)}</color>";
    }
}

/*
[System.Serializable]
public class Token :
    IStatChange,
    IStatus
{
    [SerializeField] TokenSO m_TokenData;

    // represents different things for different token types

    public TokenType TokenType => m_TokenData.TokenType;

    public Sprite Icon => m_TokenData.m_Icon;
    public Color Color => m_TokenData.m_Color;
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