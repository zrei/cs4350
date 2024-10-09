using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton class for managing game scenes.
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager>
{
    [SerializeField] Animator m_Transition;
    [SerializeField] float m_TransitionTime = 1f;
    
    const int BATTLE_SCENE_INDEX = 1;
    
    #region Scene Management

    public void LoadBattleScene(BattleSO battleSo, List<PlayerCharacterBattleData> unitBattleData, GameObject mapBiome)
    {
        // Set up the callback to initialize battle parameters for when the battle scene is loaded
        GlobalEvents.Scene.BattleSceneLoadedEvent = OnBattleSceneLoaded(battleSo, unitBattleData, mapBiome);
        
        // Load the battle scene
        StartCoroutine(LoadSceneWithTransition(BATTLE_SCENE_INDEX));
    }
    
    public void UnloadBattleScene()
    {
        // Clear the callback
        GlobalEvents.Scene.BattleSceneLoadedEvent = null;
        
        // Unload the battle scene
        StartCoroutine(UnloadSceneWithTransition(BATTLE_SCENE_INDEX));
    }

    #endregion

    #region Callbacks

    private GlobalEvents.Scene.BattleManagerEvent OnBattleSceneLoaded(
        BattleSO battleSo, List<PlayerCharacterBattleData> unitBattleData, GameObject mapBiome)
    {
        return manager =>
        {
            Debug.Log("Battle scene loaded. Initialising battle.");
            manager.InitialiseBattle(battleSo, unitBattleData, mapBiome);
        };
    }
    #endregion

    #region Transition

    IEnumerator LoadSceneWithTransition(int levelIndex)
    {
        m_Transition.SetTrigger("Start");
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        SceneManager.LoadSceneAsync(levelIndex, LoadSceneMode.Additive);
        
        m_Transition.SetTrigger("End");
        
        yield return new WaitForSeconds(m_TransitionTime);
    }
    
    IEnumerator UnloadSceneWithTransition(int levelIndex)
    {
        m_Transition.SetTrigger("Start");
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        SceneManager.UnloadSceneAsync(levelIndex);
        
        m_Transition.SetTrigger("End");
        
        yield return new WaitForSeconds(m_TransitionTime);
    }

    #endregion
    
}
