using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CharacterSaveData
{
    public int m_CharacterId;
    public int m_ClassId;
    public int m_CurrLevel;
    public int m_CurrExp;
    public Stats m_CurrStats;

    public CharacterSaveData(int characterId, int classId, int currLevel, int currExp, Stats currStats)
    {
        m_CharacterId = characterId;
        m_ClassId = classId;
        m_CurrLevel = currLevel;
        m_CurrExp = currExp;
        m_CurrStats = currStats;
    }
}
/*
// need a serialiser hrm
public struct CharacterData
{
    public CharacterSO m_BaseData;
    public ClassSO m_CurrClass;
    public int m_CurrLevel;
    public int m_CurrExp;
    // current base stats accounting for all levelling but not classes
    public Stats m_CurrStats;
}
*/
/// <summary>
/// Saves to JSON only
/// </summary>
public class SaveManager : Singleton<SaveManager>
{

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }

    public List<CharacterSaveData> LoadSaveData()
    {
        return new();
    }
    
    public void Save()
    {
        string jsonString = JsonUtility.ToJson(new CharacterSaveData(1, 2, 3, 4, new Stats()));
    }
}
