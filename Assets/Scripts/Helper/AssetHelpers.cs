using System.Collections.Generic;
using UnityEditor;

public static class AssetHelpers
{
#if UNITY_EDITOR
    public static IEnumerable<string> FindAssetPathsByType<T>(bool overrideRootFolder, params string[] foldersToSearch) {
        return FindAssetPathsByType($"t:{typeof(T)}", overrideRootFolder, foldersToSearch);
    }

    public static IEnumerable<string> FindAssetPathsByType(string filterString, bool overrideRootFolder, params string[] foldersToSearch)
    {
        string[] guids = overrideRootFolder ? AssetDatabase.FindAssets(filterString, foldersToSearch) : AssetDatabase.FindAssets(filterString);
        foreach (string guid in guids) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            yield return assetPath;
        }
    }
#endif
}
