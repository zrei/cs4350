using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Test script to initialise a once-off battle in place of the level manager when testing.
/// </summary>
public class TestBattleInitialiser : MonoBehaviour
{
    [SerializeField] private PlayerUnit m_TestUnit;
    [SerializeField] private List<CharacterBattleData> m_TestPlayerUnits;
    [SerializeField] private BattleSO m_TestBattle;
    
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
        manager.InitialiseBattle(m_TestBattle, m_TestPlayerUnits.Select(x => InstantiatePlayerUnit(x)).ToList());
    }

    private PlayerUnit InstantiatePlayerUnit(CharacterBattleData battleData)
    {
        PlayerUnit unit = Instantiate(m_TestUnit);
        unit.Initialise(battleData.m_CurrStats, battleData.m_ClassSO);
        return unit;
    }
}
