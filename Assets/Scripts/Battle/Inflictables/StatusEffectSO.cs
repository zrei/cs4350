using UnityEngine;

public enum StatusEffectType
{
    INFLICT_DAMAGE,
    STAT_CHANGE
}

public abstract class StatusEffectSO : ScriptableObject
{
    public string m_StatusEffectName;
    public string m_Description;
    public Sprite m_Sprite;
    public virtual StatusEffectType StatusEffectType => StatusEffectType.INFLICT_DAMAGE;
    public int m_MaxStack;
}

public class DamageStatusEffectSO : StatusEffectSO
{
    public float m_DamagePerTurn;

    public override StatusEffectType StatusEffectType => StatusEffectType.INFLICT_DAMAGE;
}

public class StatStatusEffectSO : StatusEffectSO
{
    public StatType m_AffectedStat;
    public float m_AffectAmount;
    public StatChangeType m_StatChangeType;

    public override StatusEffectType StatusEffectType => StatusEffectType.STAT_CHANGE;
}
