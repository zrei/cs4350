// TODO: This also shouldn't be serializable once data is being passed from persistent manager to level
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCharacterData
{
    public PlayerCharacterSO m_BaseData;
    [HideInInspector]
    public int m_CurrClassIndex;
    public PlayerClassSO CurrClass => m_BaseData.m_PathGroup.GetClass(m_CurrClassIndex);
    public HashSet<int> m_CurrUnlockedClasses;
    public bool IsClassUnlocked(int index) => m_CurrUnlockedClasses.Contains(index);
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
    public GrowthRate TotalGrowthRate => m_BaseData.m_GrowthRates.FlatAugment(CurrClass.m_GrowthRateAugments);

    public int Id => m_BaseData.m_Id;
    public bool IsLord => m_BaseData.m_IsLord;

    /// <summary>
    /// Total base stats accounting for both character's current stats with equipped class' flat augments
    /// </summary>
    public Stats TotalBaseStats => m_CurrStats.FlatAugment(CurrClass.m_StatAugments);

    /// <summary>
    /// Note: This can be null. If so, it uses the base weapons.
    /// This is only a pointer since the weapon instance itself can change.
    /// </summary>
    public int? m_CurrEquippedWeaponId;

    public PlayerCharacterBattleData GetBattleData()
    {
        return new PlayerCharacterBattleData(m_BaseData, TotalBaseStats, CurrClass, GetWeaponInstanceSO(), IsLord, CurrClass.GetInflictedTokens(this.m_CurrLevel));
    }

    private WeaponInstanceSO GetWeaponInstanceSO()
    {
        if (!m_CurrEquippedWeaponId.HasValue || !InventoryManager.Instance.TryRetrieveWeapon(m_CurrEquippedWeaponId.Value, out WeaponInstanceSO weaponInstanceSO))
            return CurrClass.DefaultWeapon;
        else
            return weaponInstanceSO;
    }

    public void UnlockClass(int index)
    {
        if (!m_CurrUnlockedClasses.Contains(index))
        {
            m_CurrUnlockedClasses.Add(index);
            // call an event if need to display popup or something
        }
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

    private List<InflictedToken> m_ClassInflictedTokens;

    /// <summary>
    /// Whether this player unit should be tracked in battle. Once this unit dies, the battle is considered lost.
    /// This could be due to a variety of reasons, e.g. the unit is a lord etc.
    /// </summary>
    public bool m_CannotDieWithoutLosingBattle;

    public List<InflictedToken> GetInflictedTokens(float currMoralityPercentage)
    {
        List<InflictedToken> inflictedTokens = new();
        inflictedTokens.AddRange(m_ClassInflictedTokens);
        inflictedTokens.AddRange(m_BaseData.GetInflictedMoralityTokens(currMoralityPercentage));
        return inflictedTokens;
    }

    public PlayerCharacterBattleData(PlayerCharacterSO baseData, Stats currStats, PlayerClassSO classSO, WeaponInstanceSO currEquippedWeapon, bool cannotDieWithoutLosingBattle, List<InflictedToken> inflictedTokens)
    {
        m_BaseData = baseData;
        m_CurrStats = currStats;
        m_ClassSO = classSO;
        m_CurrEquippedWeapon = currEquippedWeapon;
        m_CannotDieWithoutLosingBattle = cannotDieWithoutLosingBattle;
        m_ClassInflictedTokens = inflictedTokens;
    }

    public UnitModelData GetUnitModelData()
    {
        return m_BaseData.GetUnitModelData(m_ClassSO.m_OutfitType);
    }
} 
