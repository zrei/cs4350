using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton class for managing game scenes.
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager>
{
    #region Scene Management

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
            manager.InitialiseBattle(battleSo, playerUnits);
        };
    }

    #endregion
    
}
