using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    [Tooltip("Weapons to start with")]
    [SerializeField] private List<WeaponInstanceSO> m_StartingWeapons;

    private readonly Dictionary<int, WeaponInstance> m_Inventory = new();
    private int m_CurrNextId;

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();

        HandleDependencies();

        GlobalEvents.Level.LevelResultsEvent += OnLevelResult;
        GlobalEvents.UI.SavePartyChangesEvent += SaveWeapons;

        GlobalEvents.Scene.EarlyQuitEvent += OnEarlyQuit;
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

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Level.LevelResultsEvent -= OnLevelResult;
        GlobalEvents.UI.SavePartyChangesEvent -= SaveWeapons;

        GlobalEvents.Scene.EarlyQuitEvent -= OnEarlyQuit;
    }
    #endregion

    #region Level Result
    private void OnLevelResult(LevelSO _, LevelResultType levelResultType)
    {
        HandleLevelResult(levelResultType == LevelResultType.SUCCESS);
    }

    private void OnEarlyQuit()
    {
        HandleLevelResult(false);
    }

    private void HandleLevelResult(bool save)
    {
        if (save)
        {
            SaveWeapons();
        }
        else
        {
            TryLoadSaveData();
        }
    }
    #endregion

    #region Save Data
    private void LoadWeapons()
    {
        if (!TryLoadSaveData())
        {
            LoadStartingInventory();
            SaveWeapons();
        }
            
    }

    private bool TryLoadSaveData()
    {
        if (!SaveManager.Instance.TryLoadInventory(out List<WeaponInstanceSaveData> weaponInstanceSaveData))
            return false;

        ParseSaveData(weaponInstanceSaveData);
        return true;
    }

    private void LoadStartingInventory()
    {
        Debug.Log("Load starting inventory");
        m_Inventory.Clear();
        m_CurrNextId = 0;

        foreach (WeaponInstanceSO weapon in m_StartingWeapons)
        {
            // assumption is that weapons are not equipped to begin with
            WeaponInstance weaponInstance = new() {m_InstanceId = m_CurrNextId, m_IsEquipped = false, m_WeaponInstanceSO = weapon};
            m_Inventory.Add(weaponInstance.m_InstanceId, weaponInstance);
            ++m_CurrNextId;
        }
    }

    private void ParseSaveData(List<WeaponInstanceSaveData> characterSaveData)
    {
        m_Inventory.Clear();

        foreach (WeaponInstanceSaveData data in characterSaveData)
        {
            if (!PersistentDataManager.Instance.TryGetWeaponInstanceSO(data.m_WeaponInstanceId, out WeaponInstanceSO weaponInstanceSO))
            {
                Logger.Log(this.GetType().Name, $"Weapon data for {data.m_WeaponInstanceId} cannot be found", LogLevel.ERROR);
                continue;
            }

            WeaponInstance weaponInstance = new() {m_InstanceId = data.m_InstanceId, m_IsEquipped = data.m_IsEquipped, m_WeaponInstanceSO = weaponInstanceSO};
            m_Inventory.Add(weaponInstance.m_InstanceId, weaponInstance);
        }

        m_CurrNextId = m_Inventory.Count;
    }

    private void SaveWeapons()
    {
        SaveManager.Instance.SaveInventoryData(m_Inventory.Values.Select(x => x.GetSaveData()));
    }
    #endregion

    #region Edit
    public void ChangeWeaponEquipStatus(int weaponInstanceId, bool isEquipped)
    {
        m_Inventory[weaponInstanceId].ChangeEquipStatus(isEquipped);
    }

    public void ObtainWeapon(WeaponInstanceSO weaponInstanceSO)
    {
        m_Inventory.Add(m_CurrNextId, new() {m_InstanceId = m_CurrNextId, m_IsEquipped = false, m_WeaponInstanceSO = weaponInstanceSO});
        ++m_CurrNextId;
    }
    #endregion

    #region Getters
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
    
    public List<WeaponInstance> RetrieveWeaponsOfType(WeaponType weaponType)
    {
        return m_Inventory.Values.Where(x => x.m_WeaponInstanceSO.m_WeaponType == weaponType).ToList();
    }
    #endregion

#if UNITY_EDITOR
    public void SetStartingInventory(List<WeaponInstanceSO> startingWeaponInstances)
    {
        m_StartingWeapons = startingWeaponInstances;
    }
#endif
}
