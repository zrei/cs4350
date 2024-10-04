using System.Collections.Generic;
using UnityEngine;

// can move token type here
public class TokenTierSO
{
    [Header("Details")]
    public int m_Id;
    public bool m_AllowStack;
    public TokenType m_TokenType;

    [Header("Tiers")]
    [Tooltip("Tokens in order of their tiers: Start from tier 1 and go up")]
    public List<TokenSO> m_TieredTokens;
    public int NumTiers => m_TieredTokens.Count;

    public bool TryRetreiveTier(int tier, out TokenSO token)
    {
        if (tier >= m_TieredTokens.Count)
        {
            token = null;
            return false;
        }

        token = m_TieredTokens[tier - 1];
        return true;
    }
}

// some code to check that all tokens are. the same type. at the very least.
// sigh