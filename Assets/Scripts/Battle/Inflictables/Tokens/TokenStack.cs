using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenStack 
{
    public TokenTierSO m_TokenTierData;
    public bool IsEmpty => false;
    public bool AllowStack = false;
    private List<int> m_NumTokensOfEachTier;
    private int m_NumTiers;
    private bool m_HasStack => m_NumTokensOfEachTier.Any(x => x > 0);
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
                m_NumTokensOfEachTier[i]--;
                return m_TokenTierData.m_TieredTokens[i];
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
        if (!AllowStack && m_HasStack)
            return;
        
        m_NumTokensOfEachTier[tier - 1] += number;
    }
}