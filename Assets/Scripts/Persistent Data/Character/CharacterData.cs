// TODO: This also shouldn't be serializable once data is being passed from persistent manager to level
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public CharacterSO m_BaseData;
    [HideInInspector]
    public ClassSO m_CurrClass;
    public int m_CurrLevel;
    public int m_CurrExp;

    /// <summary>
    /// Current base stats accounting for all levelling but not classes
    /// </summary>
    [HideInInspector]
    public Stats m_CurrStats;

    /// <summary>
    /// Current internal stats progress
    /// </summary>
    public StatProgress m_CurrStatsProgress;

    /// <summary>
    /// Growth rate ONLY accounting for the character
    /// </summary>
    public GrowthRate BaseGrowthRate => m_BaseData.m_GrowthRates;
    /// <summary>
    /// Total growth rates accounting for both the character and the equipped class
    /// </summary>
    public GrowthRate TotalGrowthRate => m_BaseData.m_GrowthRates.FlatAugment(m_CurrClass.m_GrowthRateAugments);

    public int Id => m_BaseData.m_Id;

    /// <summary>
    /// Total base stats accounting for both character's current stats with equipped class' flat augments
    /// </summary>
    public Stats TotalBaseStats => m_CurrStats.FlatAugment(m_CurrClass.m_StatAugments);

    public CharacterBattleData GetBattleData()
    {
        return new CharacterBattleData(m_BaseData, TotalBaseStats, m_CurrClass);
    }
}

// TODO: This shouldn't be serializable once the data is being passed from level to battle
[System.Serializable]
public struct CharacterBattleData
{
    public CharacterSO m_BaseData;

    /// <summary>
    /// This accounts for base stats + class augments
    /// </summary>
    public Stats m_CurrStats;
    public ClassSO m_ClassSO;

    public CharacterBattleData(CharacterSO baseData, Stats currStats, ClassSO classSO)
    {
        m_BaseData = baseData;
        m_CurrStats = currStats;
        m_ClassSO = classSO;
    }

    public UnitModelData GetUnitModelData()
    {
        return m_BaseData.GetUnitModelData(m_ClassSO.m_OutfitType);
    }
} 
