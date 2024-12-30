// static class not linked to MonoBehaviour atm
using UnityEngine;

public static class DamageCalc
{
    private const float ALPHA = 10;

    public static float CalculateDamage(ICanAttack attacker, IHealth target, ActiveSkillSO attackSO, SkillType? forcedSkillType = null)
    {
        bool isMagic = attackSO.IsMagic;
        if (forcedSkillType.HasValue)
        {
            isMagic = forcedSkillType == SkillType.MAGIC;
        }
        
        float totalAttackStat = attacker.GetTotalStat(isMagic ? StatType.MAG_ATTACK : StatType.PHYS_ATTACK, attackSO.m_DamageModifier * attacker.GetBaseAttackModifier());

        float totalDefenceStat = target.GetTotalStat(isMagic ? StatType.MAG_DEFENCE : StatType.PHYS_DEFENCE);

        float damage = Mathf.Max(0f, totalAttackStat * (ALPHA / (totalDefenceStat + ALPHA)));

        damage *= attacker.GetFinalCritProportion();

        Logger.Log("Damage calc", $"Attack: {totalAttackStat}", LogLevel.LOG);
        Logger.Log("Damage calc", $"Defence: {totalDefenceStat}", LogLevel.LOG);
        Logger.Log("Damage calc", $"Damage: {damage}", LogLevel.LOG);
        return damage;
    }

    public static float CalculateSelfDamage(ICanAttack attacker, ActiveSkillSO attackSO)
    {
        bool isMagic = attackSO.IsMagic;

        float totalAttackStat = attacker.GetTotalStat(isMagic ? StatType.MAG_ATTACK : StatType.PHYS_ATTACK, attackSO.m_SelfDamageModifier * attacker.GetBaseAttackModifier());

        float totalDefenceStat = attacker.GetTotalStat(isMagic ? StatType.MAG_DEFENCE : StatType.PHYS_DEFENCE);

        float damage = Mathf.Max(0f, totalAttackStat * (ALPHA / (totalDefenceStat + ALPHA)));

        damage *= attacker.GetFinalCritProportion();

        Logger.Log("Damage calc", $"Attack: {totalAttackStat}", LogLevel.LOG);
        Logger.Log("Damage calc", $"Defence: {totalDefenceStat}", LogLevel.LOG);
        Logger.Log("Damage calc", $"Damage: {damage}", LogLevel.LOG);
        return damage;
    }

    public static float CalculateDamage(ICanAttack attacker, ActiveSkillSO attackSO)
    {
        bool isMagic = attackSO.IsMagic;

        float totalAttackStat = attacker.GetTotalStat(isMagic ? StatType.MAG_ATTACK : StatType.PHYS_ATTACK, attackSO.m_DamageModifier * attacker.GetBaseAttackModifier());

        float damage = Mathf.Max(0f, totalAttackStat);
        
        damage *= attacker.GetFinalCritProportion();

        return damage;
    }

    public static float CalculateHealAmount(ICanAttack healer, ActiveSkillSO attackSO)
    {
        float totalHealStat = healer.GetTotalStat(StatType.MAG_ATTACK, attackSO.m_HealProportion * healer.GetBaseHealModifier());

        float finalHealAmount = totalHealStat * healer.GetFinalCritProportion();

        return finalHealAmount;
    }

    public static float CalculateManaAlterAmount(ICanAttack healer, ActiveSkillSO attackSO)
    {
        return healer.GetTotalStat(StatType.MAG_ATTACK, attackSO.m_ManaAlterProportion);
    }
}
