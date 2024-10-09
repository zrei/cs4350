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
    public StatProgress m_CurrStatProgress;
    /// <summary>
    /// If null, the character has no weapon equipped and will use the default weapon
    /// </summary>
    public int? m_CurrEquippedWeaponId;

    public CharacterSaveData(int characterId, int classId, int currLevel, int currExp, Stats currStats, StatProgress currStatProgress, int? currEquippedWeaponId = null)
    {
        m_CharacterId = characterId;
        m_ClassId = classId;
        m_CurrLevel = currLevel;
        m_CurrExp = currExp;
        m_CurrStats = currStats;
        m_CurrStatProgress = currStatProgress;
        m_CurrEquippedWeaponId = currEquippedWeaponId;
    }
}

/// <summary>
/// Saves to JSON only
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    private const string UnitDataKey = "UnitData";
    private const string InventoryDataKey = "InventoryData";

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
    
    public void SaveCharacterData(IEnumerable<CharacterSaveData> data)
    {
        StringBuilder finalString = new();
        foreach (CharacterSaveData saveData in data)
        {
            finalString.Append(JsonUtility.ToJson(saveData) + "\t");
        }
        PlayerPrefs.SetString(UnitDataKey, finalString.ToString());
    }

    public List<WeaponInstance> LoadInventory()
    {
        string[] saveData = PlayerPrefs.GetString(InventoryDataKey).Split("\t");
        List<WeaponInstance> weaponInstances = new();
        foreach (string data in saveData)
        {
            if (string.IsNullOrEmpty(data))
                continue;
            weaponInstances.Add(JsonUtility.FromJson<WeaponInstance>(data));
        }
        return weaponInstances;
    }

    public void SaveInventoryData(IEnumerable<WeaponInstance> data)
    {
        StringBuilder finalString = new();
        foreach (WeaponInstance weaponInstance in data)
        {
            finalString.Append(JsonUtility.ToJson(weaponInstance) + "\t");
        }
        PlayerPrefs.SetString(InventoryDataKey, finalString.ToString());
    }
}
