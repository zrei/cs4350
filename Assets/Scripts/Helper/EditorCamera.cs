#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;

/// <summary>
/// Blows itself up in an actual game run
/// </summary>
[RequireComponent(typeof(Camera))]
public class EditorCamera : MonoBehaviour
{
    private void Awake()
    {
        Destroy(this.gameObject);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EditorCamera))]
public class EditorCameraEditor : Editor
{
    private Camera m_Camera;
    private LayerMask m_LayerMask;

    private void OnEnable()
    {
        m_Camera = ((EditorCamera) target).GetComponent<Camera>();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10f);

        if (GUILayout.Button("Set up level camera"))
        {
            m_Camera.orthographic = true;
            EditorUtility.SetDirty((EditorCamera) target);
        }

        if (GUILayout.Button("Set up battle camera"))
        {
            m_Camera.orthographic = false;
            EditorUtility.SetDirty((EditorCamera) target);
        }

        GUILayout.Space(10f);
        m_LayerMask = EditorGUILayout.MaskField( InternalEditorUtility.LayerMaskToConcatenatedLayersMask(m_LayerMask), InternalEditorUtility.layers);
        if (GUILayout.Button("Set culling mask"))
        {
            m_Camera.cullingMask = m_LayerMask;
            EditorUtility.SetDirty((EditorCamera) target);
        }
    }
}
#endif