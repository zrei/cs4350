// static class not linked to MonoBehaviour atm
using UnityEngine;

public static class DamageCalc
{
    private const float ALPHA = 10;

    public static float CalculateDamage(ICanAttack attacker, IHealth target, ActiveSkillSO attackSO)
    {
        // accounting for support somehow getting in here?
        bool isMagic = attackSO.IsMagic;
        
        float totalAttackStat = attacker.GetTotalStat(isMagic ? StatType.MAG_ATTACK : StatType.PHYS_ATTACK, attackSO.m_DamageModifier * attacker.GetBaseAttackModifier());

        float totalDefenceStat = target.GetTotalStat(isMagic ? StatType.MAG_DEFENCE : StatType.PHYS_DEFENCE);

        float damage = Mathf.Max(0f, totalAttackStat * (ALPHA / (totalDefenceStat + ALPHA)));

        damage *= attacker.GetFinalCritProportion();

        Logger.Log("Damage calc", $"Attack: {totalAttackStat}", LogLevel.LOG);
        Logger.Log("Damage calc", $"Defence: {totalDefenceStat}", LogLevel.LOG);
        Logger.Log("Damage calc", $"Damage: {damage}", LogLevel.LOG);
        return damage;
    }

    public static float CalculateDamage(ICanAttack attacker, ActiveSkillSO attackSO)
    {
        // accounting for support somehow getting in here?
        bool isMagic = attackSO.IsMagic;

        float totalAttackStat = attacker.GetTotalStat(isMagic ? StatType.MAG_ATTACK : StatType.PHYS_ATTACK, attackSO.m_DamageModifier * attacker.GetBaseAttackModifier());

        float damage = Mathf.Max(0f, totalAttackStat);
        return damage;
    }

    public static float CalculateHealAmount(ICanAttack healer, ActiveSkillSO attackSO)
    {
        float totalHealStat = healer.GetTotalStat(StatType.MAG_ATTACK, attackSO.m_HealProportion * healer.GetBaseHealModifier());

        float finalHealAmount = totalHealStat * healer.GetFinalCritProportion();

        return finalHealAmount;
    }
}
