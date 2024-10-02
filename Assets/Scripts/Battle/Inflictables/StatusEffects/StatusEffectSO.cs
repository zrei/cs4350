using UnityEngine;

public enum StatusEffectType
{
    INFLICT_DAMAGE,
    STAT_CHANGE
}

public abstract class StatusEffectSO : ScriptableObject
{
    public int m_Id;
    public string m_StatusEffectName;
    public string m_Description;
    public Sprite m_Sprite;
    public string m_DisplayStacksFormat = "{0:F0}";
    public virtual StatusEffectType StatusEffectType => StatusEffectType.INFLICT_DAMAGE;
    public int m_MaxStack;
}
