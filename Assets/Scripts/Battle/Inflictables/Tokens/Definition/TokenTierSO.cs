using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Groups a bunch of tokens with the same effects but of different tiers together.
/// </summary>
[CreateAssetMenu(fileName = "TokenTierSO", menuName = "ScriptableObject/Classes/ActiveSkills/Token/TokenTierSO")]
public abstract class TokenTierSO : ScriptableObject
{
    [Header("Details")]
    public int m_Id;
    /// <summary>
    /// Whether or not multiple tokens (regardless of tier) are allowed to be stacked on the same unit
    /// </summary>
    public bool m_AllowStack;
    public virtual TokenType TokenType => TokenType.INFLICT_STATUS;

    [Header("Details")]
    public string m_TokenName;
    [TextArea]
    public string m_Description;
    public Sprite m_Icon;
    public Color m_Color = Color.white;

    [Header("Tiers")]
    [Tooltip("Tokens in order of their tiers: Start from tier 1 and go up")]
    public List<TokenSO> m_TieredTokens;
    public int NumTiers => m_TieredTokens.Count;

    public bool TryRetreiveTier(int tier, out TokenSO token)
    {
        if (tier > NumTiers)
        {
            Logger.Log(this.GetType().Name, $"There are less tiers than {tier} for this token group {m_Id}", LogLevel.WARNING);
            token = null;
            return false;
        }

        token = m_TieredTokens[tier - 1];
        return true;
    }

    [Header("Consumption")]
    [Tooltip("When to consume this token")]
    public TokenConsumptionType[] m_Consumption;
    public bool ContainsConsumptionType(TokenConsumptionType consumeType) => m_Consumption.Contains(consumeType);

    public override string ToString()
    {
        return m_Icon != null ? $"{m_TokenName} <sprite name=\"{m_Icon.name}\" tint>" : m_TokenName;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {

    }
#endif
}
