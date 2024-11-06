using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneEnum
{
    MAIN_MENU,
    WORLD_MAP,
    LEVEL,
    BATTLE
}
/// <summary>
/// Singleton class for managing game scenes.
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager>
{
    [SerializeField] Light m_WorldLight;
    [SerializeField] Light m_LevelLight;
    
    [SerializeField] Animator m_Transition;
    [SerializeField] float m_TransitionTime = 1f;
    
    private static readonly int Start = Animator.StringToHash("Start");
    private static readonly int End = Animator.StringToHash("End");
    
    const int MAIN_MENU_INDEX = 0;
    const int WORLD_MAP_INDEX = 1;
    const int BATTLE_SCENE_INDEX = 2;
    
    // Respective level scenes indexes will accessed by adding the level id to the level 1 value
    const int LEVEL_1_SCENE_INDEX = 3;
    
    private VoidEvent m_OnSceneChange;
    private VoidEvent m_AfterSceneChange;
    
    #region Temporary Scene Data
    
    // Battle
    private BattleSO m_CurrentBattle;
    private List<PlayerCharacterBattleData> m_UnitBattleData;
    private GameObject m_MapBiome;

    #endregion

    #region Curr State
    private SceneEnum m_CurrScene = SceneEnum.MAIN_MENU;
    public SceneEnum CurrScene => m_CurrScene;
    private int m_CurrLevelId;
    #endregion

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();
        transform.SetParent(null);
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Scene Management
    public void ReturnToWorldMap()
    {
        FlagManager.Instance.SetFlagValue(Flag.QUIT_LEVEL_FLAG, true, FlagType.SESSION);
        m_OnSceneChange = CameraManager.Instance.SetUpLevelCamera;
        m_OnSceneChange += () => m_WorldLight.gameObject.SetActive(true);
        m_AfterSceneChange = () => GlobalEvents.Level.ReturnFromLevelEvent?.Invoke();
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.WORLD_MAP;

        switch (m_CurrScene)
        {
            case SceneEnum.BATTLE:
                StartCoroutine(UnloadMultipleAdditiveScenesWithTransition(new[] {LEVEL_1_SCENE_INDEX + m_CurrLevelId, BATTLE_SCENE_INDEX}));
                break;
            case SceneEnum.LEVEL:
                StartCoroutine(UnloadMultipleAdditiveScenesWithTransition(new[] {LEVEL_1_SCENE_INDEX + m_CurrLevelId}));
                break;
            default:
                Logger.Log(this.GetType().Name, "Not in a scene that can return to the world map", LogLevel.ERROR);
                break;
        }
    }

    public void LoadMainMenuScene()
    {
        m_OnSceneChange = () => m_CurrScene = SceneEnum.MAIN_MENU;
        m_OnSceneChange += () => GlobalEvents.Scene.MainMenuSceneLoadedEvent?.Invoke();
        StartCoroutine(LoadScene_NonAdditive(MAIN_MENU_INDEX));
    }

    public void LoadWorldMapScene()
    {
        m_OnSceneChange = () => m_CurrScene = SceneEnum.WORLD_MAP;
        m_OnSceneChange += () => GlobalEvents.Scene.WorldMapSceneLoadedEvent?.Invoke();
        StartCoroutine(LoadScene_NonAdditive(WORLD_MAP_INDEX));
    }

    private IEnumerator LoadScene_NonAdditive(int sceneIndex)
    {
        m_Transition.SetTrigger(Start);
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        var asyncHandle = SceneManager.LoadSceneAsync(sceneIndex);
        void OnSceneLoadComplete(AsyncOperation handle)
        {
            asyncHandle.completed -= OnSceneLoadComplete;

            m_OnSceneChange?.Invoke();
            m_OnSceneChange = null;
            
            m_Transition.SetTrigger(End);
        }
        asyncHandle.completed += OnSceneLoadComplete;
    }

    public void LoadLevelScene(int levelId, List<PlayerCharacterData> partyMembers)
    {
        m_CurrLevelId = levelId;
        LevelManager.OnReady += OnLevelManagerReady;
        m_WorldLight = FindAnyObjectByType<Light>(FindObjectsInactive.Exclude);
        m_OnSceneChange += () => m_WorldLight.gameObject.SetActive(false);
        
        var sceneIndex = LEVEL_1_SCENE_INDEX + levelId;
        StartCoroutine(LoadAdditiveSceneWithTransition(sceneIndex));
        return;
        
        void OnLevelManagerReady(LevelManager levelManager)
        {
            LevelManager.OnReady -= OnLevelManagerReady;
            m_CurrScene = SceneEnum.LEVEL;
        
            Debug.Log("Level scene loaded. Initialising level.");
            levelManager.Initialise(partyMembers);
        }
    }
    
    public void UnloadLevelScene(int levelId)
    {
        m_OnSceneChange = () => m_WorldLight.gameObject.SetActive(true);
        m_AfterSceneChange = () => GlobalEvents.Level.ReturnFromLevelEvent?.Invoke();
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.WORLD_MAP;

        // Unload the level scene
        var sceneIndex = LEVEL_1_SCENE_INDEX + levelId;
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
        m_LevelLight = FindAnyObjectByType<Light>(FindObjectsInactive.Exclude);
        m_OnSceneChange += () => m_LevelLight.gameObject.SetActive(false);
        m_OnSceneChange += () => m_CurrScene = SceneEnum.BATTLE;

        // Load the battle scene
        StartCoroutine(LoadAdditiveSceneWithTransition(BATTLE_SCENE_INDEX));
    }
    
    public void UnloadBattleScene()
    {
        m_OnSceneChange = CameraManager.Instance.SetUpLevelCamera;
        m_OnSceneChange += () => m_LevelLight.gameObject.SetActive(true);
        
        m_AfterSceneChange = () => GlobalEvents.Battle.ReturnFromBattleEvent?.Invoke();
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.LEVEL;
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
    IEnumerator UnloadMultipleAdditiveScenesWithTransition(params int[] sceneIndexes)
    {
        int numHandles = sceneIndexes.Count();
        int numHandlesLoaded = 0;

        m_Transition.SetTrigger(Start);
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        foreach (int sceneIndex in sceneIndexes)
        {
            var asyncHandle = SceneManager.UnloadSceneAsync(sceneIndex);
            asyncHandle.completed += OnSceneLoadComplete;
        }
        
        void OnSceneLoadComplete(AsyncOperation handle)
        {
            handle.completed -= OnSceneLoadComplete;

            numHandlesLoaded += 1;
            
            if (numHandles < numHandlesLoaded)
                return;

            m_OnSceneChange?.Invoke();
            m_OnSceneChange = null;

            m_Transition.SetTrigger(End);
            
            m_AfterSceneChange?.Invoke();
            m_AfterSceneChange = null;
        }
    }

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
