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

        float flatAttackBuffs = attacker.GetFlatStatChange(isPhysical ? StatType.PHYS_ATTACK : StatType.MAG_ATTACK);
        float multAttackBuffs = attacker.GetMultStatChange(isPhysical ? StatType.PHYS_ATTACK : StatType.MAG_ATTACK);
        totalAttackStat = (totalAttackStat + flatAttackBuffs) * multAttackBuffs;

        float totalDefenceStat = isPhysical ? target.Stat.m_PhysicalDefence : target.Stat.m_MagicDefence;

        float flatDefenceBuffs = target.GetFlatStatChange(isPhysical ? StatType.PHYS_DEFENCE : StatType.MAG_DEFENCE);
        float multDefenceBuffs = target.GetMultStatChange(isPhysical ? StatType.PHYS_DEFENCE : StatType.MAG_DEFENCE);
        totalDefenceStat = (totalDefenceStat + flatDefenceBuffs) * multDefenceBuffs;
        
        float damage = Mathf.Max(0f, totalAttackStat - (1 - totalDefenceStat / (totalDefenceStat + ALPHA)));
        Logger.Log("Damage calc", $"Damage: {damage}", LogLevel.LOG);
        return damage;
    }
}