using System.Collections.Generic;
using UnityEngine;

public class WeaponStack
{
    public int m_OwnedStack;
    public int m_NumEquipped;
    public WeaponSO m_Weapon;

    public int WeaponId => m_Weapon.m_WeaponId;
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
