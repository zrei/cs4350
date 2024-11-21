using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
public class SceneLoadTool : EditorWindow
{
    private string m_MainMenuPath = "Assets/Scenes/MainMenuScene";
    private string m_WorldMapPath = "Assets/Scenes/WorldMapScene";
    private string m_LevelPath = "Assets/Scenes/Level{0}Scene";
    private string m_BattlePath = "Assets/Scenes/BattleScene";

    private string m_CurrentScenePath; 
    private const string SCENE_PATH_FORMAT = "{0}.unity";

    private int m_LevelNumber = 1;

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

        GUILayout.Space(10f);
        m_BattlePath = EditorGUILayout.TextField("Battle Scene Path", m_BattlePath);
        if (GUILayout.Button("Load Battle Scene"))
        {
            LoadScene(string.Format(SCENE_PATH_FORMAT, m_BattlePath));
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

    private void LoadScene(string scenePath, VoidEvent postEvent = null)
    {
        if (EditorApplication.isPlaying)
        {
            Logger.LogEditor(this.GetType().Name, "Application is playing - Cancel scene action", LogLevel.ERROR);
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
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