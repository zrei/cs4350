using System.Collections.Generic;

/// <summary>
/// Record of a character level up event.
/// </summary>
public readonly struct LevelUpSummary
{
    public readonly PlayerCharacterSO m_CharacterSO;
    public readonly PlayerClassSO m_ClassSO;
    public readonly int m_LevelGrowth;
    public readonly int m_FinalLevel;
    public readonly Dictionary<StatType, int> m_TotalStatGrowths;
    public readonly Stats m_FinalStats;

    public LevelUpSummary(PlayerCharacterData characterData, int initialLevel, 
        Dictionary<StatType, int> totalStatGrowths)
    {
        m_CharacterSO = characterData.m_BaseData;
        m_ClassSO = characterData.CurrClass;
        m_LevelGrowth = characterData.m_CurrLevel - initialLevel;
        m_FinalLevel = characterData.m_CurrLevel;
        m_TotalStatGrowths = totalStatGrowths;
        m_FinalStats = characterData.m_CurrStats;
    }
}