using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneEnum
{
    MAIN_MENU,
    WORLD_MAP,
    LEVEL,
    BATTLE
}

public enum BattleMapType
{
    PLAINS,
    FOREST,
    CAVE,
    DESOLATE,
    DESSERT,
    LAVA
}

/// <summary>
/// Singleton class for managing game scenes.
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager>
{   
    [SerializeField] Animator m_Transition;
    [SerializeField] float m_TransitionTime = 1f;
    
    private static readonly int Start = Animator.StringToHash("Start");
    private static readonly int End = Animator.StringToHash("End");

    private const string BATTLE_SCENE_PATH = "BattleScene_{0}";

    private delegate AsyncOperation SceneLoadOperation();
    
    private const int MAIN_MENU_INDEX = 0;
    private const int WORLD_MAP_INDEX = 1;
    // Respective level scenes indexes will accessed by adding the level id to the level 1 value
    private const int LEVEL_1_SCENE_INDEX = 3;
    
    private VoidEvent m_OnSceneChange;
    private VoidEvent m_AfterSceneChange;
    
    #region Temporary Scene Data
    private Light m_WorldLight;
    private Light m_LevelLight;
    #endregion

    #region Curr State
    private SceneEnum m_CurrScene = SceneEnum.MAIN_MENU;
    public SceneEnum CurrScene => m_CurrScene;
    private int m_CurrLevelId;
    private BattleMapType m_CurrBiome;
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
        GlobalEvents.Scene.OnBeginSceneChange?.Invoke(m_CurrScene, SceneEnum.WORLD_MAP);

        m_OnSceneChange = CameraManager.Instance.SetUpLevelCamera;
        m_OnSceneChange += () => m_WorldLight.gameObject.SetActive(true);
        m_OnSceneChange += () => GlobalEvents.Scene.OnSceneTransitionEvent?.Invoke(SceneEnum.WORLD_MAP);
        m_AfterSceneChange = () => GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(m_CurrScene, SceneEnum.WORLD_MAP);
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.WORLD_MAP;

        switch (m_CurrScene)
        {
            case SceneEnum.BATTLE:
                UnloadMultipleAdditiveScenesWithTransition(new[] {LEVEL_1_SCENE_INDEX + m_CurrLevelId}, new[] {string.Format(BATTLE_SCENE_PATH, m_CurrBiome)});
                break;
            case SceneEnum.LEVEL:
                UnloadMultipleAdditiveScenesWithTransition(new[] {LEVEL_1_SCENE_INDEX + m_CurrLevelId}, new string[] {});
                break;
            default:
                Logger.Log(this.GetType().Name, "Not in a scene that can return to the world map", LogLevel.ERROR);
                break;
        }
    }

    public void LoadMainMenuScene()
    {
        GlobalEvents.Scene.OnBeginSceneChange?.Invoke(m_CurrScene, SceneEnum.MAIN_MENU);
        m_OnSceneChange = () => GlobalEvents.Scene.OnSceneTransitionEvent?.Invoke(SceneEnum.MAIN_MENU);
        m_AfterSceneChange = () => GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(m_CurrScene, SceneEnum.MAIN_MENU);
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.MAIN_MENU;
        LoadScene_NonAdditive(MAIN_MENU_INDEX);
    }

    public void LoadWorldMapScene()
    {
        GlobalEvents.Scene.OnBeginSceneChange?.Invoke(m_CurrScene, SceneEnum.WORLD_MAP);
        m_OnSceneChange = () => GlobalEvents.Scene.OnSceneTransitionEvent?.Invoke(SceneEnum.WORLD_MAP);
        m_AfterSceneChange = () => GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(m_CurrScene, SceneEnum.WORLD_MAP);
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.WORLD_MAP;
        LoadScene_NonAdditive(WORLD_MAP_INDEX);
    }

    public void LoadLevelScene(int levelId, List<PlayerCharacterData> partyMembers)
    {
        GlobalEvents.Scene.OnBeginSceneChange?.Invoke(m_CurrScene, SceneEnum.LEVEL);

        m_CurrLevelId = levelId;
        LevelManager.OnReady += OnLevelManagerReady;
        m_WorldLight = FindAnyObjectByType<Light>(FindObjectsInactive.Exclude);
        m_OnSceneChange = () => m_WorldLight.gameObject.SetActive(false);
        m_OnSceneChange += () => GlobalEvents.Scene.OnSceneTransitionEvent?.Invoke(SceneEnum.LEVEL);
        m_AfterSceneChange = () => GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(m_CurrScene, SceneEnum.LEVEL);
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.LEVEL;
        
        var sceneIndex = LEVEL_1_SCENE_INDEX + levelId;
        LoadAdditiveSceneWithTransition(sceneIndex);
        
        void OnLevelManagerReady()
        {
            LevelManager.OnReady -= OnLevelManagerReady;
            m_CurrScene = SceneEnum.LEVEL;
        
            Debug.Log("Level scene loaded. Initialising level.");
            LevelManager.Instance.Initialise(partyMembers);
        }
    }

    public void UnloadLevelScene(int levelId)
    {
        GlobalEvents.Scene.OnBeginSceneChange?.Invoke(m_CurrScene, SceneEnum.WORLD_MAP);

        m_OnSceneChange = () => m_WorldLight.gameObject.SetActive(true);
        m_OnSceneChange += () => GlobalEvents.Scene.OnSceneTransitionEvent?.Invoke(SceneEnum.WORLD_MAP);
        m_AfterSceneChange = () => GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(m_CurrScene, SceneEnum.WORLD_MAP);
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.WORLD_MAP;

        // Unload the level scene
        var sceneIndex = LEVEL_1_SCENE_INDEX + levelId;
        UnloadAdditiveSceneWithTransition(sceneIndex);
    }

    public void LoadBattleScene(BattleSO battleSo, List<PlayerCharacterBattleData> unitBattleData, BattleMapType mapBiome, List<InflictedToken> fatigueTokens)
    {
        GlobalEvents.Scene.OnBeginSceneChange?.Invoke(m_CurrScene, SceneEnum.BATTLE);
        m_CurrBiome = mapBiome;

        // Set up the callback to initialize battle parameters for when the battle scene is loaded
        BattleManager.OnReady += OnBattleSceneLoaded;
        
        m_OnSceneChange = CameraManager.Instance.SetUpBattleCamera;
        m_LevelLight = FindAnyObjectByType<Light>(FindObjectsInactive.Exclude);
        m_OnSceneChange += () => m_LevelLight.gameObject.SetActive(false);
        m_OnSceneChange += () => GlobalEvents.Scene.OnSceneTransitionEvent?.Invoke(SceneEnum.BATTLE);
        m_AfterSceneChange = () => GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(m_CurrScene, SceneEnum.BATTLE);
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.BATTLE;

        // Load the battle scene
        LoadAdditiveSceneWithTransition(string.Format(BATTLE_SCENE_PATH, mapBiome));
        
        void OnBattleSceneLoaded()
        {
            BattleManager.OnReady -= OnBattleSceneLoaded;
        
            Debug.Log("Battle scene loaded. Initialising battle.");
            BattleManager.Instance.InitialiseBattle(battleSo, unitBattleData, fatigueTokens);
        }
    }
    
    public void UnloadBattleScene()
    {
        GlobalEvents.Scene.OnBeginSceneChange?.Invoke(m_CurrScene, SceneEnum.LEVEL);
        
        m_OnSceneChange = CameraManager.Instance.SetUpLevelCamera;
        m_OnSceneChange += () => m_LevelLight.gameObject.SetActive(true);
        m_OnSceneChange += () => GlobalEvents.Scene.OnSceneTransitionEvent?.Invoke(SceneEnum.LEVEL);
        
        m_AfterSceneChange = () => GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(m_CurrScene, SceneEnum.LEVEL);
        m_AfterSceneChange += () => m_CurrScene = SceneEnum.LEVEL;

        // Unload the battle scene
        UnloadAdditiveSceneWithTransition(string.Format(BATTLE_SCENE_PATH, m_CurrBiome));
    }

    #endregion

    #region Scene Load
    private void LoadScene_NonAdditive(int sceneIndex)
    {
        StartCoroutine(LoadSceneWithTransition_Coroutine(() => SceneManager.LoadSceneAsync(sceneIndex)));
    }

    private void LoadAdditiveSceneWithTransition(int sceneIndex)
    {
        StartCoroutine(LoadSceneWithTransition_Coroutine(() => SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive)));
    }

    private void LoadAdditiveSceneWithTransition(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition_Coroutine(() => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)));
    }
    
    private void UnloadAdditiveSceneWithTransition(int sceneIndex)
    {
        UnloadMultipleAdditiveScenesWithTransition(new int[] {sceneIndex}, new string[] {});
    }

    private void UnloadAdditiveSceneWithTransition(string sceneName)
    {
        UnloadMultipleAdditiveScenesWithTransition(new int[] {}, new string[] {sceneName});
    }

    private void UnloadMultipleAdditiveScenesWithTransition(int[] sceneIndexes, string[] sceneNames)
    {
        StartCoroutine(UnloadMultipleAdditiveScenesWithTransition_Coroutine(sceneIndexes, sceneNames));
    }
    #endregion

    #region Transition
    public void PlayTransition(VoidEvent midTransitionAction, VoidEvent postTransitionAction)
    {
        StartCoroutine(PlayTransition_Coroutine(midTransitionAction, postTransitionAction, m_TransitionTime));
    }
    #endregion

    #region Transition Coroutines
    private IEnumerator UnloadMultipleAdditiveScenesWithTransition_Coroutine(int[] sceneIndexes, string[] sceneNames)
    {
        int numHandles = sceneIndexes.Count() + sceneNames.Count();
        int numHandlesLoaded = 0;

        m_Transition.SetTrigger(Start);
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        foreach (int sceneIndex in sceneIndexes)
        {
            var asyncHandle = SceneManager.UnloadSceneAsync(sceneIndex);
            if (asyncHandle != null)
            {
                asyncHandle.completed += OnSceneLoadComplete;
            }
        }

        foreach (string sceneName in sceneNames)
        {
            var asyncHandle = SceneManager.UnloadSceneAsync(sceneName);
            if (asyncHandle != null)
            {
                asyncHandle.completed += OnSceneLoadComplete;
            }
        }
        
        void OnSceneLoadComplete(AsyncOperation handle)
        {
            handle.completed -= OnSceneLoadComplete;

            numHandlesLoaded += 1;
            
            if (numHandlesLoaded < numHandles)
                return;

            // clear all still open screens
            UIScreenManager.Instance.ClearScreen();

            m_OnSceneChange?.Invoke();
            m_OnSceneChange = null;

            m_Transition.SetTrigger(End);
            
            m_AfterSceneChange?.Invoke();
            m_AfterSceneChange = null;
        }
    }

    /// <summary>
    /// Does not care if it's additive or not additive, the caller decides
    /// </summary>
    /// <param name="sceneLoadOperation"></param>
    /// <returns></returns>
    private IEnumerator LoadSceneWithTransition_Coroutine(SceneLoadOperation sceneLoadOperation)
    {
        m_Transition.SetTrigger(Start);
        
        yield return new WaitForSeconds(m_TransitionTime);
        
        var asyncHandle = sceneLoadOperation();
        
        if (asyncHandle != null)
        {
            asyncHandle.completed += OnSceneLoadComplete;
        }

        void OnSceneLoadComplete(AsyncOperation handle)
        {
            handle.completed -= OnSceneLoadComplete;

            // clear all still open screens
            UIScreenManager.Instance.ClearScreen();
            
            m_OnSceneChange?.Invoke();
            m_OnSceneChange = null;
            
            m_Transition.SetTrigger(End);

            m_AfterSceneChange?.Invoke();
            m_AfterSceneChange = null;
        }        
    }

    private IEnumerator PlayTransition_Coroutine(VoidEvent midTransitionAction, VoidEvent postTransitionAction, float transitionTime)
    {
        m_Transition.SetTrigger(Start);
        
        yield return new WaitForSeconds(transitionTime);

        midTransitionAction?.Invoke();

        m_Transition.SetTrigger(End);

        yield return new WaitForSeconds(transitionTime);

        postTransitionAction?.Invoke();
    }
    #endregion
    
}
