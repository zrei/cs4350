using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Representation of a weapon instance in the session data
/// </summary>
public class WeaponInstance
{
    /// <summary>
    /// Uniquely identifies this weapon instance in the inventory
    /// </summary>
    public int m_InstanceId;
    public bool m_IsEquipped;
    /// <summary>
    /// Base data for this weapon instance
    /// </summary>
    public WeaponInstanceSO m_WeaponInstanceSO;

    public WeaponInstanceSaveData GetSaveData()
    {
        return new WeaponInstanceSaveData(m_InstanceId, m_IsEquipped, m_WeaponInstanceSO.m_WeaponId);
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
    public int m_WeaponInstanceId;

    public WeaponInstanceSaveData(int instanceId, bool isEquipped, int weaponInstanceId)
    {
        m_InstanceId = instanceId;
        m_IsEquipped = isEquipped;
        m_WeaponInstanceId = weaponInstanceId;
    }
}


public class InventoryManager : Singleton<InventoryManager>
{
    public Dictionary<int, WeaponInstance> m_Inventory;
    private int m_CurrNextId;

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        if (!PersistentDataManager.IsReady)
        {
            PersistentDataManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;
        PersistentDataManager.OnReady -= HandleDependencies;

        LoadWeapons();
    }

    private void LoadWeapons()
    {
        ParseSaveData(SaveManager.Instance.LoadInventory());
        m_CurrNextId = m_Inventory.Count;
    }

    private void ParseSaveData(List<WeaponInstanceSaveData> characterSaveData)
    {
        m_Inventory.Clear();

        foreach (WeaponInstanceSaveData data in characterSaveData)
        {
            if (!PersistentDataManager.Instance.TryGetWeaponInstanceSO(data.m_InstanceId, out WeaponInstanceSO weaponInstanceSO))
            {
                Logger.Log(this.GetType().Name, $"Weapon data for {data.m_InstanceId} cannot be found", LogLevel.ERROR);
                continue;
            }

            WeaponInstance weaponInstance = new() {m_InstanceId = data.m_InstanceId, m_IsEquipped = data.m_IsEquipped, m_WeaponInstanceSO = weaponInstanceSO};
            m_Inventory.Add(weaponInstance.m_InstanceId, weaponInstance);
        }
    }

    public void SaveWeapons()
    {
        SaveManager.Instance.SaveInventoryData(m_Inventory.Values.Select(x => x.GetSaveData()));
    }

    public void ChangeWeaponEquipStatus(int weaponInstanceId, bool isEquipped)
    {
        m_Inventory[weaponInstanceId].ChangeEquipStatus(isEquipped);
    }

    public void ObtainWeapon(WeaponInstanceSO weaponInstanceSO)
    {
        m_Inventory.Add(m_CurrNextId, new() {m_InstanceId = m_CurrNextId, m_IsEquipped = false, m_WeaponInstanceSO = weaponInstanceSO});
        ++m_CurrNextId;
    }

    public bool TryRetrieveWeapon(int weaponInstanceId, out WeaponInstanceSO weaponInstanceSO)
    {
        if (!m_Inventory.TryGetValue(weaponInstanceId, out WeaponInstance weaponInstance))
        {
            weaponInstanceSO = null;
            return false;
        }
        else
        {
            weaponInstanceSO = weaponInstance.m_WeaponInstanceSO;
            return true;
        }
    }
}
