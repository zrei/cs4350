using System.Collections.Generic;
using UnityEngine;

public class WeaponStack
{
    public int m_OwnedStack;
    public int m_NumEquipped;
    public WeaponSO m_Weapon;

    public WeaponStackSaveData Serialize()
    {
        return new WeaponStackSaveData(m_OwnedStack, m_NumEquipped, m_Weapon.m_WeaponId);
    }
}

[System.Serializable]
public struct WeaponStackSaveData
{
    public int m_OwnedStack;
    public int m_NumEquipped;
    public int m_WeaponId;

    public WeaponStackSaveData(int ownedStack, int numEquipped, int weaponId)
    {
        m_OwnedStack = ownedStack;
        m_NumEquipped = numEquipped;
        m_WeaponId = weaponId;
    }
}

public class InventoryManager : Singleton<InventoryManager>
{
    public Dictionary<int, WeaponStack> m_Inventory;

    public void UnequipWeapon(int weaponId)
    {

    }

    public void EquipWeapon(int weaponId)
    {

    }

    public void ObtainWeapon(WeaponSO weaponSO)
    {

    }

    public void LoadWeapons()
    {

    }

    public void SaveWeapons()
    {

    }
}
