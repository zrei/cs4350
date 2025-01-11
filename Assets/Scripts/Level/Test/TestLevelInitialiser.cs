using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestCharacterData
{
    [Header("Info")]
    public PlayerCharacterSO m_BaseData;

    [Header("State")]
    public int m_CurrLevel = 1;

    [Header("Weapons")]
    public bool m_OverrideWeapon = false;
    public WeaponInstanceSO m_OverriddenWeaponInstance = null;

    [Header("Starting Stats Options - this will override the starting stats if enabled")]
    public bool m_OverrideStartingStats = false;
    public Stats m_OverriddenStartingStats;

    [Header("Final Stats Options - this will override the final stats after level up if enabled")]
    public bool m_OverrideFinalStats = false;
    public Stats m_OverriddenFinalStats;
    public bool m_OverrideStatsProgress = false;
    public StatProgress m_OverriddenStatsProgress;
    
    [Header("Class Options - this will override equipped class if enabled")]
    public bool m_OverrideClass = false;
    public int m_OverriddenClassIndex = -1;
    public bool m_OverrideUnlockedClasses = false;
    public List<bool> m_OverriddenUnlockedClasses;

    public PlayerCharacterData GetPlayerCharacterData(int? overridenWeaponId)
    {
        PlayerCharacterData playerCharacterData = new(
            baseData: m_BaseData, 
            currClassIndex: m_OverrideClass ? m_OverriddenClassIndex : m_BaseData.StartingClassIndex, 
            currExp: 0,
            currLevel: 1, 
            currStats: m_OverrideStartingStats ? m_OverriddenStartingStats : m_BaseData.m_StartingStats, 
            statProgress: new StatProgress(),
            currUnlockedClasses: m_BaseData.GetUnlockedClassIndexes(m_BaseData.m_StartingLevel),
            currEquippedWeaponId: overridenWeaponId);
        
        LevellingManager.Instance.LevelCharacterToLevel(playerCharacterData, m_CurrLevel);

        if (m_OverrideUnlockedClasses)
            playerCharacterData.m_CurrUnlockedClasses = m_OverriddenUnlockedClasses;

        if (m_OverrideFinalStats)
            playerCharacterData.m_CurrStats = m_OverriddenFinalStats;

        if (m_OverrideStatsProgress)
            playerCharacterData.m_CurrStatsProgress = m_OverriddenStatsProgress;
        
        return playerCharacterData;
    }

    public PlayerCharacterBattleData GetPlayerCharacterBattleData(int? overriddenWeaponId)
    {
        PlayerCharacterData playerCharacterData = GetPlayerCharacterData(overriddenWeaponId);
        return playerCharacterData.GetBattleData();
    }
}

/// <summary>
/// Test script to initialise a standalone level without loading from world map.
/// </summary>
public class TestLevelInitialiser : MonoBehaviour
{
    [Header("Test Data")]
    [Tooltip("Override current morality percentage")]
    [SerializeField] private float m_MoralityPercentage = 0;
    [SerializeField] private List<TestCharacterData> m_TestCharacterData;
    
    private void Start()
    {
        HandleDependencies();
    }

    private void HandleDependencies()
    {
        if (!InventoryManager.IsReady)
        {
            InventoryManager.OnReady += HandleDependencies;
            return;
        }

        if (!LevellingManager.IsReady)
        {
            LevellingManager.OnReady += HandleDependencies;
            return;
        }

        InventoryManager.OnReady -= HandleDependencies;
        LevellingManager.OnReady -= HandleDependencies;

        Initialise();
    }

    private void Initialise()
    {
        List<PlayerCharacterData> finalData = new();
        foreach (TestCharacterData testCharacterData in m_TestCharacterData)
        {
            int? weaponId = null;
            if (testCharacterData.m_OverrideWeapon)
            {
                weaponId = InventoryManager.Instance.ObtainWeapon(testCharacterData.m_OverriddenWeaponInstance);
            }
            finalData.Add(testCharacterData.GetPlayerCharacterData(weaponId));
        }

        if (LevelManager.IsReady)
            LevelManager.Instance.Initialise(finalData);
        else 
            LevelManager.OnReady += () => LevelManager.Instance.Initialise(finalData);
        
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(SceneEnum.WORLD_MAP, SceneEnum.LEVEL);
    }
}
