using UnityEditor;
using UnityEngine;

[ContextMenu()]
/// <summary>
/// Helps to: check for identical IDs, reset IDs, clear player prefs
/// Should warn that changing IDs can break player prefs
/// </summary>
public class IdHelper : EditorWindow
{
    public override void OnGUI()
    {
        GUILayout.Label("Note that changing all IDs can break existing save data");
    }
}
