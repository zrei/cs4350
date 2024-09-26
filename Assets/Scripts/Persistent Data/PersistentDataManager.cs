using System.Collections.Generic;
using UnityEngine;

public class PersistentDataManager : Singleton<PersistentDataManager>
{
    [SerializeField] private List<CharacterSO> m_CharacterSOs;
    [SerializeField] private List<ClassSO> m_ClassSOs;

    private Dictionary<int, CharacterSO> m_CharacterSOsMap;
    private Dictionary<int, ClassSO> m_ClassSOsMap;

    // mapping character IDs to their data?
    private Dictionary<int, CharacterData> m_PersistentData;

    protected override void HandleAwake()
    {
        base.HandleAwake();
        m_CharacterSOsMap = new();
        m_ClassSOsMap = new();
        m_CharacterSOs.ForEach(x => m_CharacterSOsMap.Add(x.m_Id, x));
        m_ClassSOs.ForEach(x => m_ClassSOsMap.Add(x.m_Id, x));
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
            CharacterData persistentData = new() {m_BaseData = m_CharacterSOs[data.m_CharacterId], m_CurrClass = m_ClassSOs[data.m_ClassId], m_CurrExp = data.m_CurrExp, m_CurrLevel = data.m_CurrLevel, m_CurrStats = data.m_CurrStats};
            m_PersistentData.Add(persistentData.Id, persistentData);
        }
    }
    
}
