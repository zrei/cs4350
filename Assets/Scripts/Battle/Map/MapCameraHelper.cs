#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Camera))]
public class MapCameraHelper : MonoBehaviour
{
    [SerializeField] Transform m_LookAtPoint;

    public void AdjustCamera()
    {
        if (m_LookAtPoint != null)
            transform.LookAt(m_LookAtPoint);
    }
}

[CustomEditor(typeof(MapCameraHelper))]
public class MapCameraHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Adjust camera look at"))
        {
            ((MapCameraHelper) target).AdjustCamera();
        }
    }
}
#endif