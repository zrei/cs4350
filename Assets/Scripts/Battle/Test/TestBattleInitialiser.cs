using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Test script to initialise a once-off battle in place of the level manager when testing.
/// </summary>
public class TestBattleInitialiser : MonoBehaviour
{
    [SerializeField] private List<PlayerCharacterBattleData> m_TestData;
    [SerializeField] private BattleSO m_TestBattle;
    
    private void Awake()
    {
        // If BattleSceneLoadedEvent has any subscribers (from level manager), don't add the test battle initialiser
        if (BattleManager.OnReady != null) return;
        
        Debug.Log("TestBattleInitializer: No subscribers to BattleSceneLoadedEvent. Adding test battle initialiser.");
        BattleManager.OnReady += OnBattleSceneLoaded;
    }
    
    private void OnBattleSceneLoaded()
    {
        BattleManager.OnReady -= OnBattleSceneLoaded;
        
        Debug.Log("TestBattleInitializer: Battle scene loaded. Initialising battle.");
        BattleManager.Instance.InitialiseBattle(m_TestBattle, m_TestData, new());
    }
}
