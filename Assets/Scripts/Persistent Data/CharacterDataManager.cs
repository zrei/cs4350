using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterDataManager : Singleton<CharacterDataManager>
{
    // mapping character IDs to their data?
    private readonly Dictionary<int, CharacterData> m_PersistentData = new();

    protected override void HandleAwake()
    {
        base.HandleAwake();

        HandleDependencies();
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        if (PersistentDataManager.IsReady)
        {
            PersistentDataManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;
        PersistentDataManager.OnReady -= HandleDependencies;
        
        ParseSaveData(SaveManager.Instance.LoadCharacterSaveData()); 
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }

    private void ParseSaveData(List<CharacterSaveData> characterSaveData)
    {
        m_PersistentData.Clear();

        foreach (CharacterSaveData data in characterSaveData)
        {
            if (!PersistentDataManager.Instance.TryGetCharacterSO(data.m_CharacterId, out CharacterSO chaacterSO))
            {
                Logger.Log(this.GetType().Name, $"Character data for {data.m_CharacterId} cannot be found", LogLevel.ERROR);
                continue;
            }

            if (!PersistentDataManager.Instance.TryGetClassSO(data.m_ClassId, out ClassSO classSO))
            {
                Logger.Log(this.GetType().Name, $"Class data for {data.m_ClassId} cannot be found", LogLevel.ERROR);
                continue;
            }
            CharacterData persistentData = new() {m_BaseData = chaacterSO, m_CurrClass = classSO, m_CurrExp = data.m_CurrExp,
                m_CurrLevel = data.m_CurrLevel, m_CurrStats = data.m_CurrStats, m_CurrStatsProgress = data.m_CurrStatProgress};
            m_PersistentData.Add(persistentData.Id, persistentData);
        }
    }

    public List<CharacterData> RetrieveAllCharacterData()
    {
        return m_PersistentData.Values.ToList();
    }

    public List<CharacterData> RetrieveCharacterData(List<int> IDs)
    {
        return m_PersistentData.Values.Where(x => IDs.Contains(x.Id)).ToList();
    }

    /// <summary>
    /// Update the persistent data with the newly updated data from a finished level
    /// </summary>
    /// <param name="updatedData"></param>
    public void UpdateCharacterData(List<CharacterData> updatedData)
    {
        foreach (CharacterData data in updatedData)
        {
            m_PersistentData[data.Id] = data;
        }
    }
}