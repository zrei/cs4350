using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton class for managing game scenes.
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager>
{
    #region Scene Management

    [SerializeField] PlayerUnit m_TestUnit;

    public void LoadBattleScene(BattleSO battleSo, List<CharacterBattleData> playerUnits)
    {
        // Set up the callback to initialize battle parameters for when the battle scene is loaded
        GlobalEvents.Scene.BattleSceneLoadedEvent = OnBattleSceneLoaded(battleSo, playerUnits);
        
        // Load the battle scene
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }
    
    public void UnloadBattleScene()
    {
        // Clear the callback
        GlobalEvents.Scene.BattleSceneLoadedEvent = null;
        
        // Unload the battle scene
        SceneManager.UnloadSceneAsync(1);
    }

    #endregion

    #region Callbacks

    private GlobalEvents.Scene.BattleManagerEvent OnBattleSceneLoaded(
        BattleSO battleSo, List<CharacterBattleData> playerUnits)
    {
        return manager =>
        {
            Debug.Log("Battle scene loaded. Initialising battle.");
            manager.InitialiseBattle(battleSo, playerUnits.Select(x => InstantiatePlayerUnit(x)).ToList());
        };
    }

    private PlayerUnit InstantiatePlayerUnit(CharacterBattleData battleData)
    {
        PlayerUnit unit = Instantiate(m_TestUnit);
        unit.Initialise(battleData.m_CurrStats, battleData.m_ClassSO);
        return unit;
    }

    #endregion
    
}
