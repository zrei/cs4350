using UnityEngine;

public enum StatusEffectType
{
    INFLICT_DAMAGE,
    STAT_CHANGE
}

public abstract class StatusEffectSO : ScriptableObject
{
    public string m_StatusEffetName;
    public string m_Description;
    public Sprite m_Sprite;
    public StatusEffectType m_StatusEffectType;
    public int m_MaxStack;
}

public class DamageStatusEffectSO : StatusEffectSO
{
    public float m_DamagePerTurn;
}

public class StatStatusEffectSO : StatusEffectSO
{
    public StatType m_AffectedStat;
    public float m_AffectAmount;
    public StatChangeType m_StatChangeType;
}
