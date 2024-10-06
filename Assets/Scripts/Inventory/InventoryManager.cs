using System.Collections.Generic;

public class WeaponInstance
{
    public int m_InstanceId;
    public bool m_IsEquipped;
    public WeaponInstanceSO m_Weapon;

    public WeaponInstanceSaveData Serialize()
    {
        return new WeaponInstanceSaveData(m_InstanceId, m_IsEquipped, m_Weapon.m_WeaponId);
    }

    public WeaponInstance(int instanceId, WeaponInstanceSO weaponInstanceSO)
    {
        m_InstanceId = instanceId;
        m_IsEquipped = false;
        m_Weapon = weaponInstanceSO;
    }

    public void ChangeEquipStatus(bool isEquipped)
    {
        m_IsEquipped = isEquipped;
    }
}

[System.Serializable]
public struct WeaponInstanceSaveData
{
    public int m_InstanceId;
    public bool m_IsEquipped;
    public int m_WeaponId;

    public WeaponInstanceSaveData(int instanceId, bool isEquipped, int weaponId)
    {
        m_InstanceId = instanceId;
        m_IsEquipped = isEquipped;
        m_WeaponId = weaponId;
    }
}

public class InventoryManager : Singleton<InventoryManager>
{
    public Dictionary<int, WeaponInstance> m_Inventory;
    private int m_CurrNextId;

    public void ChangeWeaponEquipStatus(int weaponId, bool isEquipped)
    {
        m_Inventory[weaponId].ChangeEquipStatus(isEquipped);
    }

    public void ObtainWeapon(WeaponInstanceSO weaponInstanceSO)
    {
        m_Inventory.Add(m_CurrNextId, new WeaponInstance(m_CurrNextId, weaponInstanceSO));
        ++m_CurrNextId;
    }

    public void LoadWeapons()
    {

    }

    public void SaveWeapons()
    {

    }

    public WeaponInstanceSO RetrieveWeapon(int weaponInstanceId)
    {
        return m_Inventory[weaponInstanceId].m_Weapon;
    }
}
