using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct InflictedTileEffect
{
    public TileEffectSO m_TileEffect;
    public float m_InitialTime;
}

public class TileEffectSO : ScriptableObject
{
    [Header("Details")]
    public TileType m_TileType;
    public float m_MaxTime;

    [Tooltip("Note that some skill types won't have any effect")]
    public SkillEffectType[] m_EffectTypes;

    [Header("Effects")]
    public float m_DamageAmount;

    [Space]
    public List<InflictedStatusEffect> m_InflictedStatusEffects;
    public List<InflictedToken> m_InflictedTokens;

    [Space]
    [Tooltip("What this skill will cleanse")]
    public List<StatusType> m_CleansedStatusTypes;

    [Space]
    public float m_HealAmount;
    [Tooltip("Sign is important!")]
    public float m_ChangeManaAmount;

    [Tooltip("Game object to spawn on top of the tile")]
    public GameObject m_TileGameObject;
    // VFX to spawn
    // public List<VFXSO> m_TileVfx;

    public bool ContainsEffectType(SkillEffectType skillEffectType) => m_EffectTypes.Contains(skillEffectType);
}

public class TileEffect 
{
    public TileEffectSO m_TileEffectSO;
    public float m_TimeRemaining;
    public bool IsEmpty => m_TimeRemaining <= 0;
    public TileType TileType => m_TileEffectSO.m_TileType;

    public TileEffect(TileEffectSO tileEffectSO, float inflictedTime)
    {
        m_TileEffectSO = tileEffectSO;
        m_TimeRemaining = Mathf.Min(inflictedTime, m_TileEffectSO.m_MaxTime);
    }

    public void TopUp(float inflictedTime)
    {
        m_TimeRemaining = Mathf.Min(m_TimeRemaining + inflictedTime, m_TileEffectSO.m_MaxTime);
    }

    public void Tick(float passedTime)
    {
        m_TimeRemaining -= passedTime;
    }

    public void ApplyEffects(Unit unit)
    {
        if (m_TileEffectSO.ContainsEffectType(SkillEffectType.DEALS_DAMAGE))
            unit.TakeDamage(m_TileEffectSO.m_DamageAmount);
        if (m_TileEffectSO.ContainsEffectType(SkillEffectType.DEALS_STATUS_OR_TOKENS))
        {
            List<StatusEffect> statusEffects = m_TileEffectSO.m_InflictedStatusEffects.Select(x => new StatusEffect(x.m_StatusEffect, x.m_Stack)).ToList();
            List<InflictedToken> inflictedTokens = m_TileEffectSO.m_InflictedTokens;
            unit.InflictTokens(inflictedTokens, null);
            unit.InflictStatus(statusEffects);
        }
        if (m_TileEffectSO.ContainsEffectType(SkillEffectType.HEAL))
            unit.Heal(m_TileEffectSO.m_HealAmount);
        if (m_TileEffectSO.ContainsEffectType(SkillEffectType.ALTER_MANA))
            unit.AlterMana(m_TileEffectSO.m_ChangeManaAmount);
        if (m_TileEffectSO.ContainsEffectType(SkillEffectType.CLEANSE))
            unit.Cleanse(m_TileEffectSO.m_CleansedStatusTypes);
    }
}
