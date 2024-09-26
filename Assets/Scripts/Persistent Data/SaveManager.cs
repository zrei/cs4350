using System.Collections.Generic;
using System.Text;
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

/// <summary>
/// Saves to JSON only
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    private const string UnitDataKey = "UnitData";

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }

    public List<CharacterSaveData> LoadCharacterSaveData()
    {
        string[] saveData = PlayerPrefs.GetString(UnitDataKey).Split("\t");
        List<CharacterSaveData> characterData = new();
        foreach (string data in saveData)
        {
            if (string.IsNullOrEmpty(data))
                continue;
            characterData.Add(JsonUtility.FromJson<CharacterSaveData>(data));
        }
        return characterData;
    }
    
    public void SaveCharacterData(List<CharacterSaveData> data)
    {
        StringBuilder finalString = new();
        foreach (CharacterSaveData saveData in data)
        {
            finalString.Append(JsonUtility.ToJson(saveData) + "\t");
        }
        PlayerPrefs.SetString(UnitDataKey, finalString.ToString());
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}