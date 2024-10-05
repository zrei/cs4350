using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Helps to store stats.
/// </summary>
[System.Serializable]
public struct Stats
{
    public float m_Health;
    public float m_Mana;
    public float m_PhysicalAttack;
    public float m_MagicAttack;
    public float m_PhysicalDefence;
    public float m_MagicDefence;
    public float m_Speed;
    public int m_MovementRange;

    public float GetStat(StatType stat)
    {
        return stat switch
        {
            StatType.HEALTH => m_Health,
            StatType.MANA => m_Mana,
            StatType.PHYS_ATTACK => m_PhysicalAttack,
            StatType.MAG_ATTACK => m_MagicAttack,
            StatType.PHYS_DEFENCE => m_PhysicalDefence,
            StatType.MAG_DEFENCE => m_MagicDefence,
            StatType.SPEED => m_Speed,
            StatType.MOVEMENT_RANGE => m_MovementRange,
            _ => -1,
        };
    }

    public Stats(float health, float mana, float physicalAttack, float magicalAttack, float physicalDefence, float magicDefence, float speed, int movementRange)
    {
        m_Health = health;
        m_Mana = mana;
        m_PhysicalAttack = physicalAttack;
        m_MagicAttack = magicalAttack;
        m_PhysicalDefence = physicalDefence;
        m_MagicDefence = magicDefence;
        m_Speed = speed;
        m_MovementRange = movementRange;
    }

    public Stats FlatAugment(Stats otherStat)
    {
        return new Stats(m_Health + otherStat.m_Health, m_Mana + otherStat.m_Mana, m_PhysicalAttack + otherStat.m_PhysicalAttack, m_MagicAttack + otherStat.m_MagicAttack, m_PhysicalDefence + otherStat.m_PhysicalDefence, m_MagicDefence + otherStat.m_MagicDefence, m_Speed + otherStat.m_Speed, m_MovementRange + otherStat.m_MovementRange);
    }

    public Stats LevelUpStats(Dictionary<StatType, int> statGrowths)
    {
        return new Stats(m_Health + statGrowths.GetValueOrDefault(StatType.HEALTH, 0), m_Mana + statGrowths.GetValueOrDefault(StatType.MANA, 0), m_PhysicalAttack + statGrowths.GetValueOrDefault(StatType.PHYS_ATTACK, 0), m_MagicAttack + statGrowths.GetValueOrDefault(StatType.MAG_ATTACK, 0), m_PhysicalDefence + statGrowths.GetValueOrDefault(StatType.PHYS_DEFENCE, 0), m_MagicDefence + statGrowths.GetValueOrDefault(StatType.MAG_DEFENCE, 0), m_Speed + statGrowths.GetValueOrDefault(StatType.SPEED, 0), m_MovementRange);
    }
}

/// <summary>
/// Helps to store the internal growth progress of a stat.
/// </summary>
[System.Serializable]
public class StatProgress
{
    public const int MAX_STAT_PROGESS = 100;
    public int m_HealthProgress;
    public int m_ManaProgress;
    public int m_PhysicalAttackProgress;
    public int m_MagicAttackProgress;
    public int m_PhysicalDefenceProgress;
    public int m_MagicDefenceProgress;
    public int m_SpeedProgress;

    public int GetStatProgress(StatType statType)
    {
        return statType switch
        {
            StatType.HEALTH => m_HealthProgress,
            StatType.MANA => m_ManaProgress,
            StatType.PHYS_ATTACK => m_PhysicalAttackProgress,
            StatType.MAG_ATTACK => m_MagicAttackProgress,
            StatType.PHYS_DEFENCE => m_PhysicalDefenceProgress,
            StatType.MAG_DEFENCE => m_MagicDefenceProgress,
            StatType.SPEED => m_SpeedProgress,
            _ => -1,
        };
    }

    public float GetStatProgressFractional(StatType statType)
    {
        return (float) (statType switch
        {
            StatType.HEALTH => m_HealthProgress,
            StatType.MANA => m_ManaProgress,
            StatType.PHYS_ATTACK => m_PhysicalAttackProgress,
            StatType.MAG_ATTACK => m_MagicAttackProgress,
            StatType.PHYS_DEFENCE => m_PhysicalDefenceProgress,
            StatType.MAG_DEFENCE => m_MagicDefenceProgress,
            StatType.SPEED => m_SpeedProgress,
            _ => -1,
        }) / MAX_STAT_PROGESS;
    }

    /// <summary>
    /// Returns true if there was any stat that levelled up
    /// </summary>
    /// <param name="growthRate"></param>
    /// <param name="statGrowths"></param>
    /// <returns></returns>
    public bool TryProgressStats(GrowthRate growthRate, out List<(StatType, int)> statGrowths)
    {
        bool hadAnyStatGrowth = false;
        statGrowths = new();
        
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            if (statType == StatType.MOVEMENT_RANGE)
                continue;

            if (TryProgressStat(statType, growthRate.GetStatGrowthRate(statType), out int statGrowth))
            {
                hadAnyStatGrowth = true;
                statGrowths.Add((statType, statGrowth));
            }
        }

        return hadAnyStatGrowth;
    }

    private bool TryProgressStat(StatType statType, int statGrowthRate, out int statGrowth)
    {
        switch (statType)
        {
            case StatType.HEALTH:
                return TryGetStatGrowth(m_HealthProgress, statGrowthRate, out m_HealthProgress, out statGrowth);
            case StatType.MANA:
                return TryGetStatGrowth(m_ManaProgress, statGrowthRate, out m_ManaProgress, out statGrowth);
            case StatType.PHYS_ATTACK:
                return TryGetStatGrowth(m_PhysicalAttackProgress, statGrowthRate, out m_PhysicalAttackProgress, out statGrowth);
            case StatType.MAG_ATTACK:
                return TryGetStatGrowth(m_MagicAttackProgress, statGrowthRate, out m_MagicAttackProgress, out statGrowth);
            case StatType.PHYS_DEFENCE:
                return TryGetStatGrowth(m_PhysicalDefenceProgress, statGrowthRate, out m_PhysicalDefenceProgress, out statGrowth);
            case StatType.MAG_DEFENCE:
                return TryGetStatGrowth(m_MagicDefenceProgress, statGrowthRate, out m_MagicDefenceProgress, out statGrowth);
            case StatType.SPEED:
                return TryGetStatGrowth(m_SpeedProgress, statGrowthRate, out m_SpeedProgress, out statGrowth);
            default:
                statGrowth = 0;
                return false;
        }
    }

    private bool TryGetStatGrowth(int initialStatProgress, int statGrowthRate, out int finalStatProgress, out int statGrowth)
    {
        statGrowth = 0;
        finalStatProgress = initialStatProgress + statGrowthRate;

        if (finalStatProgress < MAX_STAT_PROGESS)
        {
            return false;
        }

        statGrowth = (int) Mathf.Floor(finalStatProgress / MAX_STAT_PROGESS);
        finalStatProgress = finalStatProgress % MAX_STAT_PROGESS;
        return true;
    }
}

/// <summary>
/// Helps to store the growth rate.
/// </summary>
[System.Serializable]
public struct GrowthRate
{
    public int m_HealthGrowthRate;
    public int m_ManaGrowthRate;
    public int m_PhysicalAttackGrowthRate;
    public int m_MagicAttackGrowthRate;
    public int m_PhysicalDefenceGrowthRate;
    public int m_MagicDefenceGrowthRate;
    public int m_SpeedGrowthRate;

    public GrowthRate(int healthGrowthRate, int manaGrowthRate, int physicalAttackGrowthRate, int magicAttackGrowthRate, int physicalDefenceGrowthRate, int magicDefenceGrowthRate, int speedGrowthRate)
    {
        m_HealthGrowthRate = healthGrowthRate;
        m_ManaGrowthRate = manaGrowthRate;
        m_PhysicalAttackGrowthRate = physicalAttackGrowthRate;
        m_MagicAttackGrowthRate = magicAttackGrowthRate;
        m_PhysicalDefenceGrowthRate = physicalDefenceGrowthRate;
        m_MagicDefenceGrowthRate = magicDefenceGrowthRate;
        m_SpeedGrowthRate = speedGrowthRate;
    }

    public int GetStatGrowthRate(StatType statType)
    {
        return statType switch
        {
            StatType.HEALTH => m_HealthGrowthRate,
            StatType.MANA => m_ManaGrowthRate,
            StatType.PHYS_ATTACK => m_PhysicalAttackGrowthRate,
            StatType.MAG_ATTACK => m_MagicAttackGrowthRate,
            StatType.PHYS_DEFENCE => m_PhysicalDefenceGrowthRate,
            StatType.MAG_DEFENCE => m_MagicDefenceGrowthRate,
            StatType.SPEED => m_SpeedGrowthRate,
            _ => -1,
        };
    }

    public GrowthRate FlatAugment(GrowthRate otherGrowthRate)
    {
        return new GrowthRate(m_HealthGrowthRate + otherGrowthRate.m_HealthGrowthRate, m_ManaGrowthRate + otherGrowthRate.m_ManaGrowthRate, m_PhysicalAttackGrowthRate + otherGrowthRate.m_PhysicalAttackGrowthRate, m_MagicAttackGrowthRate + otherGrowthRate.m_MagicAttackGrowthRate, m_PhysicalDefenceGrowthRate + otherGrowthRate.m_PhysicalDefenceGrowthRate, m_MagicDefenceGrowthRate + otherGrowthRate.m_MagicDefenceGrowthRate, m_SpeedGrowthRate + otherGrowthRate.m_SpeedGrowthRate);
    }
}

public enum StatType
{
    HEALTH,
    MANA,
    PHYS_ATTACK,
    MAG_ATTACK,
    PHYS_DEFENCE,
    MAG_DEFENCE,
    SPEED,
    MOVEMENT_RANGE
}

public interface IFlatStatChange
{
    public float GetFlatStatChange(StatType statType);
}

public interface IMultStatChange {
    public float GetMultStatChange(StatType statType);
}

public interface IStat
{
    public float GetTotalStat(StatType statType, float baseModifier = 1f);
}
