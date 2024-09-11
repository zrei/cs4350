// going to keep mods separate from status effects for now...
using System.Collections.Generic;
using System.Linq;

public class StatusManager : IStatChange
{
    private List<StatusEffect> m_StatusEffects = new List<StatusEffect>();
    private List<Token> m_Tokens = new List<Token>();

    #region Add Inflictable
    // haven't accounted for. add this much stack
    public void AddEffect(StatusEffect statusEffect)
    {
        m_StatusEffects.Add(statusEffect);
    }

    public void AddToken(Token token)
    {
        m_Tokens.Add(token);
    }
    #endregion

    #region Tick
    public void Tick(Unit unit)
    {
        HashSet<StatusEffect> toRemove = new() {};
        foreach (StatusEffect statusEffect in m_StatusEffects)
        {
            statusEffect.Tick(unit);
            if (statusEffect.IsDepleted)
                toRemove.Add(statusEffect);
        }

        foreach (StatusEffect statusEffect in toRemove)
        {
            m_StatusEffects.Remove(statusEffect);
        }
    }
    #endregion

    #region Getters
    public IEnumerable<Token> GetTokens(TokenType tokenType)
    {
        return m_Tokens.Where(x => x.TokenType == tokenType);
    }

    public IEnumerable<Token> GetTokens(ConsumeType consumeType)
    {
        return m_Tokens.Where(x => x.ContainsConsumptionType(consumeType));
    }

    public IEnumerable<Token> GetTokens(ConsumeType consumeType, TokenType tokenType)
    {
        return m_Tokens.Where(x => x.ContainsConsumptionType(consumeType) && x.TokenType == tokenType);
    }

    public List<StatusEffect> GetInflictedStatusEffects(ConsumeType consumeType)
    {
        List<StatusEffect> statusEffects = new();

        foreach (Token token in m_Tokens)
        {
            if (token.ContainsConsumptionType(consumeType) && token.TryGetInflictedStatusEffect(out StatusEffect statusEffect))
            {
                statusEffects.Add(statusEffect);
            }
        }
        
        return statusEffects;
    }
    #endregion

    #region Clear Tokens
    public void ClearTokens()
    {
        m_Tokens.Clear();
    }

    public void ClearTokens(ConsumeType consumeType)
    {
        m_Tokens = m_Tokens.Where(x => !x.ContainsConsumptionType(consumeType)).ToList();
    }
    #endregion

    #region Stat Change
    public float GetFlatStatChange(StatType statType)
    {
        float totalFlatStatChange = 0;

        foreach (Token token in m_Tokens)
        {
            totalFlatStatChange += token.GetFlatStatChange(statType);
        }

        foreach (StatusEffect statusEffect in m_StatusEffects)
        {
            totalFlatStatChange += statusEffect.GetFlatStatChange(statType);
        }

        return totalFlatStatChange;
    }

    public float GetMultStatChange(StatType statType)
    {
        float totalFlatStatChange = 1;

        foreach (Token token in m_Tokens)
        {
            totalFlatStatChange *= token.GetMultStatChange(statType);
        }

        foreach (StatusEffect statusEffect in m_StatusEffects)
        {
            totalFlatStatChange *= statusEffect.GetMultStatChange(statType);
        }

        return totalFlatStatChange;
    }
    #endregion
}