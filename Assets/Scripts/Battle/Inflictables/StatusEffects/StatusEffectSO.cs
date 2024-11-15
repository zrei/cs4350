using UnityEngine;

public enum StatusEffectType
{
    INFLICT_DAMAGE,
}

public abstract class StatusEffectSO : ScriptableObject
{
    public int m_Id;
    public string m_StatusEffectName;
    public string m_Description;
    public Sprite m_Sprite;
    public Color m_Color = Color.white;
    public virtual StatusEffectType StatusEffectType => StatusEffectType.INFLICT_DAMAGE;
    public int m_MaxStack;

    public override string ToString()
    {
        return m_Sprite != null ? $"{m_StatusEffectName} <sprite name=\"{m_Sprite.name}\" tint>" : m_StatusEffectName;
    }
}
