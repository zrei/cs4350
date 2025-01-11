#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SceneLoadTool : EditorWindow
{
    private string m_MainMenuPath = "Assets/Scenes/MainMenuScene";
    private string m_WorldMapPath = "Assets/Scenes/WorldMapScene";
    private string m_LevelPath = "Assets/Scenes/Level{0}Scene";
    private string m_BattlePath = "Assets/Scenes/BattleScene_{0}";

    private string m_CurrentScenePath; 
    private const string SCENE_PATH_FORMAT = "{0}.unity";

    private int m_LevelNumber = 1;
    private int m_BattleMapBiomeIndex = 0;
    
    private string m_TestBattleAdditivePath = "Assets/Scenes/TestScenes/TestBattleAdditiveScene";
    private string m_TestLevelAdditivePath = "Assets/Scenes/TestScenes/TestLevelAdditiveScene";
    private string m_SetupBattlePath = "Assets/Scenes/BattleSetupScene";

    [MenuItem("Window/Scene Load Tool")]
    public static void ShowSceneLoadWindow()
    {
        SceneLoadTool wnd = GetWindow<SceneLoadTool>();
        wnd.titleContent = new GUIContent("SceneLoadTool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Load Scenes");
        m_MainMenuPath = EditorGUILayout.TextField("Main Menu Scene Path", m_MainMenuPath);
        if (GUILayout.Button("Load Menu Scene"))
        {
            LoadScene(string.Format(SCENE_PATH_FORMAT, m_MainMenuPath));
        }

        GUILayout.Space(10f);
        m_WorldMapPath = EditorGUILayout.TextField("World Map Scene Path", m_WorldMapPath);
        if (GUILayout.Button("Load World Map Scene"))
        {
            LoadScene(string.Format(SCENE_PATH_FORMAT, m_WorldMapPath));
        }

        GUILayout.Space(10f);
        m_LevelPath = EditorGUILayout.TextField("Level Scene Path", m_LevelPath);
        m_LevelNumber = EditorGUILayout.IntField("Level Number", m_LevelNumber);
        if (GUILayout.Button($"Load Level {m_LevelNumber} Scene"))
        {
            LoadScene(string.Format(SCENE_PATH_FORMAT, string.Format(m_LevelPath, m_LevelNumber)));
        }
        GUILayout.Space(5f);
        m_TestLevelAdditivePath = EditorGUILayout.TextField("Test Level Additive Scene Path", m_TestLevelAdditivePath);
        if (GUILayout.Button($"Load Test Level Additive Scene"))
        {
            LoadScene(string.Format(SCENE_PATH_FORMAT, string.Format(m_TestLevelAdditivePath)));
        }
        if (GUILayout.Button($"Load Game in Level {m_LevelNumber}"))
        {
            m_CurrentScenePath = EditorSceneManager.GetActiveScene().path;
            var m_LevelScenePath = string.Format(SCENE_PATH_FORMAT, string.Format(m_LevelPath, m_LevelNumber));
            var m_TestLevelAdditiveScenePath = string.Format(SCENE_PATH_FORMAT, m_TestLevelAdditivePath);
            LoadScene(m_LevelScenePath, () => LoadTestScene(m_TestLevelAdditiveScenePath, PlayScene, OpenSceneMode.Additive));
        }

        GUILayout.Space(10f);
        m_BattlePath = EditorGUILayout.TextField("Battle Scene Path", m_BattlePath);

        IEnumerable<BattleMapType> battleMapBiomes = Enum.GetValues(typeof(BattleMapType)).OfType<BattleMapType>();
        string[] options = battleMapBiomes.Select(x => x.ToString()).ToArray();
        m_BattleMapBiomeIndex = EditorGUILayout.Popup("Battle Map Biome", m_BattleMapBiomeIndex, options);
        BattleMapType finalBiome = battleMapBiomes.ElementAt(m_BattleMapBiomeIndex);
        m_SetupBattlePath = EditorGUILayout.TextField("Battle Setup Scene Path", m_SetupBattlePath);
        if (GUILayout.Button($"Load {finalBiome} Battle Scene"))
        {
            LoadScene(string.Format(SCENE_PATH_FORMAT, string.Format(m_BattlePath, finalBiome)));
        }
        if (GUILayout.Button($"Load Setup Battle Scene"))
        {
            LoadScene(string.Format(SCENE_PATH_FORMAT, m_SetupBattlePath));
        }
        GUILayout.Space(5f);
        m_TestBattleAdditivePath = EditorGUILayout.TextField("Test Battle Additive Scene Path", m_TestBattleAdditivePath);
        if (GUILayout.Button($"Load Test Battle Additive Scene"))
        {
            LoadScene(string.Format(SCENE_PATH_FORMAT, string.Format(m_TestBattleAdditivePath)));
        }
        if (GUILayout.Button($"Load Game in Battle Biome {finalBiome}"))
        {
            m_CurrentScenePath = EditorSceneManager.GetActiveScene().path;
            var m_BattleScenePath = string.Format(SCENE_PATH_FORMAT, string.Format(m_BattlePath, finalBiome));
            var m_TestBattleAdditiveScenePath = string.Format(SCENE_PATH_FORMAT, m_TestBattleAdditivePath);
            LoadScene(m_BattleScenePath, () => LoadTestScene(m_TestBattleAdditiveScenePath, PlayScene, OpenSceneMode.Additive));
        }

        GUILayout.Space(30f);
        GUILayout.Label("Load Game");
        if (GUILayout.Button("Load Game"))
        {
            m_CurrentScenePath = EditorSceneManager.GetActiveScene().path;
            LoadScene(string.Format(SCENE_PATH_FORMAT, m_MainMenuPath), PlayScene);
        }
        if (GUILayout.Button("Stop - does normal editor stop"))
        {
            Stop();
        }
        if (GUILayout.Button("Stop at Previously Opened Scene"))
        {
            Stop(m_CurrentScenePath);
        }
        if (GUILayout.Button("Stop at Current Scene\nThis doesn't work super well with async scenes"))
        {
            StopAtCurrentScene();
        }
    }

    private void LoadTestScene(string scenePath, VoidEvent postEvent = null, OpenSceneMode mode = OpenSceneMode.Single)
    {
        LoadScene(scenePath, PostLoad, mode);

        void PostLoad()
        {
            TestSceneInitialiser testSceneInitialiser = UnityEngine.Object.FindObjectsOfType(typeof(TestSceneInitialiser))[0].GetComponent<TestSceneInitialiser>();
            testSceneInitialiser.SetTestValues();
            postEvent?.Invoke();
        }
    }

    private void LoadScene(string scenePath, VoidEvent postEvent = null, OpenSceneMode mode = OpenSceneMode.Single)
    {
        if (EditorApplication.isPlaying)
        {
            Logger.LogEditor(this.GetType().Name, "Application is playing - Cancel scene action", LogLevel.ERROR);
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath, mode);
            postEvent?.Invoke();
        }
        else
        {
            Logger.LogEditor(this.GetType().Name, "Cancel scene action", LogLevel.LOG);
        }
    }

    private void PlayScene()
    {
        EditorApplication.EnterPlaymode();
    }

    private void StopAtCurrentScene()
    {
        if (!EditorApplication.isPlaying)
        {
            Logger.LogEditor(this.GetType().Name, "Application is not playing - Cancel stop", LogLevel.WARNING);
            return;
        }

        Stop(SceneManager.GetActiveScene().path);
    }

    private void Stop(string scenePathToReturnTo = null)
    {
        if (!EditorApplication.isPlaying)
        {
            Logger.LogEditor(this.GetType().Name, "Application is not playing - Cancel stop", LogLevel.WARNING);
            return;
        }

        EditorApplication.playModeStateChanged += PostStop;
        EditorApplication.ExitPlaymode();

        void PostStop(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.EnteredEditMode)
                return;

            EditorApplication.playModeStateChanged -= PostStop;
            if (!string.IsNullOrEmpty(scenePathToReturnTo))
            {
                LoadScene(scenePathToReturnTo);
            }
            m_CurrentScenePath = null;
        }
    }
}
#endif