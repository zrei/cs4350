using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "StartingDataSetter", menuName = "ScriptableObject/StartingData/StartingDataSetter")]
public class StartingDataSetter : ScriptableObject
{
    [Header("Managers")]
    public InventoryManager m_InventoryManager;
    public CharacterDataManager m_CharacterDataManager;
    public FlagManager m_FlagManager;
    public WorldMapManager m_WorldMapManager;
    public MoralityManager m_MoralityManager;

    [Header("Starting Data")]
    public StartingDataSO m_StartingData;

    public void SetStartingData()
    {
        m_InventoryManager.SetStartingInventory(m_StartingData.m_StartingWeapons);
        EditorUtility.SetDirty(m_InventoryManager.gameObject);

        m_CharacterDataManager.SetStartingCharacters(m_StartingData.m_StartingCharacters);
        EditorUtility.SetDirty(m_CharacterDataManager.gameObject);

        m_FlagManager.SetStartingFlags(m_StartingData.m_StartingPersistentFlags);
        EditorUtility.SetDirty(m_FlagManager.gameObject);

        m_WorldMapManager.SetStartingLevel(m_StartingData.m_StartingLevel);
        EditorUtility.SetDirty(m_WorldMapManager.gameObject);

        m_MoralityManager.SetStartingMorality(m_StartingData.m_StartingMoralityPercentage);
        EditorUtility.SetDirty(m_MoralityManager.gameObject);
    }
}
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(StartingDataSetter))]
public class StartingDataSetterHelper : Editor
{
    private StartingDataSetter m_Target;

    private void OnEnable()
    {
        m_Target = (StartingDataSetter) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Clear player prefs"))
        {
            PlayerPrefs.DeleteAll();
        }

        if (GUILayout.Button("Set starting data"))
        {
            m_Target.SetStartingData();
        }

        GUILayout.Label("You must save project to save the changes!");
    }
}
#endif