using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public struct CharacterSaveData
{
    public int m_CharacterId;
    public int m_ClassIndex;
    public int m_CurrLevel;
    public int m_CurrExp;
    public Stats m_CurrStats;
    public StatProgress m_CurrStatProgress;
    /// <summary>
    /// If null, the character has no weapon equipped and will use the default weapon
    /// </summary>
    public int? m_CurrEquippedWeaponId;
    public int m_UnlockedClasses;

    public CharacterSaveData(int characterId, int classIndex, int currLevel, int currExp, Stats currStats, StatProgress currStatProgress, int unlockedClasses, int? currEquippedWeaponId = null)
    {
        m_CharacterId = characterId;
        m_ClassIndex = classIndex;
        m_CurrLevel = currLevel;
        m_CurrExp = currExp;
        m_CurrStats = currStats;
        m_CurrStatProgress = currStatProgress;
        m_CurrEquippedWeaponId = currEquippedWeaponId;
        m_UnlockedClasses = unlockedClasses;
    }
}

/// <summary>
/// Saves to JSON only
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    private const string UNIT_DATA_KEY = "UnitData";
    private const string INVENTORY_DATA_KEY = "InventoryData";
    private const string MORALITY_DATA_KEY = "MoralityData";
    private const string FLAG_KEY = "FlagData";
    private const string LEVEL_KEY = "LevelProgress";
    private const string ITEM_SEPARATOR = "\t";

    /// <summary>
    /// Initial guaranteed save delay to ensure the save indicator shows up
    /// </summary>
    private const float SAVE_DELAY = 3.0f;

    private Coroutine m_SaveCoroutine = null;

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }

    #region Save
    private IEnumerator Save_Coroutine(VoidEvent postSaveEvent = null)
    {
        yield return new WaitForSeconds(SAVE_DELAY);
        PlayerPrefs.Save();
        postSaveEvent?.Invoke();
        m_SaveCoroutine = null;
    }
    
    public void Save(VoidEvent postSaveEvent = null)
    {
        if (m_SaveCoroutine != null)
        {
            Logger.Log(this.GetType().Name, "There is already a save process occurring!", LogLevel.ERROR);
            return;
        }
        m_SaveCoroutine = StartCoroutine(Save_Coroutine(postSaveEvent));
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteAll();
    }
    #endregion

    #region Character
    public bool TryLoadCharacterSaveData(out List<CharacterSaveData> characterSaveData)
    {
        if (PlayerPrefs.HasKey(UNIT_DATA_KEY))
        {
            characterSaveData = LoadData<CharacterSaveData>(UNIT_DATA_KEY);
            return true;
        }
        else
        {
            characterSaveData = null;
            return false;
        }
    }
    
    public void SaveCharacterData(IEnumerable<CharacterSaveData> data)
    {
        SaveData<CharacterSaveData>(UNIT_DATA_KEY, data);
    }
    #endregion

    #region Inventory
    public bool TryLoadInventory(out List<WeaponInstanceSaveData> weaponInstanceSaveData)
    {
        if (PlayerPrefs.HasKey(INVENTORY_DATA_KEY))
        {
            weaponInstanceSaveData = LoadData<WeaponInstanceSaveData>(INVENTORY_DATA_KEY);
            return true;
        }
        else
        {
            weaponInstanceSaveData = null;
            return false;
        }
    }

    public void SaveInventoryData(IEnumerable<WeaponInstanceSaveData> data)
    {
        SaveData<WeaponInstanceSaveData>(INVENTORY_DATA_KEY, data);
    }
    #endregion

    #region Persistent Flags
    public bool TryLoadPersistentFlags(out List<string> persistentFlags)
    {
        if (PlayerPrefs.HasKey(FLAG_KEY))
        {
            persistentFlags = LoadSaveDataStrings(FLAG_KEY);
            return true;
        }
        else
        {
            persistentFlags = null;
            return false;
        }
    }

    public void SavePersistentFlags(IEnumerable<string> flags)
    {
        SaveDataStrings(FLAG_KEY, flags);
    }
    #endregion

    #region Morality
    public bool TryLoadMorality(out int morality)
    {
        morality = PlayerPrefs.GetInt(MORALITY_DATA_KEY, 0);
        return PlayerPrefs.HasKey(MORALITY_DATA_KEY);
    }

    public void SaveMorality(int morality)
    {
        PlayerPrefs.SetInt(MORALITY_DATA_KEY, morality);
    }
    #endregion

    #region Level Progress
    public bool TryLoadCurrentLevel(out int currLevel)
    {
        if (PlayerPrefs.HasKey(LEVEL_KEY))
        {
            currLevel = PlayerPrefs.GetInt(LEVEL_KEY, 1);
            return true;
        }
        else
        {
            currLevel = 1;
            return false;
        }
    }

    public void SetCurrentLevel(int currLevel)
    {
        PlayerPrefs.SetInt(LEVEL_KEY, currLevel);
    }
    #endregion

    #region Array Handlers
    private List<T> LoadData<T>(string saveKey)
    {
        List<T> data = new();
        foreach (string dataString in LoadSaveDataStrings(saveKey))
        {
            data.Add(JsonUtility.FromJson<T>(dataString));
        }
        return data;
    }

    private void SaveData<T>(string saveKey, IEnumerable<T> data)
    {
        List<string> parsedData = new();
        foreach (T item in data)
        {
            parsedData.Add(JsonUtility.ToJson(item));
        }
        SaveDataStrings(saveKey, parsedData);
    }

    private void SaveDataStrings(string saveKey, IEnumerable<string> data)
    {
        StringBuilder finalString = new();
        foreach (string val in data)
        {
            finalString.Append(val + ITEM_SEPARATOR);
        }
        PlayerPrefs.SetString(saveKey, finalString.ToString());
    }

    private List<string> LoadSaveDataStrings(string saveKey)
    {
        string[] saveData = PlayerPrefs.GetString(saveKey).Split(ITEM_SEPARATOR);
        List<string> dataStrings = new();
        foreach (string data in saveData)
        {
            if (string.IsNullOrEmpty(data))
                continue;
            dataStrings.Add(data);
        }
        return dataStrings;
    }
    #endregion
}
