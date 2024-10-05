using System.Collections.Generic;
using System.Linq;

public class StatusManager :
    IFlatStatChange,
    IMultStatChange//,
    //IStatusManager
{
    private readonly Dictionary<int, StatusEffect> m_StatusEffects = new();
    private readonly Dictionary<int, TokenStack> m_TokenStacks = new();

    // public IEnumerable<Token> Tokens => m_Tokens.AsEnumerable();
    public IEnumerable<TokenStack> TokenStacks => m_TokenStacks.Values;
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
        Logger.Log(this.GetType().Name, $"ADD STATUS EFFECT {statusEffect.Name}", LogLevel.LOG);
    }

    public void AddToken(TokenTierSO tokenData, int tier, int number = 1)
    {
        if (m_TokenStacks.ContainsKey(tokenData.m_Id))
        {
            m_TokenStacks[tokenData.m_Id].AddToken(tier, number);
        }
        else
        {
            m_TokenStacks[tokenData.m_Id] = new TokenStack(tokenData, tier, number);
        }
        // OnAdd?.Invoke(token);
    }

    // special case for now
    public void AddTauntToken(TokenTierSO tokenData, Unit forceTarget, int number = 1)
    {
        if (m_TokenStacks.ContainsKey(tokenData.m_Id))
        {
            return;
        }
        else
        {
            m_TokenStacks[tokenData.m_Id] = new TauntTokenStack(forceTarget, tokenData, number);
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

    public List<StatusEffect> GetInflictedStatusEffects(TokenConsumptionType consumeType)
    {
        List<StatusEffect> statusEffects = new();

        foreach (TokenStack tokenStack in TokenStacks)
        {
            if (tokenStack.TryGetInflictedStatusEffect(out StatusEffect statusEffect))
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
        m_TokenStacks.Clear();
    }

    public void ClearTokens(TokenConsumptionType consumeType)
    {
        IEnumerable<TokenStack> tokenStacks = m_TokenStacks.Values;

        foreach (TokenStack tokenStack in tokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType))
            {
                tokenStack.ConsumeToken();
                if (tokenStack.IsEmpty)
                {
                    m_TokenStacks.Remove(tokenStack.Id);
                    // OnRemove?.Invoke(tokenStack);
                }       
            }
        }
    }
    #endregion

    #region Stat Change
    public float GetFlatStatChange(StatType statType)
    {
        float totalFlatStatChange = 0;

        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            totalFlatStatChange += tokenStack.GetFlatStatChange(statType);
        }

        return totalFlatStatChange;
    }

    public float GetMultStatChange(StatType statType)
    {
        float totalFlatStatChange = 1;

        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            totalFlatStatChange *= tokenStack.GetMultStatChange(statType);
        }

        return totalFlatStatChange;
    }
    #endregion

    #region Special Handle
    public bool IsTaunted(out Unit forceTarget)
    {
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            if (tokenStack.TokenType == TokenType.TAUNT)
            {
                forceTarget = ((TauntTokenStack) tokenStack).TauntedUnit;
                if (forceTarget == null || forceTarget.IsDead)
                    return false;
                return true;
            }
        }
        forceTarget = null;
        return false;
    }

    public float GetCritAmount()
    {
        float finalCritProportion = 1f;
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            finalCritProportion *= tokenStack.GetFinalCritProportion();
        }
        return finalCritProportion;
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
