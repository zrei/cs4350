// static class not linked to MonoBehaviour atm
using System.Collections.Generic;
using UnityEngine;

public static class DamageCalc
{
    private const float ALPHA = 0.5f;

    public static float CalculateDamage(Unit attacker, Unit target, ActiveSkillSO attackSO)
    {
        // accounting for support somehow getting in here?
        bool isPhysical = attackSO.m_AttackType == AttackType.PHYSICAL;
        float totalAttackStat = isPhysical ? attacker.Stat.m_PhysicalAttack : attacker.Stat.m_MagicAttack;
        
        IEnumerable<Token> attackerTokens = attacker.GetTokens(isPhysical ? ConsumeType.CONSUME_ON_PHYS_ATTACK : ConsumeType.CONSUME_ON_MAG_ATTACK);
        
        float flatAttackBuffs = 0;
        float multAttackBuffs = 1;
        // add flat buffs
        foreach (Token token in attackerTokens)
        {
            TokenSO tokenSO = token.m_TokenData;
            if (tokenSO.m_TokenType == TokenType.FLAT_STAT_CHANGE)
            {
                if (((StatChangeTokenSO) tokenSO).m_AffectedStat == (isPhysical ? StatType.PHYS_ATTACK : StatType.MAG_ATTACK))
                {
                    flatAttackBuffs += token.m_Amount;
                }
            }
            else if (tokenSO.m_TokenType == TokenType.MULT_STAT_CHANGE)
            {
                if (((StatChangeTokenSO) tokenSO).m_AffectedStat == (isPhysical ? StatType.PHYS_ATTACK : StatType.MAG_ATTACK))
                {
                    multAttackBuffs *= token.m_Amount;
                }
            }
        }

        totalAttackStat = (totalAttackStat + flatAttackBuffs) * multAttackBuffs;

        Logger.Log("Damage calc", $"{attacker.name}'s total attack: {totalAttackStat}", LogLevel.LOG);

        IEnumerable<Token> targetTokens = target.GetTokens(isPhysical ? ConsumeType.CONSUME_ON_PHYS_DEFEND : ConsumeType.CONSUME_ON_MAG_DEFEND);

        float totalDefenceStat = isPhysical ? target.Stat.m_PhysicalDefence : target.Stat.m_MagicDefence;

        float flatDefenceBuffs = 0;
        float multDefenceBuffs = 1;
        
        // add flat buffs
        foreach (Token token in targetTokens)
        {
            TokenSO tokenSO = token.m_TokenData;
            if (tokenSO.m_TokenType == TokenType.FLAT_STAT_CHANGE)
            {
                if (((StatChangeTokenSO) tokenSO).m_AffectedStat == (isPhysical ? StatType.PHYS_DEFENCE : StatType.MAG_DEFENCE))
                {
                    flatDefenceBuffs += token.m_Amount;
                }
            }
            else if (tokenSO.m_TokenType == TokenType.MULT_STAT_CHANGE)
            {
                if (((StatChangeTokenSO) tokenSO).m_AffectedStat == (isPhysical ? StatType.PHYS_DEFENCE : StatType.MAG_DEFENCE))
                {
                    multDefenceBuffs *= token.m_Amount;
                }
            }
        }
        // add flat buffs (are defence buffs/debuffs also... tokens)

        totalDefenceStat = (totalDefenceStat + flatDefenceBuffs) * multDefenceBuffs;

        float damage = Mathf.Max(0f, totalAttackStat - (1 - totalDefenceStat / (totalDefenceStat + ALPHA)));
        Logger.Log("Damage calc", $"Damage: {damage}", LogLevel.LOG);
        return Mathf.Max(0f, totalAttackStat - (1 - totalDefenceStat / (totalDefenceStat + ALPHA)));
    }
}