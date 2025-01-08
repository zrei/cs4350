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
    private TokenManager m_TokenManager = new(false);

    #region IStatusManager
    public IEnumerable<TokenStack> TokenStacks => m_TokenManager.TokenStacks;
    public IEnumerable<StatusEffect> StatusEffects => m_StatusEffects.Values;
    public event StatusEvent OnAdd;
    public event StatusEvent OnChange;
    public event StatusEvent OnRemove;
    #endregion

    #region Initialisation
    public StatusManager()
    {
        m_TokenManager.OnAdd += OnAdd;
        m_TokenManager.OnChange += OnChange;
        m_TokenManager.OnRemove += OnRemove;
    }
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
        m_TokenManager.AddToken(inflictedToken, inflicter);
    }

    public void TryClearTauntToken(Unit deadUnit)
    {
        m_TokenManager.TryClearTauntToken(deadUnit);
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
        return m_TokenManager.HasTokenType(tokenType);
    }

    public List<TargetBundle<StatusEffect>> GetInflictedStatusEffects(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        return m_TokenManager.GetInflictedStatusEffects(consumeType, unit, filteredUnits);
    }

    public List<TargetBundle<InflictedToken>> GetInflictedTokens(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        return m_TokenManager.GetInflictedTokens(consumeType, unit, filteredUnits);
    }

    public List<TargetBundle<PassiveChangeBundle>> GetFlatPassiveChange(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        return m_TokenManager.GetFlatPassiveChange(consumeType, unit, filteredUnits);
    }

    public List<TargetBundle<InflictedTileEffect>> GetInflictedTileEffects(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        return m_TokenManager.GetInflictedTileEffects(consumeType, unit, filteredUnits);
    }

    public List<TargetBundle<PassiveChangeBundle>> GetMultPassiveChange(TokenConsumptionType consumeType, Unit unit, List<Unit> filteredUnits = null)
    {
        return m_TokenManager.GetMultPassiveChange(consumeType, unit, filteredUnits);
    }

    public List<SummonWrapper> GetSummonWrappers(TokenConsumptionType consumeType, Unit unit)
    {
        return m_TokenManager.GetSummonWrappers(consumeType, unit);
    }
    #endregion

    #region Clear Tokens
    public void ClearTokens()
    {
        m_TokenManager.ClearTokens();
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
                m_TokenManager.ClearTokens(true);
                break;
            case StatusType.DEBUFF_TOKEN:
                m_TokenManager.ClearTokens(false);
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

    public void ConsumeTokens(TokenConsumptionType consumeType)
    {
        m_TokenManager.ConsumeTokens(consumeType);
    }

    public void ConsumeTokens(TokenType tokenType)
    {
        m_TokenManager.ConsumeTokens(tokenType);
    }
    #endregion

    #region Stat Change
    public float GetFlatStatChange(StatType statType, Unit unit)
    {
        return m_TokenManager.GetFlatStatChange(statType, unit);
    }

    public float GetMultStatChange(StatType statType, Unit unit)
    {
        return m_TokenManager.GetMultStatChange(statType, unit);
    }
    #endregion

    #region Special Handle
    public bool CanExtendTurn(Unit unit, AttackInfo attackInfo = null)
    {
        return m_TokenManager.CanExtendTurn(unit, attackInfo);
    }

    public bool IsTaunted(out Unit forceTarget)
    {
        return m_TokenManager.IsTaunted(out forceTarget);
    }

    public float GetCritAmount(Unit unit)
    {
        return m_TokenManager.GetCritAmount(unit);
    }

    public float GetLifestealProportion(Unit unit)
    {
        return m_TokenManager.GetLifestealProportion(unit);
    }

    public float GetReflectProportion(Unit unit)
    {
        return m_TokenManager.GetReflectProportion(unit);
    }
    #endregion

    #region Condition Check
    public void PreConsumptionConditionCheck(Unit unit, TokenConsumptionType tokenConsumptionType, TokenType tokenType)
    {
        m_TokenManager.PreConsumptionConditionCheck(unit, tokenConsumptionType, tokenType);
    }
    #endregion
}
