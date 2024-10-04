using System.Collections.Generic;
using System.Linq;

public class StatusManager :
    IStatChange,
    IStatusManager
{
    private readonly Dictionary<int, StatusEffect> m_StatusEffects = new();
    private readonly Dictionary<int, TokenStack> m_TokenStack = new();

    // public IEnumerable<Token> Tokens => m_Tokens.AsEnumerable();
    public IEnumerable<StatusEffect> StatusEffects => m_StatusEffects.Values;
    public event StatusEvent OnAdd;
    public event StatusEvent OnChange;
    public event StatusEvent OnRemove;

    #region Add Inflictable
    public void AddEffect(StatusEffect statusEffect)
    {
        if (m_StatusEffects.ContainsKey(statusEffect.Id))
        {
            m_StatusEffects[statusEffect.Id].AddStack(statusEffect.StackRemaining);
            OnChange?.Invoke(statusEffect);
        }
        else
        {
            m_StatusEffects[statusEffect.Id] = statusEffect;
            OnAdd?.Invoke(statusEffect);
        }
        Logger.Log(this.GetType().Name, "ADD STATUS EFFECT", LogLevel.LOG);
    }

    public void AddToken(TokenTierSO tokenData, int tier, int number = 1)
    {
        if (m_TokenStack.ContainsKey(tokenData.m_Id))
        {
            m_TokenStack[tokenData.m_Id].AddToken(tier, number);
            // :(
        }
        else
        {
            m_TokenStack[tokenData.m_Id] = new TokenStack(tokenData);
            m_TokenStack[tokenData.m_Id].AddToken(tier, number);
        }
        // OnAdd?.Invoke(token);
    }
    #endregion

    #region Reduce Stack
    public void ReduceStack(int statusEffectId, int reduceAmount)
    {
        if (!m_StatusEffects.TryGetValue(statusEffectId, out StatusEffect statusEffect))
            return;

        statusEffect.ReduceStack(reduceAmount);
        OnChange?.Invoke(statusEffect);
        if (statusEffect.IsDepleted)
        {
            OnRemove?.Invoke(statusEffect);
            m_StatusEffects.Remove(statusEffectId);
        }
    }

    public void ClearStatusEffect(int statusEffectId)
    {
        if (m_StatusEffects.ContainsKey(statusEffectId))
        {
            OnRemove?.Invoke(m_StatusEffects[statusEffectId]);
            m_StatusEffects.Remove(statusEffectId);
        }
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
            OnChange?.Invoke(statusEffect);

            if (statusEffect.IsDepleted)
                toRemove.Add(statusEffect);
        }

        foreach (StatusEffect statusEffect in toRemove)
        {
            OnRemove?.Invoke(statusEffect);
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
        m_Tokens = m_Tokens.Where(token =>
        {
            var isConsumed = token.ContainsConsumptionType(consumeType);
            if (isConsumed) OnRemove?.Invoke(token);
            return !isConsumed;
        }).ToList();
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

    #region Special Handle
    public bool IsTaunted(out Unit forceTarget)
    {

    }

    public float GetCritAmount()
    {

    }

    public float GetLifestealAmount(float dealtDamageThisTurn)
    {

    }

    public float GetReflectDamage(float damageReceived)
    {
        
    }
    #endregion
}
