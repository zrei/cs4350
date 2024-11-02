using System.Collections.Generic;
using UnityEditor;

public static class AssetHelpers
{
#if UNITY_EDITOR
    public static IEnumerable<string> FindAssetPathsByType<T>(bool overrideRootFolder, params string[] foldersToSearch) {
        string[] guids = overrideRootFolder ? AssetDatabase.FindAssets($"t:{typeof(T)}", foldersToSearch) : AssetDatabase.FindAssets($"t:{typeof(T)}");
        foreach (string guid in guids) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            yield return assetPath;
        }
    }
#endif
}