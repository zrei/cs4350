// going to keep mods separate from status effects for now...
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StatusEffectType
{
    POISON,
    STUN,
}

// might move some data to an SO later :?
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

public class StatusManager 
{
    private List<StatusEffect> m_StatusEffects = new List<StatusEffect>();
    private List<Token> m_Tokens;

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

    public IEnumerable<Token> GetAttackTokens()
    {
        return m_Tokens.Where(x => x.m_TokenData.AffectDamageCalcs);
    }

    public IEnumerable<Token> GetStatusTokens()
    {
        return m_Tokens.Where(x => x.m_TokenData.m_TokenType == TokenType.INFLICT_STATUS);
    }

    public IEnumerable<Token> GetTokens(TokenType tokenType)
    {
        return m_Tokens.Where(x => x.m_TokenData.m_TokenType == tokenType);
    }

    public void ClearTokens()
    {
        m_Tokens.Clear();
    }
}