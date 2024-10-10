using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class IdHelper : ScriptableObject
{
    [Tooltip("Whether to limit the search to a separate root folder instead of searching through the entire Assets folder")]
    public bool m_UseFilterFolder;
    [Tooltip("The root folder to limit the search to - Assets is the top-level folder for the project")]
    public string m_RootFolder;

    public abstract void CheckForDuplicateIds();

    public abstract void RenumberIds();

    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public IEnumerable<string> FindAssetPathsByType<T>() where T : Object {
        string[] guids = m_UseFilterFolder
            ? AssetDatabase.FindAssets($"t:{typeof(T)}", new string[] {m_RootFolder})
            : AssetDatabase.FindAssets($"t:{typeof(T)}");
        foreach (string guid in guids) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            yield return assetPath;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(IdHelper), true)]
public class IdHelperEditor : Editor
{
    IdHelper idHelper;

    void OnEnable()
    {
        idHelper = (IdHelper) target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(30f);

        if (GUILayout.Button("Check for duplicate IDs - may be slow for a project-wide search"))
            idHelper.CheckForDuplicateIds();
        
        GUILayout.Space(15f);

        if (GUILayout.Button("Replace all IDs - note: this can break existing save data"))
        {
            idHelper.RenumberIds();
        }

        GUILayout.Space(15f);
        
        if (GUILayout.Button("Clear player prefs"))
        {
            idHelper.ClearPlayerPrefs();
        }
    }
}
#endif
