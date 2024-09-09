// going to keep mods separate from status effects for now...
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StatusEffectType
{
    POISON,
    STUN,
}

// status effects are 
public abstract class StatusEffect
{
    public virtual StatusEffectType StatusEffectType => StatusEffectType.POISON;
    public bool IsDepleted => m_StackRemaining <= 0;

    public const int MAX_STACK = 3;
    protected int m_StackRemaining;

    public virtual void Tick(Unit unit)
    {
        ApplyAffect(unit);
        ReduceStack(1);
    }

    public void AddStack(int amt)
    {
        m_StackRemaining = Mathf.Max(m_StackRemaining + amt, MAX_STACK);
    }

    public void ReduceStack(int amt)
    {
        m_StackRemaining -= amt;
    }

    protected abstract void ApplyAffect(Unit unit);
}

public class PoisonStatusEffect : StatusEffect
{
    protected override void ApplyAffect(Unit unit)
    {
        unit.TakeDamage(2);
    }
}

public class StatusManager 
{
    private List<StatusEffect> m_StatusEffects = new List<StatusEffect>();
    private List<Token> m_Tokens = new List<Token>();

    // haven't accounted for. add this much stack
    public void AddEffect(StatusEffect statusEffect)
    {
        m_StatusEffects.Add(statusEffect);
    }

    public void AddToken(Token token)
    {
        m_Tokens.Add(token);
    }

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

    public IEnumerable<Token> GetTokens(TokenType tokenType)
    {
        return m_Tokens.Where(x => x.m_TokenData.m_TokenType == tokenType);
    }

    public IEnumerable<Token> GetTokens(ConsumeType consumeType)
    {
        return m_Tokens.Where(x => x.m_TokenData.m_Consumption.Contains(consumeType));
    }

    public IEnumerable<Token> GetTokens(ConsumeType consumeType, TokenType tokenType)
    {
        return m_Tokens.Where(x => x.m_TokenData.m_Consumption.Contains(consumeType) && x.m_TokenData.m_TokenType == tokenType);
    }

    public void ClearTokens()
    {
        m_Tokens.Clear();
    }

    public void ClearTokens(ConsumeType consumeType)
    {
        m_Tokens = m_Tokens.Where(x => !x.m_TokenData.m_Consumption.Contains(consumeType)).ToList();
    }

    public float GetFlatStatChange(StatType statType)
    {
        float totalFlatStatChange = 0;
        foreach (Token token in m_Tokens)
        {
            totalFlatStatChange += token.GetFlatStatChange(statType);
        }
        // dump the status effects on here too later
        return totalFlatStatChange;
    }

    public float GetMultStatChange(StatType statType)
    {
        float totalFlatStatChange = 1;
        foreach (Token token in m_Tokens)
        {
            totalFlatStatChange *= token.GetMultStatChange(statType);
        }
        // dump the status effects on here too later
        return totalFlatStatChange;
    }
}

public interface IStatChange
{
    public float GetFlatStatChange(StatType statType);

    public float GetMultStatChange(StatType statType);
}