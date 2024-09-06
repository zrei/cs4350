// static class not linked to MonoBehaviour atm
using UnityEngine;

public static class DamageCalc
{
    private const float ALPHA = 0.5f;

    public static float CalculateDamage(Unit attacker, Unit target, ActiveSkillSO attackSO)
    {
        // accounting for support somehow getting in here?
        bool isPhysical = attackSO.m_AttackType == AttackType.PHYSICAL;
        float totalAttackStat = isPhysical ? attacker.Stat.m_PhysicalAttack : attacker.Stat.m_MagicAttack;
        
        // add flat buffs
        foreach (Token token in attacker.GetTokens(isPhysical ? TokenType.FLAT_PHYS_BUFF : TokenType.FLAT_MAG_BUFF))
        {
            totalAttackStat += token.m_Amount;
        }

        // multiply buffs
        foreach (Token token in attacker.GetTokens(isPhysical ? TokenType.MULT_PHYS_BUFF : TokenType.MULT_MAG_BUFF))
        {
            totalAttackStat *= token.m_Amount;
        }

        float totalDefenceStat = isPhysical ? target.Stat.m_PhysicalDefence : target.Stat.m_MagicDefence;

        // add flat buffs (are defence buffs/debuffs also... tokens)


        return Mathf.Max(0f, totalAttackStat - (1 - totalDefenceStat / (totalDefenceStat + ALPHA)));
    }
}