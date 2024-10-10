using System.Collections;
using System.Collections.Generic;
using Game;
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
    
    #region Temporary Scene Data
    
    // Battle
    private BattleSO m_CurrentBattle;
    private List<PlayerCharacterBattleData> m_UnitBattleData;
    private GameObject m_MapBiome;
    
    #endregion
    
    #region Scene Management

    public void LoadBattleScene(BattleSO battleSo, List<PlayerCharacterBattleData> unitBattleData, GameObject mapBiome)
    {
        m_CurrentBattle = battleSo;
        m_UnitBattleData = unitBattleData;
        m_MapBiome = mapBiome;
        
        // Set up the callback to initialize battle parameters for when the battle scene is loaded
        GlobalEvents.Scene.BattleSceneLoadedEvent += OnBattleSceneLoaded;
        
        // Load the battle scene
        StartCoroutine(LoadBattleSceneWithTransition());
    }
    
    public void UnloadBattleScene()
    {
        // Unload the battle scene
        StartCoroutine(UnloadBattleSceneWithTransition());
    }

    #endregion

    #region Callbacks

    private void OnBattleSceneLoaded()
    {
        GlobalEvents.Scene.BattleSceneLoadedEvent -= OnBattleSceneLoaded;
        
        Debug.Log("Battle scene loaded. Initialising battle.");
        BattleManager.Instance.InitialiseBattle(m_CurrentBattle, m_UnitBattleData, m_MapBiome);
        
        m_CurrentBattle = null;
        m_UnitBattleData = null;
        m_MapBiome = null;
    }
    #endregion

    #region Transition

    IEnumerator LoadBattleSceneWithTransition()
    {
        m_Transition.SetTrigger("Start");
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        SceneManager.LoadSceneAsync(BATTLE_SCENE_INDEX, LoadSceneMode.Additive);
        
        CameraManager.Instance.SetUpBattleCamera();
        
        m_Transition.SetTrigger("End");
        
        yield return new WaitForSeconds(m_TransitionTime);
    }
    
    IEnumerator UnloadBattleSceneWithTransition()
    {
        m_Transition.SetTrigger("Start");
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        CameraManager.Instance.SetUpLevelCamera();
        
        SceneManager.UnloadSceneAsync(BATTLE_SCENE_INDEX);
        
        m_Transition.SetTrigger("End");
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        GlobalEvents.Battle.ReturnFromBattleEvent?.Invoke();
    }

    #endregion
    
}
