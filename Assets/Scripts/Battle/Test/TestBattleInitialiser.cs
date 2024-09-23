using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to initialise a once-off battle in place of the level manager when testing.
/// </summary>
public class TestBattleInitialiser : MonoBehaviour
{
    [SerializeField] private BattleSO m_TestBattle;
    [SerializeField] private List<Unit> m_TestPlacement;
    [SerializeField] private List<Stats> m_TestStats;
    [SerializeField] private List<ClassSO> m_TestClasses;
    
    private void Awake()
    {
        // If BattleSceneLoadedEvent has any subscribers (from level manager), don't add the test battle initialiser
        if (GlobalEvents.Scene.BattleSceneLoadedEvent != null) return;
        
        Debug.Log("TestBattleInitializer: No subscribers to BattleSceneLoadedEvent. Adding test battle initialiser.");
        GlobalEvents.Scene.BattleSceneLoadedEvent += OnBattleSceneLoaded;
    }
    
    private void OnBattleSceneLoaded(BattleManager manager)
    {
        Debug.Log("TestBattleInitializer: Battle scene loaded. Initialising battle.");
        manager.InitialiseBattle(m_TestBattle, m_TestPlacement, m_TestStats, m_TestClasses);
    }
}
