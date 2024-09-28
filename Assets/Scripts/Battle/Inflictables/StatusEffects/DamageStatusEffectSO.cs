using UnityEngine;

[CreateAssetMenu(fileName="DamageStatusEffectSO", menuName = "ScriptableObject/Classes/ActiveSkills/StatusEffect/DamageStatusEffectSO")]
public class DamageStatusEffectSO : StatusEffectSO
{
    public float m_DamagePerTurn;

    public override StatusEffectType StatusEffectType => StatusEffectType.INFLICT_DAMAGE;
}
