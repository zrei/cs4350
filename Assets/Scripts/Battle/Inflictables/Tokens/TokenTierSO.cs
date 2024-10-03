using System.Collections.Generic;
using UnityEngine;

public class TokenTierSO
{
    [Tooltip("Tokens in order of their tiers")]
    public List<TokenSO> m_TieredTokens;
    public bool m_AllowStack;
    public int m_Id;
}
