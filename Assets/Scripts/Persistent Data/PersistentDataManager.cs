using System.Collections.Generic;
using UnityEngine;

public class PersistentDataManager : Singleton<PersistentDataManager>
{
    [SerializeField] private List<PlayerCharacterSO> m_CharacterSOs;
    [SerializeField] private List<WeaponInstanceSO> m_WeaponInstanceSOs;

    private readonly Dictionary<int, PlayerCharacterSO> m_CharacterSOsMap = new();
    private readonly Dictionary<int, WeaponInstanceSO> m_WeaponInstanceSOsMap = new();

    protected override void HandleAwake()
    {
        base.HandleAwake();

        m_CharacterSOs.ForEach(x => m_CharacterSOsMap.Add(x.m_Id, x));
        m_WeaponInstanceSOs.ForEach(x => m_WeaponInstanceSOsMap.Add(x.m_WeaponId, x));
    }

    public bool TryGetPlayerCharacterSO(int characterId, out PlayerCharacterSO characterSO)
    {
        return TryGetSO<PlayerCharacterSO>(characterId, m_CharacterSOsMap, out characterSO);
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
