using System.Collections.Generic;
using UnityEngine;

public class LevellingManager : Singleton<LevellingManager>
{
    [SerializeField] LevellingSO m_LevellingSO;

    public void LevelCharacter(PlayerCharacterData characterData, int expGained, out bool hasLevelledUp, out Dictionary<StatType, int> totalStatGrowths)
    {
        hasLevelledUp = false;
        totalStatGrowths = new();
        if (characterData.m_CurrLevel == LevellingSO.MAX_LEVEL)
        {
            return;
        }

        int finalExp = Mathf.Min(characterData.m_CurrExp + expGained, m_LevellingSO.GetRequiredExpAmount(LevellingSO.MAX_LEVEL));
        
        while (characterData.m_CurrLevel < LevellingSO.MAX_LEVEL)
        {
            if (finalExp >= m_LevellingSO.GetRequiredExpAmount(characterData.m_CurrLevel + 1))
            {
                hasLevelledUp = true;
                characterData.m_CurrLevel += 1;
                characterData.m_CurrStats = LevelUpStats(characterData.m_CurrStats, characterData.m_CurrStatsProgress, characterData.TotalGrowthRate, out List<(StatType, int)> statGrowths);
                foreach ((StatType statType, int growth) in statGrowths)
                {
                    if (!totalStatGrowths.ContainsKey(statType))
                        totalStatGrowths[statType] = 0;

                    totalStatGrowths[statType] += growth;
                }
                characterData.CheckClassUnlocks();
            }
            else
            {
                break;
            }
        }
    }

    // level up a character from its current level to the given level
    public void LevelCharacterToLevel(PlayerCharacterData characterData, int level)
    {
        // no need to level up
        if (characterData.m_CurrLevel >= level)
            return;

        if (characterData.m_CurrLevel == LevellingSO.MAX_LEVEL)
        {
            return;
        }

        while (characterData.m_CurrLevel < level)
        {   
            characterData.m_CurrLevel += 1;
            characterData.m_CurrStats = LevelUpStats(characterData.m_CurrStats, characterData.m_CurrStatsProgress, characterData.TotalGrowthRate, out List<(StatType, int)> _);
        }
        characterData.CheckClassUnlocks();
        characterData.m_CurrExp = m_LevellingSO.GetRequiredExpAmount(level);
    }

    private Stats LevelUpStats(Stats currStats, StatProgress currStatProgress, GrowthRate growthRate, out List<(StatType, int)> statGrowths)
    {
        currStatProgress.TryProgressStats(growthRate, out statGrowths);

        Dictionary<StatType, int> statGrowthDict = new();
        statGrowths.ForEach(x => statGrowthDict.Add(x.Item1, x.Item2));
        return currStats.LevelUpStats(statGrowthDict);
    }
}
