using System.Collections.Generic;

/// <summary>
/// Record of a character level up event.
/// </summary>
public readonly struct LevelUpSummary
{
    public readonly CharacterSO m_CharacterSO;
    public readonly ClassSO m_ClassSO;
    public readonly int m_LevelGrowth;
    public readonly int m_FinalLevel;
    public readonly Dictionary<StatType, int> m_TotalStatGrowths;
    public readonly Stats m_FinalStats;

    public LevelUpSummary(CharacterData characterData, int initialLevel, 
        Dictionary<StatType, int> totalStatGrowths)
    {
        m_CharacterSO = characterData.m_BaseData;
        m_ClassSO = characterData.m_CurrClass;
        m_LevelGrowth = characterData.m_CurrLevel - initialLevel;
        m_FinalLevel = characterData.m_CurrLevel;
        m_TotalStatGrowths = totalStatGrowths;
        m_FinalStats = characterData.m_CurrStats;
    }
}