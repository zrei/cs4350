using System;
using UnityEngine;

public class StatusEffect :
    IStatus
{
    private static readonly Color InflictDamageColor = new(1, 0, 0, 1);
    // private static readonly Color StatChangeColor = new(0.5f, 0, 1, 1);

    private StatusEffectSO m_StatusEffectSO;
    private int m_StackRemaining;

    public int Id => m_StatusEffectSO.m_Id;
    public int StackRemaining => m_StackRemaining;
    public bool IsDepleted => m_StackRemaining <= 0;

    public Sprite Icon => m_StatusEffectSO.m_Sprite;
    public Color Color => m_StatusEffectSO.StatusEffectType switch
    {
        StatusEffectType.INFLICT_DAMAGE => InflictDamageColor,
        //StatusEffectType.STAT_CHANGE => StatChangeColor,
        _ => Color.white,
    };
    public string DisplayAmount => !string.IsNullOrEmpty(m_StatusEffectSO.m_DisplayStacksFormat)
        ? string.Format(m_StatusEffectSO.m_DisplayStacksFormat, m_StackRemaining)
        : string.Empty;
    public string Name => m_StatusEffectSO.name;
    public string Description => m_StatusEffectSO.m_Description;

    public StatusEffect(StatusEffectSO statusEffect, int initialStack)
    {
        m_StatusEffectSO = statusEffect;
        m_StackRemaining = initialStack;
    }

    public void Tick(Unit unit)
    {
        if (IsDepleted)
            return;
        ApplyAffect(unit);
        ReduceStack(1);
    }

    public void AddStack(int amt)
    {
        m_StackRemaining = Mathf.Max(m_StackRemaining + amt, m_StatusEffectSO.m_MaxStack);
    }

    public void ReduceStack(int amt)
    {
        m_StackRemaining = Mathf.Max(m_StackRemaining - amt, 0);
    }

    private void ApplyAffect(Unit unit)
    {
        if (m_StatusEffectSO.StatusEffectType == StatusEffectType.INFLICT_DAMAGE)
        {
            unit.TakeDamage(((DamageStatusEffectSO) m_StatusEffectSO).m_DamagePerTurn);
        }
    }

    /*
    public float GetFlatStatChange(StatType statType)
    {
        if (m_StatusEffectSO.StatusEffectType != StatusEffectType.STAT_CHANGE)
            return 0;

        StatStatusEffectSO statStatusEffectSO = (StatStatusEffectSO) m_StatusEffectSO;
        
        if (statStatusEffectSO.m_StatChangeType != StatChangeType.FLAT)
            return 0;

        if (statStatusEffectSO.m_AffectedStat != statType)
            return 0;

        return statStatusEffectSO.m_AffectAmount;
    }

    public float GetMultStatChange(StatType statType)
    {
        if (m_StatusEffectSO.StatusEffectType != StatusEffectType.STAT_CHANGE)
            return 1;

        StatStatusEffectSO statStatusEffectSO = (StatStatusEffectSO) m_StatusEffectSO;
        
        if (statStatusEffectSO.m_StatChangeType != StatChangeType.MULT)
            return 1;

        if (statStatusEffectSO.m_AffectedStat != statType)
            return 1;

        return statStatusEffectSO.m_AffectAmount;
    }
    */
}
