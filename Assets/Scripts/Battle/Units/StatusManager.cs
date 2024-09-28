using System.Collections.Generic;
using System.Linq;

public class StatusManager : IStatChange
{
    private readonly Dictionary<int, StatusEffect> m_StatusEffects = new();
    private List<Token> m_Tokens = new List<Token>();

    #region Add Inflictable
    public void AddEffect(StatusEffect statusEffect)
    {
        if (m_StatusEffects.ContainsKey(statusEffect.Id))
            m_StatusEffects[statusEffect.Id].AddStack(statusEffect.StackRemaining);
        else
            m_StatusEffects[statusEffect.Id] = statusEffect;
        Logger.Log(this.GetType().Name, "ADD STATUS EFFECT", LogLevel.LOG);
    }

    public void AddToken(Token token)
    {
        m_Tokens.Add(token);
    }
    #endregion

    #region Reduce Stack
    public void ReduceStack(int statusEffectId, int reduceAmount)
    {
        if (!m_StatusEffects.TryGetValue(statusEffectId, out StatusEffect statusEffect))
            return;

        statusEffect.ReduceStack(reduceAmount);
        if (statusEffect.IsDepleted)
            m_StatusEffects.Remove(statusEffectId);
    }

    public void ClearStatusEffect(int statusEffectId)
    {
        if (m_StatusEffects.ContainsKey(statusEffectId))
            m_StatusEffects.Remove(statusEffectId);
    }
    #endregion

    #region Tick
    public void Tick(Unit unit)
    {
        HashSet<StatusEffect> toRemove = new() {};
        foreach (StatusEffect statusEffect in m_StatusEffects.Values)
        {
            Logger.Log(this.GetType().Name, $"Afflicting status effect {statusEffect.Name}", LogLevel.LOG);
            statusEffect.Tick(unit);

            if (statusEffect.IsDepleted)
                toRemove.Add(statusEffect);
        }

        foreach (StatusEffect statusEffect in toRemove)
        {
            m_StatusEffects.Remove(statusEffect.Id);
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

        foreach (StatusEffect statusEffect in m_StatusEffects.Values)
        {
            if (!statusEffect.IsDepleted)
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

        foreach (StatusEffect statusEffect in m_StatusEffects.Values)
        {
            if (!statusEffect.IsDepleted)
                totalFlatStatChange *= statusEffect.GetMultStatChange(statType);
        }

        return totalFlatStatChange;
    }
    #endregion
}
