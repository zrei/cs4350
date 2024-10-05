using System.Collections.Generic;
using System.Linq;

public interface IInflictStatus
{
    public bool TryGetInflictedStatusEffect(out StatusEffect statusEffect);
}

public interface ICritModifier
{
    public float GetFinalCritProportion();
}

/// <summary>
/// An instance, in-battle, of a stack of tokens coming from the same group
/// </summary>
public class TokenStack : IFlatStatChange, IMultStatChange, IInflictStatus, ICritModifier
{
    #region Details
    private TokenTierSO m_TokenTierData;
    public bool AllowStack => m_TokenTierData.m_AllowStack;
    private int m_NumTiers;
    public int Id => m_TokenTierData.m_Id;
    public TokenType TokenType => m_TokenTierData.TokenType;
    public bool ContainsConsumptionType(TokenConsumptionType tokenConsumptionType) => m_TokenTierData.ContainsConsumptionType(tokenConsumptionType);
    #endregion

    #region Stack
    protected List<int> m_NumTokensOfEachTier;
    #endregion
    
    #region State
    public bool IsEmpty => m_NumTokensOfEachTier.All(x => x <= 0);
    private bool HasStack => m_NumTokensOfEachTier.Any(x => x > 0);
    #endregion

    public TokenStack(TokenTierSO tokenTier, int initialTier, int initialNumber = 1)
    {
        m_NumTiers = m_TokenTierData.NumTiers;
        m_NumTokensOfEachTier = new List<int>(m_NumTiers);
        for (int i = 0; i < tokenTier.NumTiers; ++i)
        {
            m_NumTokensOfEachTier.Add(0);
        }
        m_NumTokensOfEachTier[initialTier - 1] = initialNumber; 
    }

    /// <summary>
    /// Consume a single token, taking the highest tiered one first
    /// </summary>
    /// <returns></returns>
    public TokenSO GetToken()
    {
        int maxTier = GetMaxTier();
        if (maxTier > 0 && m_TokenTierData.TryRetreiveTier(maxTier, out TokenSO tokenSO))
        {
            return tokenSO;
        }
        return default;
    }

    public void ConsumeToken()
    {
        m_NumTokensOfEachTier[GetMaxTier() - 1]--;
    }

    private int GetMaxTier()
    {
        for (int i = m_NumTiers; i > 0; --i)
        {
            if (m_NumTokensOfEachTier[i - 1] > 0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Add a token of a tier belonging to this group, taking into account whether
    /// stacking is allowed
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="number"></param>
    public void AddToken(int tier, int number = 1)
    {
        if (!AllowStack && HasStack)
        {
            Logger.Log(this.GetType().Name, $"No stacking allowed for token tier {Id}", LogLevel.LOG);
            return;
        }
        
        m_NumTokensOfEachTier[tier - 1] += number;
    }

    public float GetMultStatChange(StatType statType)
    {
        if (TokenType == TokenType.MULT_STAT_CHANGE)
        {
            MultStatChangeTokenTierSO mult = (MultStatChangeTokenTierSO) m_TokenTierData;
            if (mult.m_AffectedStat != statType)
            {
                return 1f;
            }
            else
            {
                return mult.GetMultStatChange(GetMaxTier());
            }
        }
        else
        {
            return 1f;
        }
    }

    public float GetFlatStatChange(StatType statType)
    {
        if (TokenType == TokenType.FLAT_STAT_CHANGE)
        {
            FlatStatChangeTokenTierSO flat = (FlatStatChangeTokenTierSO) m_TokenTierData;
            if (flat.m_AffectedStat != statType)
            {
                return 0f;
            }
            else
            {
                return flat.GetFlatStatChange(GetMaxTier());
            }
        }
        else
        {
            return 0f;
        }
    }

    public bool TryGetInflictedStatusEffect(out StatusEffect statusEffect)
    {
        if (TokenType == TokenType.INFLICT_STATUS)
        {
            statusEffect = ((StatusEffectTokenTierSO) m_TokenTierData).GetInflictedStatusEffect(GetMaxTier());
            return true;
        }
        else
        {
            statusEffect = null;
            return false;
        }
    }

    public float GetFinalCritProportion()
    {
        if (TokenType == TokenType.CRIT)
        {
            return ((CritTokenTierSO) m_TokenTierData).GetFinalDamageModifier(GetMaxTier());
        }
        else
        {
            return 1f;
        }
    }
}

public class TauntTokenStack : TokenStack
{
    public Unit TauntedUnit {get; private set;}

    public TauntTokenStack(Unit targetedUnit, TokenTierSO tokenTierSO, int initialNumber = 1) : base(tokenTierSO, 1, initialNumber)
    {
        TauntedUnit = targetedUnit;
    }
}
