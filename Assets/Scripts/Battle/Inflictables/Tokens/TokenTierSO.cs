using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    [Header("Consumption")]
    [Tooltip("When to consume this token")]
    public ConsumeType[] m_Consumption;
    public bool ContainsConsumptionType(ConsumeType consumeType) => m_Consumption.Contains(consumeType);

#if UNITY_EDITOR
    private void OnValidate()
    {

    }
#endif
}
