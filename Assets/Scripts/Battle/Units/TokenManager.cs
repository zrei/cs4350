using System.Collections.Generic;
using System.Linq;

public class TargetBundle<T> 
{
    public T m_Effect;
    public IEnumerable<(CoordPair, GridType)> m_Positions;

    public TargetBundle(T effect, IEnumerable<(CoordPair, GridType)> positions)
    {
        m_Effect = effect;
        m_Positions = positions;
    }
}

public class PassiveChangeBundle
{
    public float m_HealthAmount;
    public float m_ManaAmount;

    public PassiveChangeBundle(float healthAmount, float manaAmount)
    {
        m_HealthAmount = healthAmount;
        m_ManaAmount = manaAmount;
    }

    public void AddBundle(PassiveChangeBundle passiveChangeBundle)
    {
        m_HealthAmount += passiveChangeBundle.m_HealthAmount;
        m_ManaAmount += passiveChangeBundle.m_ManaAmount;
    }
}

public class TokenManager
{
    private readonly Dictionary<int, TokenStack> m_TokenStacks = new();
    private bool m_IsPermanent = false;

    public TokenManager(bool isPermanent)
    {
        m_IsPermanent = isPermanent;
    }

    #region Status Manager Access
    public IEnumerable<TokenStack> TokenStacks => m_TokenStacks.Values;
    public event StatusEvent OnAdd;
    public event StatusEvent OnChange;
    public event StatusEvent OnRemove;
    #endregion

    #region Add Inflictable
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
            OnChange?.Invoke(m_TokenStacks[inflictedToken.Id]);
        }
        else
        {
            m_TokenStacks[inflictedToken.Id] = inflictedToken.m_TokenTierData is TargetOtherTilesTokenTierSO ? new TargetOtherUnitsTokenStack(inflictedToken.m_TokenTierData, inflictedToken.m_Tier, inflictedToken.m_Number, m_IsPermanent) : new TokenStack(inflictedToken.m_TokenTierData, inflictedToken.m_Tier, inflictedToken.m_Number, m_IsPermanent);
            OnAdd?.Invoke(m_TokenStacks[inflictedToken.Id]);
        }
    }

    // special case for now
    private void AddTauntToken(TokenTierSO tokenData, Unit forceTarget, int number = 1)
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

    public void TryClearTauntToken(Unit deadUnit)
    {
        if (TryGetTauntedUnit(out Unit tauntedUnit) && tauntedUnit.Equals(deadUnit))
        {
            ConsumeTokens(TokenType.TAUNT);
        }
    }
    #endregion

    #region Getters
    public bool HasTokenType(TokenType tokenType)
    {
        return m_TokenStacks.Any(x => x.Value.TokenType == tokenType);
    }

    private IEnumerable<(CoordPair, GridType)> GetTargetedPositions(TargetOtherUnitsTokenStack tokenStack, Unit unit, List<Unit> filteredUnits = null)
    {
        return tokenStack.GetTargettedUnits(unit, filteredUnits);
    }

    public List<TargetBundle<StatusEffect>> GetInflictedStatusEffects(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        List<TargetBundle<StatusEffect>> statusEffects = new();

        foreach (TokenStack tokenStack in TokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType) && tokenStack.TryGetInflictedStatusEffect(unit, out StatusEffect statusEffect))
            {
                statusEffects.Add(new TargetBundle<StatusEffect>(statusEffect, GetTargetedPositions((TargetOtherUnitsTokenStack) tokenStack, unit, filteredUnits)));
            }
        }

        return statusEffects;
    }

    public List<TargetBundle<InflictedToken>> GetInflictedTokens(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        List<TargetBundle<InflictedToken>> inflictedTokens = new();

        foreach (TokenStack tokenStack in TokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType) && tokenStack.TryGetInflictedToken(unit, out InflictedToken inflictedToken))
            {
                inflictedTokens.Add(new TargetBundle<InflictedToken>(inflictedToken, GetTargetedPositions((TargetOtherUnitsTokenStack) tokenStack, unit, filteredUnits)));
            }
        }

        return inflictedTokens;
    }

    public List<TargetBundle<InflictedTileEffect>> GetInflictedTileEffects(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        List<TargetBundle<InflictedTileEffect>> inflictedTileEffects = new();

        foreach (TokenStack tokenStack in TokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType) && tokenStack.TryGetInflictedTileEffect(unit, out InflictedTileEffect inflictedTileEffect))
            {
                inflictedTileEffects.Add(new TargetBundle<InflictedTileEffect>(inflictedTileEffect, GetTargetedPositions((TargetOtherUnitsTokenStack) tokenStack, unit, filteredUnits)));
            }
        }

        return inflictedTileEffects;
    }

    public List<TargetBundle<PassiveChangeBundle>> GetFlatPassiveChange(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        List<TargetBundle<PassiveChangeBundle>> flatPassiveChanges = new();

        foreach (TokenStack tokenStack in TokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType) && tokenStack.TryGetFlatPassiveChange(unit, out PassiveChangeBundle flatPassiveChange))
            {
                flatPassiveChanges.Add(new TargetBundle<PassiveChangeBundle>(flatPassiveChange, GetTargetedPositions((TargetOtherUnitsTokenStack) tokenStack, unit, filteredUnits)));
            }
        }

        return flatPassiveChanges;
    }

    public List<TargetBundle<PassiveChangeBundle>> GetMultPassiveChange(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        List<TargetBundle<PassiveChangeBundle>> multPassiveChanges = new();

        foreach (TokenStack tokenStack in TokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType) && tokenStack.TryGetMultPassiveChange(unit, out PassiveChangeBundle multPassiveChange))
            {
                multPassiveChanges.Add(new TargetBundle<PassiveChangeBundle>(multPassiveChange, GetTargetedPositions((TargetOtherUnitsTokenStack) tokenStack, unit, filteredUnits)));
            }
        }

        return multPassiveChanges;
    }

    public List<SummonWrapper> GetSummonWrappers(TokenConsumptionType consumeType, Unit unit)
    {
        List<SummonWrapper> overallSummonWrappers = new();

        foreach (TokenStack tokenStack in TokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType) && tokenStack.TryGetSummonedUnits(unit, out List<SummonWrapper> summonWrappers))
            {
                overallSummonWrappers.AddRange(summonWrappers);
            }
        }

        return overallSummonWrappers;
    }
    #endregion

    #region Clear Tokens
    public void ClearTokens()
    {
        var tokens = new List<TokenStack>(m_TokenStacks.Values);
        m_TokenStacks.Clear();
        tokens.ForEach(x => OnRemove?.Invoke(x));
    }

    public void ClearTokens(bool isBuff)
    {
        IEnumerable<TokenStack> tokenStacks = m_TokenStacks.Values.ToList();

        foreach (TokenStack tokenStack in tokenStacks)
        {
            if (tokenStack.IsBuff == isBuff)
            {
                m_TokenStacks.Remove(tokenStack.Id);
                OnRemove?.Invoke(tokenStack);
            }
        }
    }

    public void ConsumeTokens(TokenConsumptionType consumeType)
    {
        IEnumerable<TokenStack> tokenStacks = m_TokenStacks.Values.ToList();

        foreach (TokenStack tokenStack in tokenStacks)
        {
            if (tokenStack.ContainsConsumptionType(consumeType))
            {
                tokenStack.ConsumeToken();
                OnChange?.Invoke(tokenStack);
                if (tokenStack.IsEmpty)
                {
                    m_TokenStacks.Remove(tokenStack.Id);
                    OnRemove?.Invoke(tokenStack);
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
                OnChange?.Invoke(tokenStack);
                if (tokenStack.IsEmpty)
                {
                    m_TokenStacks.Remove(tokenStack.Id);
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

        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            totalFlatStatChange += tokenStack.GetFlatStatChange(statType, unit);
        }

        return totalFlatStatChange;
    }

    public float GetMultStatChange(StatType statType, Unit unit)
    {
        float totalFlatStatChange = 1;

        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            totalFlatStatChange *= tokenStack.GetMultStatChange(statType, unit);
        }

        return totalFlatStatChange;
    }
    #endregion

    #region Special Handle
    public bool CanExtendTurn(Unit unit, AttackInfo attackInfo = null)
    {
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            if (tokenStack.TokenType == TokenType.EXTEND_TURN)
            {
                if (tokenStack.CheckConditions(unit, attackInfo))
                    return true;
            }
        }
        return false;
    }

    public bool IsTaunted(out Unit forceTarget)
    {
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
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
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
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
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            finalCritProportion *= tokenStack.GetFinalCritProportion(unit);
        }
        return finalCritProportion;
    }

    public float GetLifestealProportion(Unit unit)
    {
        float finalLifestealProportion = 0f;
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            finalLifestealProportion += tokenStack.GetLifestealProportion(unit);
        }
        return finalLifestealProportion;
    }

    public float GetReflectProportion(Unit unit)
    {
        float finalReflectProportion = 0f;
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            finalReflectProportion += tokenStack.GetReflectProportion(unit);
        }
        return finalReflectProportion;
    }
    #endregion

    #region Condition Check
    public void PreConsumptionConditionCheck(Unit unit, TokenConsumptionType tokenConsumptionType, TokenType tokenType)
    {
        foreach (TokenStack tokenStack in m_TokenStacks.Values)
        {
            if (tokenStack.ContainsConsumptionType(tokenConsumptionType) && tokenStack.TokenType == tokenType)
            {
                tokenStack.CheckConditions(unit);
            }
        }
    }
    #endregion
}
