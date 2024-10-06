using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public void AddToken(InflictedToken inflictedToken, Unit inflicter)
    {
        // special handling for taunt
        if (inflictedToken.TokenType == TokenType.TAUNT)
        {
            AddTauntToken(inflictedToken.m_TokenTierData, inflicter, inflictedToken.m_Number);
            return;
        }

        Logger.Log(this.GetType().Name, $"Add token: {inflictedToken.m_TokenTierData.name} of tier: {inflictedToken.m_Tier}", LogLevel.LOG);

        if (m_TokenStacks.ContainsKey(inflictedToken.Id))
        {
            m_TokenStacks[inflictedToken.Id].AddToken(inflictedToken.m_Tier, inflictedToken.m_Number);
        }
        else
        {
            m_TokenStacks[inflictedToken.Id] = new TokenStack(inflictedToken.m_TokenTierData, inflictedToken.m_Tier, inflictedToken.m_Number);
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
            Logger.Log(this.GetType().Name, $"Add taunt token, force target: {forceTarget.name}", LogLevel.LOG);
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
    public bool HasTokenType(TokenType tokenType)
    {
        return m_TokenStacks.Any(x => x.Value.TokenType == tokenType);
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

    public void ConsumeTokens(TokenConsumptionType consumeType)
    {
        IEnumerable<TokenStack> tokenStacks = m_TokenStacks.Values.ToList();

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

    public void ConsumeTokens(TokenType tokenType)
    {
        IEnumerable<TokenStack> tokenStacks = m_TokenStacks.Values.ToList();

        foreach (TokenStack tokenStack in tokenStacks)
        {
            if (tokenStack.TokenType == tokenType)
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
                Logger.Log(this.GetType().Name, $"Is being taunted by {forceTarget.name}", LogLevel.LOG);
                return true;
            }
        }
        forceTarget = null;
        return false;
    }

    public bool CanEvade()
    {
        return HasTokenType(TokenType.EVADE);
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

    public bool IsStunned()
    {
        return HasTokenType(TokenType.STUN);
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
