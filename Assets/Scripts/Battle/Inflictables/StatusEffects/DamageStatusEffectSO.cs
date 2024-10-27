using UnityEngine;

[CreateAssetMenu(fileName="DamageStatusEffectSO", menuName = "ScriptableObject/Inflictables/StatusEffect/DamageStatusEffectSO")]
public class DamageStatusEffectSO : StatusEffectSO
{
    public float m_DamagePerTurn;

    public override StatusEffectType StatusEffectType => StatusEffectType.INFLICT_DAMAGE;

    public override string ToString()
    {
        return $"{base.ToString()}{Mathf.RoundToInt(m_DamagePerTurn)}";
    }
}
