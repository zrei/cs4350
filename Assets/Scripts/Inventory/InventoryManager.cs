using System.Collections.Generic;

public class WeaponInstance
{
    public int m_InstanceId;
    public bool m_IsEquipped;
    public int m_WeaponSoId;

    public WeaponInstance(int instanceId, int weaponSoId)
    {
        m_InstanceId = instanceId;
        m_IsEquipped = false;
        m_WeaponSoId = weaponSoId;
    }

    public void ChangeEquipStatus(bool isEquipped)
    {
        m_IsEquipped = isEquipped;
    }
}

/*
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
*/

public class InventoryManager : Singleton<InventoryManager>
{
    public Dictionary<int, WeaponInstance> m_Inventory;
    private int m_CurrNextId;

    public void ChangeWeaponEquipStatus(int weaponId, bool isEquipped)
    {
        m_Inventory[weaponId].ChangeEquipStatus(isEquipped);
    }

    public void ObtainWeapon(int weaponSoId)
    {
        m_Inventory.Add(m_CurrNextId, new WeaponInstance(m_CurrNextId, weaponSoId));
        ++m_CurrNextId;
    }

    public void LoadWeapons()
    {
        foreach (WeaponInstance weaponInstance in SaveManager.Instance.LoadInventory())
        {
            m_Inventory.Add(weaponInstance.m_InstanceId, weaponInstance);
        }
        m_CurrNextId = m_Inventory.Count;
    }

    public void SaveWeapons()
    {
        SaveManager.Instance.SaveInventoryData(m_Inventory.Values);
    }

    public WeaponInstanceSO RetrieveWeapon(int weaponInstanceId)
    {
        PersistentDataManager.Instance.TryGetWeaponInstanceSO(m_Inventory[weaponInstanceId].m_WeaponSoId, out WeaponInstanceSO weaponInstanceSO);
        return weaponInstanceSO;
    }
}
