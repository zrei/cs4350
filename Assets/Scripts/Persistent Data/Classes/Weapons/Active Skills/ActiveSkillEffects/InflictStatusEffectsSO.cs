/*
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "InflictStatusEffectsSO", menuName = "ScriptableObjects/ActiveSKills/Effects/InflictStatusEffectsSO")]
public abstract class InflictStatusEffectsSO : ActiveSkillEffectSO
{
    public List<InflictedStatusEffect> m_InflictedStatusEffects;

    public override void ApplyEffect(Unit attacker, Unit target, bool isPhysical)
    {
        target.InflictStatus(m_InflictedStatusEffects.Select(x => new StatusEffect(x.m_StatusEffect, x.m_Stack)).ToList());
    }
}
*/
