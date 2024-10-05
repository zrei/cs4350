/*
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DealDamageEffectSO", menuName = "ScriptableObjects/ActiveSKills/Effects/DealDamageEffectSO")]
public abstract class DealDamageEffectSO : ActiveSkillEffectSO
{
    public float m_AttackModifier = 1;

    /// <summary>
    /// Also will apply any status effects through tokens
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    /// <param name="isPhysical"></param>
    public override void ApplyEffect(Unit attacker, Unit target, bool isPhysical)
    {
        List<StatusEffect> inflictedStatusEffects = new();
        if (isPhysical)
            inflictedStatusEffects.AddRange(attacker.GetInflictedStatusEffects(ConsumeType.CONSUME_ON_PHYS_ATTACK));
        else
            inflictedStatusEffects.AddRange(attacker.GetInflictedStatusEffects(ConsumeType.CONSUME_ON_MAG_ATTACK));
        target.TakeDamage(DamageCalc.CalculateDamage(attacker, target, !isPhysical, m_AttackModifier));
        target.ClearTokens(!isPhysical ? ConsumeType.CONSUME_ON_MAG_DEFEND : ConsumeType.CONSUME_ON_PHYS_DEFEND);
        target.InflictStatus(inflictedStatusEffects);
    }
}
*/
