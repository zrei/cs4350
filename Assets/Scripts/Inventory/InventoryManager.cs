using System.Collections.Generic;
using UnityEngine;

public class WeaponInstance
{
    public int m_InstanceId;
    public bool m_IsEquipped;
    public WeaponInstanceSO m_Weapon;

    public WeaponStackSaveData Serialize()
    {
        return new WeaponStackSaveData(m_InstanceId, m_IsEquipped, m_Weapon.m_WeaponId);
    }
}

[System.Serializable]
public struct WeaponStackSaveData
{
    public int m_InstanceId;
    public bool m_IsEquipped;
    public int m_WeaponId;

    public WeaponStackSaveData(int instanceId, bool isEquipped, int weaponId)
    {
        m_InstanceId = instanceId;
        m_IsEquipped = isEquipped;
        m_WeaponId = weaponId;
    }
}

public class InventoryManager : Singleton<InventoryManager>
{
    public Dictionary<int, WeaponInstance> m_Inventory;

    public void UnequipWeapon(int weaponId)
    {

    }

    public void EquipWeapon(int weaponId)
    {

    }

    public void ObtainWeapon(WeaponInstanceSO weaponSO)
    {

    }

    public void LoadWeapons()
    {

    }

    public void SaveWeapons()
    {

    }
}
