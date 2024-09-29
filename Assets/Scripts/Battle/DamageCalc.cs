// static class not linked to MonoBehaviour atm
using UnityEngine;

public static class DamageCalc
{
    private const float ALPHA = 0.5f;

    public static float CalculateDamage(ICanAttack attacker, IHealth target, ActiveSkillSO attackSO)
    {
        // accounting for support somehow getting in here?
        bool isMagic = attackSO.IsMagic;
        
        float totalAttackStat = attacker.GetTotalStat(isMagic ? StatType.MAG_ATTACK : StatType.PHYS_ATTACK, attackSO.m_DamageModifier);

        float totalDefenceStat = target.GetTotalStat(isMagic ? StatType.MAG_DEFENCE : StatType.PHYS_DEFENCE);

        float damage = Mathf.Max(0f, totalAttackStat - (1 - totalDefenceStat / (totalDefenceStat + ALPHA)));
        Logger.Log("Damage calc", $"Attack: {totalAttackStat}", LogLevel.LOG);
        Logger.Log("Damage calc", $"Defence: {totalDefenceStat}", LogLevel.LOG);
        Logger.Log("Damage calc", $"Damage: {damage}", LogLevel.LOG);
        return damage;
    }
}
