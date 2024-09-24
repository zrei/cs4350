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
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }
}
