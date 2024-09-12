// static class not linked to MonoBehaviour atm
using UnityEngine;

public static class DamageCalc
{
    private const float ALPHA = 0.5f;

    public static float CalculateDamage(ICanAttack attacker, IHealth target, ActiveSkillSO attackSO)
    {
        // accounting for support somehow getting in here?
        bool isPhysical = attackSO.ContainsAttackType(SkillType.PHYSICAL_ATTACK);
        
        float totalAttackStat = attacker.GetTotalStat(isPhysical ? StatType.PHYS_ATTACK : StatType.MAG_ATTACK, attackSO.m_Amount);

        float totalDefenceStat = target.GetTotalStat(isPhysical ? StatType.PHYS_DEFENCE : StatType.MAG_DEFENCE);

        float damage = Mathf.Max(0f, totalAttackStat - (1 - totalDefenceStat / (totalDefenceStat + ALPHA)));
        Logger.Log("Damage calc", $"Damage: {damage}", LogLevel.LOG);
        return damage;
    }
}