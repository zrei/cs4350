using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

// hack to get around the generic
public interface IIdHelper
{
    public void CheckForDuplicateIds();

    public void RenumberIds();

    public void ClearPlayerPrefs();
}

#if UNITY_EDITOR
public abstract class IdHelper<T> : ScriptableObject, IIdHelper where T : ScriptableObject
{
    [Tooltip("Whether to limit the search to a separate root folder instead of searching through the entire Assets folder")]
    public bool m_OverrideRootFolder;
    [Tooltip("The root folder to limit the search to - Assets is the top-level folder for the project")]
    public string m_OverriddenRootFolder = "Assets/Persistent Data";

    protected virtual string InstanceSoName => "InstanceSO";

    public void CheckForDuplicateIds()
    {
        Dictionary<int, List<string>> idMap = new();
        foreach (string instancePath in FindAssetPathsByType())
        {
            T instanceSO = AssetDatabase.LoadAssetAtPath<T>(instancePath);
            
            if (instanceSO == null)
                continue;

            int instanceId = GetInstanceSoId(instanceSO);
            if (!idMap.ContainsKey(instanceId))
                idMap[instanceId] = new();
            idMap[instanceId].Add(instancePath);
        }

        foreach (KeyValuePair<int, List<string>> keyValuePair in idMap)
        {
            if (keyValuePair.Value.Count <= 1)
                continue;
            
            StringBuilder stringBuilder = new($"\n{InstanceSoName}s at paths:\n\n");
            foreach (string s in keyValuePair.Value)
            {
                stringBuilder.Append(s + "\n");
            }
            stringBuilder.Append($"\nhave the same id {keyValuePair.Key}\n");
            Logger.Log(this.GetType().Name, stringBuilder.ToString(), LogLevel.WARNING);
        }
    }

    public void RenumberIds()
    {
        int id = 0;

        foreach (string instancePath in FindAssetPathsByType())
        {
            T instanceSO = AssetDatabase.LoadAssetAtPath<T>(instancePath);
            
            if (instanceSO == null)
                continue;

            EditInstanceSoId(instanceSO, id);
            Logger.Log(this.GetType().Name, $"Set {InstanceSoName} {instanceSO.name} at location {instancePath} id to {id}", LogLevel.LOG);
            EditorUtility.SetDirty(instanceSO);
            ++id;
        }
    }

    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    protected abstract int GetInstanceSoId(T instanceSO);

    protected abstract void EditInstanceSoId(T instanceSO, int newId);

    private IEnumerable<string> FindAssetPathsByType() {
        string[] guids = m_OverrideRootFolder
            ? AssetDatabase.FindAssets($"t:{typeof(T)}", new string[] {m_OverriddenRootFolder})
            : AssetDatabase.FindAssets($"t:{typeof(T)}");
        foreach (string guid in guids) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            yield return assetPath;
        }
    }
}
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(IdHelper<>), true)]
public class IdHelperEditor : Editor
{
    IIdHelper idHelper;

    void OnEnable()
    {
        idHelper = (IIdHelper) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(30f);

        if (GUILayout.Button("Check for duplicate IDs\n(May be slow for a project-wide search)"))
            idHelper.CheckForDuplicateIds();
        
        GUILayout.Space(15f);

        if (GUILayout.Button("Replace all IDs\n(This can break existing save data)"))
        {
            idHelper.RenumberIds();
        }

        GUILayout.Space(15f);
        
        if (GUILayout.Button("Clear player prefs"))
        {
            idHelper.ClearPlayerPrefs();
        }

        GUILayout.Space(30f);

        GUILayout.Label("If you've changed SO data, remember to\nFile > Save Project\nif you want to register your changes");
    }
}
#endif
