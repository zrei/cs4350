using System.Collections.Generic;
using System.Linq;

public class StatusManager :
    /*
    IFlatStatChange,
    IMultStatChange,
    */
    IStatusManager
{
    private readonly Dictionary<int, StatusEffect> m_StatusEffects = new();
    private readonly Dictionary<int, TokenStack> m_ConsumableTokenStacks = new();

    #region IStatusManager
    public IEnumerable<TokenStack> TokenStacks => m_ConsumableTokenStacks.Values;
    public IEnumerable<StatusEffect> StatusEffects => m_StatusEffects.Values;
    public event StatusEvent OnAdd;
    public event StatusEvent OnChange;
    public event StatusEvent OnRemove;
    #endregion

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
        Logger.Log(this.GetType().Name, $"ADD STATUS EFFECT {statusEffect.Name}", LogLevel.LOG);
    }

    public void AddToken(InflictedToken inflictedToken, Unit inflicter)
    {
        // special handling for taunt
        if (inflictedToken.TokenType == TokenType.TAUNT)
        {
            AddTauntToken(inflictedToken.m_TokenTierData, inflicter, inflictedToken.m_Number);
            return;
        }

        Logger.Log(this.GetType().Name, $"Add token: {inflictedToken.m_TokenTierData.name} of tier: {inflictedToken.m_Tier}", LogLevel.LOG);

        if (m_ConsumableTokenStacks.ContainsKey(inflictedToken.Id))
        {
            m_ConsumableTokenStacks[inflictedToken.Id].AddToken(inflictedToken.m_Tier, inflictedToken.m_Number);
            OnChange?.Invoke(m_ConsumableTokenStacks[inflictedToken.Id]);
        }
        else
        {
            m_ConsumableTokenStacks[inflictedToken.Id] = new TokenStack(inflictedToken.m_TokenTierData, inflictedToken.m_Tier, inflictedToken.m_Number);
            OnAdd?.Invoke(m_ConsumableTokenStacks[inflictedToken.Id]);
        }
    }

    // special case for now
    public void AddTauntToken(TokenTierSO tokenData, Unit forceTarget, int number = 1)
    {
        if (m_ConsumableTokenStacks.ContainsKey(tokenData.m_Id))
        {
            return;
        }
        else
        {
            Logger.Log(this.GetType().Name, $"Add taunt token, force target: {forceTarget.name}", LogLevel.LOG);
            m_ConsumableTokenStacks[tokenData.m_Id] = new TauntTokenStack(forceTarget, tokenData, number);
        }
    }

    public void TryClearTauntToken(Unit deadUnit)
    {
        if (TryGetTauntedUnit(out Unit tauntedUnit) && tauntedUnit.Equals(deadUnit))
        {
            ConsumeTokens(TokenType.TAUNT);
        }
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
            m_StatusEffects.Remove(statusEffectId);
            OnRemove?.Invoke(statusEffect);
        }
    }

    public void ClearStatusEffect(int statusEffectId)
    {
        if (m_StatusEffects.TryGetValue(statusEffectId, out var statusEffect))
        {
            m_StatusEffects.Remove(statusEffectId);
            OnRemove?.Invoke(statusEffect);
        }
    }
    #endregion

    #region Tick
    public void Tick(Unit unit)
    {
        HashSet<StatusEffect> toRemove = new() { };
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
            m_StatusEffects.Remove(statusEffect.Id);
            OnRemove?.Invoke(statusEffect);
        }
    }
    #endregion

    #region Getters
    public bool HasTokenType(TokenType tokenType)
    {
        return m_ConsumableTokenStacks.Any(x => x.Value.TokenType == tokenType);
    }
    /*
    public IEnumerable<Token> GetTokens(TokenType tokenType)
    {
        return m_Tokens.Where(x => x.TokenType == tokenType);
    }

    public IEnumerable<Token> GetTokens(TokenConsumptionType consumeType)
    {
        return m_Tokens.Where(x => x.ContainsConsumptionType(consumeType));
    }

    public IEnumerable<Token> GetTokens(TokenConsumptionType consumeType, TokenType tokenType)
    {
        return m_Tokens.Where(x => x.ContainsConsumptionType(consumeType) && x.TokenType == tokenType);
    }
    */

    public List<StatusEffect> GetInflictedStatusEffects(TokenConsumptionType consumeType, Unit unit)
    {
        List<StatusEffect> statusEffects = new();

        foreach (TokenStack tokenStack in TokenStacks)
        {
            if (tokenStack.TryGetInflictedStatusEffect(unit, out StatusEffect statusEffect))
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
        var tokens = new List<TokenStack>(m_ConsumableTokenStacks.Values);
        m_ConsumableTokenStacks.Clear();
        tokens.ForEach(x => OnRemove?.Invoke(x));
    }

    public void CleanseStatusTypes(List<StatusType> statusTypes)
    {
        foreach (StatusType statusType in statusTypes)
            CleanseStatusType(statusType);
    }

    private void CleanseStatusType(StatusType statusType)
    {
        switch (statusType)
        {
            case StatusType.STATUS_EFFECT:
                ClearStatusEffects();
                break;
            case StatusType.BUFF_TOKEN:
                ClearTokens(true);
                break;
            case StatusType.DEBUFF_TOKEN:
                ClearTokens(false);
                break;
        }
    }

    private void ClearStatusEffects()
    {
        foreach (StatusEffect statusEffect in m_StatusEffects.Values)
        {
            OnRemove?.Invoke(statusEffect);
        }

        m_StatusEffects.Clear();
    }

    private void ClearTokens(bool isBuff)
    {
        IEnumerable<TokenStack> tokenStacks = m_ConsumableTokenStacks.Values.ToList();

        foreach (TokenStack tokenStack in tokenStacks)
        {
            if (tokenStack.IsBuff == isBuff)
            {
                m_ConsumableTokenStacks.Remove(tokenStack.Id);
                OnRemove?.Invoke(tokenStack);
            }
        }
    }

    public void ConsumeTokens(TokenConsumptionType consumeType)
    {
        IEnumerable<TokenStack> tokenStacks = m_ConsumableTokenStacks.Values.ToList();

        foreach (TokenStack tokenStack in tokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType))
            {
                tokenStack.ConsumeToken();
                OnChange?.Invoke(tokenStack);
                if (tokenStack.IsEmpty)
                {
                    m_ConsumableTokenStacks.Remove(tokenStack.Id);
                    OnRemove?.Invoke(tokenStack);
                }
            }
        }
    }

    public void ConsumeTokens(TokenType tokenType)
    {
        IEnumerable<TokenStack> tokenStacks = m_ConsumableTokenStacks.Values.ToList();

        foreach (TokenStack tokenStack in tokenStacks)
        {
            if (tokenStack.TokenType == tokenType)
            {
                tokenStack.ConsumeToken();
                OnChange?.Invoke(tokenStack);
                if (tokenStack.IsEmpty)
                {
                    m_ConsumableTokenStacks.Remove(tokenStack.Id);
                    OnRemove?.Invoke(tokenStack);
                }
            }
        }
    }
    #endregion

    #region Stat Change
    public float GetFlatStatChange(StatType statType, Unit unit)
    {
        float totalFlatStatChange = 0;

        foreach (TokenStack tokenStack in m_ConsumableTokenStacks.Values)
        {
            totalFlatStatChange += tokenStack.GetFlatStatChange(statType, unit);
        }

        return totalFlatStatChange;
    }

    public float GetMultStatChange(StatType statType, Unit unit)
    {
        float totalFlatStatChange = 1;

        foreach (TokenStack tokenStack in m_ConsumableTokenStacks.Values)
        {
            totalFlatStatChange *= tokenStack.GetMultStatChange(statType, unit);
        }

        return totalFlatStatChange;
    }
    #endregion

    #region Special Handle
    public bool IsTaunted(out Unit forceTarget)
    {
        foreach (TokenStack tokenStack in m_ConsumableTokenStacks.Values)
        {
            if (tokenStack.TokenType == TokenType.TAUNT)
            {
                forceTarget = ((TauntTokenStack)tokenStack).TauntedUnit;
                if (forceTarget == null || forceTarget.IsDead)
                    return false;
                Logger.Log(this.GetType().Name, $"Is being taunted by {forceTarget.name}", LogLevel.LOG);
                return true;
            }
        }
        forceTarget = null;
        return false;
    }

    /// <summary>
    /// Disregards if unit is dead
    /// </summary>
    /// <param name="forceTarget"></param>
    /// <returns></returns>
    private bool TryGetTauntedUnit(out Unit forceTarget)
    {
        foreach (TokenStack tokenStack in m_ConsumableTokenStacks.Values)
        {
            if (tokenStack.TokenType == TokenType.TAUNT)
            {
                forceTarget = ((TauntTokenStack)tokenStack).TauntedUnit;
                return true;
            }
        }
        forceTarget = null;
        return false;
    }

    public float GetCritAmount(Unit unit)
    {
        float finalCritProportion = 1f;
        foreach (TokenStack tokenStack in m_ConsumableTokenStacks.Values)
        {
            finalCritProportion *= tokenStack.GetFinalCritProportion(unit);
        }
        return finalCritProportion;
    }

    public float GetLifestealProportion(Unit unit)
    {
        float finalLifestealProportion = 0f;
        foreach (TokenStack tokenStack in m_ConsumableTokenStacks.Values)
        {
            finalLifestealProportion += tokenStack.GetLifestealProportion(unit);
        }
        return finalLifestealProportion;
    }

    public float GetReflectProportion(Unit unit)
    {
        float finalReflectProportion = 0f;
        foreach (TokenStack tokenStack in m_ConsumableTokenStacks.Values)
        {
            finalReflectProportion += tokenStack.GetReflectProportion(unit);
        }
        return finalReflectProportion;
    }

    /*
    public float GetLifestealAmount(float dealtDamageThisTurn)
    {

    }

    public float GetReflectDamage(float damageReceived)
    {
        
    }
    */
    #endregion
}
