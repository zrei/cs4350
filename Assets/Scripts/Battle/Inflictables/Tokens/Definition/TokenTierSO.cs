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
    public bool m_IsBuff = true;
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
    public virtual int NumTiers => m_TieredTokens.Count;

    [Header("Conditions")]
    [Tooltip("Conditions, that if not met, the token will not be consumed")]
    public List<ActionConditionSO> m_ActivationConditions;
    [Tooltip("Conditions that should be used for attacks specifically")]
    public List<AttackInfoConditionSO> m_AttackInfoConditions;
    [Tooltip("Tick this if the effect will always remain once activated. If so, the effect will not deactivate even if the conditions are no longer met")]
    public bool m_CannotBeDeactivated = true;

    // not sure if this can be handled under the activation conditions, but I think not, so here it lays for now
    [Tooltip("Whether this skill can only activate for a limited number of times")]
    public bool m_LimitedActivation = false;
    [Tooltip("Number of times this token can be activated. Will be ignored if limited activation is not true")]
    public int m_MaxActivations = 1;

    protected bool TryRetrieveTier(int tier, out TokenSO token)
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

    /// <summary>
    /// For static conditions, limited activation check will have to be done by the runtime wrapper as no state is stored here
    /// </summary>
    /// <returns></returns>
    public bool IsConditionsMet(Unit unit, MapLogic mapLogic, AttackInfo attackInfo = null)
    {
        return m_ActivationConditions.All(x => x.IsConditionMet(unit, mapLogic)) && (m_AttackInfoConditions.Count == 0 || (attackInfo != null && m_AttackInfoConditions.All(x => x.IsConditionMet(attackInfo))));
    }

    [Header("Consumption")]
    [Tooltip("When to consume this token")]
    public TokenConsumptionType[] m_Consumption;
    public virtual bool m_ResetConditionMet => false;
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
