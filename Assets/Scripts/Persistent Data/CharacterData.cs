public class CharacterData
{
    public CharacterSO m_BaseData;
    public ClassSO m_CurrClass;
    public int m_CurrLevel;
    public int m_CurrExp;
    // current base stats accounting for all levelling but not classes
    public Stats m_CurrStats;

    public CharacterBattleData GetBattleData()
    {
        return new CharacterBattleData(m_BaseData, m_CurrStats, m_CurrClass);
    }
}

// TODO: This shouldn't be serializable once the data is being passed from level to battle
[System.Serializable]
public struct CharacterBattleData
{
    public CharacterSO m_BaseData;
    public Stats m_CurrStats;
    public ClassSO m_ClassSO;

    public CharacterBattleData(CharacterSO baseData, Stats currStats, ClassSO classSO)
    {
        m_BaseData = baseData;
        m_CurrStats = currStats;
        m_ClassSO = classSO;
    }
} 
