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
    private const string UNIT_DATA_KEY = "UnitData";
    private const string INVENTORY_DATA_KEY = "InventoryData";
    private const string ITEM_SEPARATOR = "\t";

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
        return LoadData<CharacterSaveData>(UNIT_DATA_KEY);
    }
    
    public void SaveCharacterData(IEnumerable<CharacterSaveData> data)
    {
        SaveData<CharacterSaveData>(UNIT_DATA_KEY, data);
    }

    public List<WeaponInstanceSaveData> LoadInventory()
    {
        return LoadData<WeaponInstanceSaveData>(INVENTORY_DATA_KEY);
    }

    public void SaveInventoryData(IEnumerable<WeaponInstanceSaveData> data)
    {
        SaveData<WeaponInstanceSaveData>(INVENTORY_DATA_KEY, data);
    }

    private List<T> LoadData<T>(string saveKey)
    {
        string[] saveData = PlayerPrefs.GetString(saveKey).Split(ITEM_SEPARATOR);
        List<T> characterData = new();
        foreach (string data in saveData)
        {
            if (string.IsNullOrEmpty(data))
                continue;
            characterData.Add(JsonUtility.FromJson<T>(data));
        }
        return characterData;
    }

    private void SaveData<T>(string saveKey, IEnumerable<T> data)
    {
        StringBuilder finalString = new();
        foreach (T item in data)
        {
            finalString.Append(JsonUtility.ToJson(item) + ITEM_SEPARATOR);
        }
        PlayerPrefs.SetString(saveKey, finalString.ToString());
    }
}
