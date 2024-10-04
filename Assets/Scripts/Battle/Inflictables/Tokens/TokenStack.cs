using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenStack 
{
    private TokenTierSO m_TokenTierData;
    public bool AllowStack = m_TokenTierData.m_AllowStack;
    private List<int> m_NumTokensOfEachTier;
    private int NumTiers => m_TokenTierData.NumTiers;
    public bool IsEmpty => m_TokenTierData.All(x => x <= 0);
    private bool HasStack => m_NumTokensOfEachTier.Any(x => x > 0);
    public int Id => m_TokenTierData.m_Id;

    public TokenStack(TokenTierSO tokenTier)
    {
        m_NumTokensOfEachTier = new List<int>(tokenTier.NumTiers);
        for (int i = 0; i < tokenTier.NumTiers; ++i)
        {
            m_NumTokensOfEachTier.Add(0);
        }
    }

    /// <summary>
    /// Consume a single token, taking the highest tiered one first
    /// </summary>
    /// <returns></returns>
    public TokenSO ConsumeToken()
    {
        for (int i = m_NumTiers - 1; i >= 0; --i)
        {
            if (m_NumTokensOfEachTier[i] > 0)
            {
                if (m_TokenTierData.TryRetreiveTier(out TokenSO tokenSO))
                {
                    m_NumTokensOfEachTier[i]--;
                    return tokenSO;
                }
            }
        }
        return default;
    }

    /// <summary>
    /// Add a token belonging to the same tier if stacking is allowed
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="number"></param>
    public void AddToken(int tier, int number = 1)
    {
        if (!AllowStack && HasStack)
            return;
        
        m_NumTokensOfEachTier[tier - 1] += number;
    }
}

public class TauntTokenStack : TokenStack
{
    public Unit TauntedUnit {get; private set;}

    public TauntTokenStack(Unit targetedUnit, TokenTierSO tokenTierSO) : base(tokenTierSO)
    {
        TauntedUnit = targetedUnit;
    }
}
