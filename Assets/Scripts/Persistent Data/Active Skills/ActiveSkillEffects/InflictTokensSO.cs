/*
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "InflictTokensSO", menuName = "ScriptableObjects/ActiveSKills/Effects/InflictTokensSO")]
public abstract class InflictTokensSO : ActiveSkillEffectSO
{
    public List<Token> m_InflictedTokens;

    public override void ApplyEffect(Unit attacker, Unit target, bool isPhysical)
    {
        target.InflictStatus(m_InflictedStatusEffects.Select(x => new StatusEffect(x.m_StatusEffect, x.m_Stack)).ToList());
    }
}
*/
