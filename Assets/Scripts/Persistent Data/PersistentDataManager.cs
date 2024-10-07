using System.Collections.Generic;
using UnityEngine;

public class PersistentDataManager : Singleton<PersistentDataManager>
{
    [SerializeField] private List<CharacterSO> m_CharacterSOs;
    [SerializeField] private List<ClassSO> m_ClassSOs;
    [SerializeField] private List<WeaponInstanceSO> m_WeaponInstanceSOs;

    private readonly Dictionary<int, CharacterSO> m_CharacterSOsMap = new();
    private readonly Dictionary<int, ClassSO> m_ClassSOsMap = new();
    private readonly Dictionary<int, WeaponInstanceSO> m_WeaponInstanceSOsMap = new();

    protected override void HandleAwake()
    {
        base.HandleAwake();

        m_CharacterSOs.ForEach(x => m_CharacterSOsMap.Add(x.m_Id, x));
        m_ClassSOs.ForEach(x => m_ClassSOsMap.Add(x.m_Id, x));
        m_WeaponInstanceSOs.ForEach(x => m_WeaponInstanceSOsMap.Add(x.m_WeaponId, x));
    }

    public bool TryGetCharacterSO(int characterId, out CharacterSO characterSO)
    {
        return TryGetSO<CharacterSO>(characterId, m_CharacterSOsMap, out characterSO);
    }

    public bool TryGetClassSO(int classId, out ClassSO classSO)
    {
        return TryGetSO<ClassSO>(classId, m_ClassSOsMap, out classSO);
    }

    public bool TryGetWeaponInstanceSO(int weaponInstanceId, out WeaponInstanceSO weaponInstanceSO)
    {
        return TryGetSO<WeaponInstanceSO>(weaponInstanceId, m_WeaponInstanceSOsMap, out weaponInstanceSO);
    }
    
    private bool TryGetSO<T>(int id, Dictionary<int, T> map, out T soInstance) where T : ScriptableObject
    {
        if (map.ContainsKey(id))
        {
            soInstance = map[id];
            return true;
        }
        else
        {
            soInstance = null;
            return false;
        }
    }
}
