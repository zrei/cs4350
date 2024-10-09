// TODO: This also shouldn't be serializable once data is being passed from persistent manager to level
using UnityEngine;

[System.Serializable]
public class PlayerCharacterData
{
    public PlayerCharacterSO m_BaseData;
    [HideInInspector]
    public PlayerClassSO m_CurrClass;
    public int m_CurrLevel;
    public int m_CurrExp;

    /// <summary>
    /// Current base stats accounting for all levelling but not classes
    /// </summary>
    [HideInInspector]
    public Stats m_CurrStats;

    /// <summary>
    /// Current internal stats progress
    /// </summary>
    public StatProgress m_CurrStatsProgress;

    /// <summary>
    /// Growth rate ONLY accounting for the character
    /// </summary>
    public GrowthRate BaseGrowthRate => m_BaseData.m_GrowthRates;
    /// <summary>
    /// Total growth rates accounting for both the character and the equipped class
    /// </summary>
    public GrowthRate TotalGrowthRate => m_BaseData.m_GrowthRates.FlatAugment(m_CurrClass.m_GrowthRateAugments);

    public int Id => m_BaseData.m_Id;

    /// <summary>
    /// Total base stats accounting for both character's current stats with equipped class' flat augments
    /// </summary>
    public Stats TotalBaseStats => m_CurrStats.FlatAugment(m_CurrClass.m_StatAugments);

    /// <summary>
    /// Note: This can be null. If so, it uses the base weapons.
    /// This is only a pointer since the weapon instance itself can change.
    /// </summary>
    public int? m_CurrEquippedWeaponId;

    public PlayerCharacterBattleData GetBattleData()
    {
        return new PlayerCharacterBattleData(m_BaseData, TotalBaseStats, m_CurrClass, GetWeaponInstanceSO());
    }

    private WeaponInstanceSO GetWeaponInstanceSO()
    {
        if (!m_CurrEquippedWeaponId.HasValue || !InventoryManager.Instance.TryRetrieveWeapon(m_CurrEquippedWeaponId.Value, out WeaponInstanceSO weaponInstanceSO))
            return m_CurrClass.DefaultWeapon;
        else
            return weaponInstanceSO;
    }
}

// TODO: This shouldn't be serializable once the data is being passed from level to battle
[System.Serializable]
public struct PlayerCharacterBattleData
{
    public PlayerCharacterSO m_BaseData;

    /// <summary>
    /// This accounts for base stats + class augments
    /// </summary>
    public Stats m_CurrStats;
    public PlayerClassSO m_ClassSO;

    public WeaponInstanceSO m_CurrEquippedWeapon;

    public PlayerCharacterBattleData(PlayerCharacterSO baseData, Stats currStats, PlayerClassSO classSO, WeaponInstanceSO currEquippedWeapon)
    {
        m_BaseData = baseData;
        m_CurrStats = currStats;
        m_ClassSO = classSO;
        m_CurrEquippedWeapon = currEquippedWeapon;
    }

    public UnitModelData GetUnitModelData()
    {
        return m_BaseData.GetUnitModelData(m_ClassSO.m_OutfitType);
    }
} 
