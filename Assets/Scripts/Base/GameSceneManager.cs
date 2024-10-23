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
    
    private static readonly int Start = Animator.StringToHash("Start");
    private static readonly int End = Animator.StringToHash("End");
    
    const int BATTLE_SCENE_INDEX = 1;
    const int LEVEL_1_SCENE_INDEX = 2;
    const int LEVEL_2_SCENE_INDEX = 2;
    
    private VoidEvent m_OnSceneChange;
    private VoidEvent m_AfterSceneChange;
    
    #region Temporary Scene Data
    
    // Battle
    private BattleSO m_CurrentBattle;
    private List<PlayerCharacterBattleData> m_UnitBattleData;
    private GameObject m_MapBiome;

    #endregion
    
    #region Scene Management

    public void LoadLevelScene(int levelId, List<PlayerCharacterData> partyMembers)
    {
        LevelManager.OnReady += OnLevelManagerReady;
        var sceneIndex = levelId == 0 ? LEVEL_1_SCENE_INDEX : LEVEL_2_SCENE_INDEX;
        StartCoroutine(LoadAdditiveSceneWithTransition(sceneIndex));
        return;
        
        void OnLevelManagerReady(LevelManager levelManager)
        {
            LevelManager.OnReady -= OnLevelManagerReady;
        
            Debug.Log("Level scene loaded. Initialising level.");
            levelManager.Initialise(partyMembers);
        }
    }
    
    public void UnloadLevelScene(int levelId)
    {
        m_AfterSceneChange = () => GlobalEvents.Level.ReturnFromLevelEvent?.Invoke();
        
        // Unload the level scene
        var sceneIndex = levelId == 0 ? LEVEL_1_SCENE_INDEX : LEVEL_2_SCENE_INDEX;
        StartCoroutine(UnloadAdditiveSceneWithTransition(sceneIndex));
    }

    public void LoadBattleScene(BattleSO battleSo, List<PlayerCharacterBattleData> unitBattleData, GameObject mapBiome)
    {
        m_CurrentBattle = battleSo;
        m_UnitBattleData = unitBattleData;
        m_MapBiome = mapBiome;
        
        // Set up the callback to initialize battle parameters for when the battle scene is loaded
        GlobalEvents.Scene.BattleSceneLoadedEvent += OnBattleSceneLoaded;
        
        m_OnSceneChange = CameraManager.Instance.SetUpBattleCamera;

        // Load the battle scene
        StartCoroutine(LoadAdditiveSceneWithTransition(BATTLE_SCENE_INDEX));
    }
    
    public void UnloadBattleScene()
    {
        m_OnSceneChange = CameraManager.Instance.SetUpLevelCamera;
        
        m_AfterSceneChange = () => GlobalEvents.Battle.ReturnFromBattleEvent?.Invoke();
        
        // Unload the battle scene
        StartCoroutine(UnloadAdditiveSceneWithTransition(BATTLE_SCENE_INDEX));
    }

    #endregion

    #region Callbacks

    private void OnBattleSceneLoaded()
    {
        GlobalEvents.Scene.BattleSceneLoadedEvent -= OnBattleSceneLoaded;
        
        Debug.Log("Battle scene loaded. Initialising battle.");
        BattleManager.Instance.InitialiseBattle(m_CurrentBattle, m_UnitBattleData, m_MapBiome, new());
        
        m_CurrentBattle = null;
        m_UnitBattleData = null;
        m_MapBiome = null;
    }
    #endregion

    #region Transition

    IEnumerator LoadAdditiveSceneWithTransition(int sceneIndex)
    {
        m_Transition.SetTrigger(Start);
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        var asyncHandle = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        void OnSceneLoadComplete(AsyncOperation handle)
        {
            asyncHandle.completed -= OnSceneLoadComplete;

            m_OnSceneChange?.Invoke();
            m_OnSceneChange = null;
            
            m_Transition.SetTrigger(End);
        }
        asyncHandle.completed += OnSceneLoadComplete;
    }
    
    IEnumerator UnloadAdditiveSceneWithTransition(int sceneIndex)
    {
        m_Transition.SetTrigger(Start);
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        var asyncHandle = SceneManager.UnloadSceneAsync(sceneIndex);
        void OnSceneUnloadComplete(AsyncOperation handle)
        {
            asyncHandle.completed -= OnSceneUnloadComplete;

            m_OnSceneChange?.Invoke();
            m_OnSceneChange = null;

            m_Transition.SetTrigger(End);
            
            m_AfterSceneChange?.Invoke();
            m_AfterSceneChange = null;
        }
        asyncHandle.completed += OnSceneUnloadComplete;
    }

    #endregion
    
}
