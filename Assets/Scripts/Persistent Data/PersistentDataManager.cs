using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersistentDataManager : Singleton<PersistentDataManager>
{
    [SerializeField] private List<PlayerCharacterSO> m_CharacterSOs;
    [SerializeField] private List<PlayerClassSO> m_ClassSOs;

    private Dictionary<int, PlayerCharacterSO> m_CharacterSOsMap;
    private Dictionary<int, PlayerClassSO> m_ClassSOsMap;

    // mapping character IDs to their data?
    private Dictionary<int, CharacterData> m_PersistentData;

    protected override void HandleAwake()
    {
        base.HandleAwake();

        m_CharacterSOsMap = new();
        m_ClassSOsMap = new();
        m_CharacterSOs.ForEach(x => m_CharacterSOsMap.Add(x.m_Id, x));
        m_ClassSOs.ForEach(x => m_ClassSOsMap.Add(x.m_Id, x));

        HandleDependencies();
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;
        
        ParseSaveData(SaveManager.Instance.LoadCharacterSaveData()); 
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }

    private void ParseSaveData(List<CharacterSaveData> characterSaveData)
    {
        m_PersistentData = new();

        foreach (CharacterSaveData data in characterSaveData)
        {
            CharacterData persistentData = new() {m_BaseData = m_CharacterSOs[data.m_CharacterId], m_CurrClass = m_ClassSOs[data.m_ClassId], m_CurrExp = data.m_CurrExp, m_CurrLevel = data.m_CurrLevel, m_CurrStats = data.m_CurrStats, m_CurrStatsProgress = data.m_CurrStatProgress};
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
