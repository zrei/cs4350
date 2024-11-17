using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class EdgeVisual : MonoBehaviour
{
    [SerializeField] EdgeInternal m_EdgeInternal;
    
    [SerializeField] public LineRenderer m_LineRenderer;
    [SerializeField] public TextMeshPro m_CostText;
    
    [Header("Edge Renderer Settings")]
    public float width;
    public Material material;
    [Range(16, 512)]
    public int subdivisions;
    
    #region Initialisation
    
    public void Initialise(EdgeInternal edgeInternal)
    {
        m_EdgeInternal = edgeInternal;
        
        // Draws a line to connect the nodes
        // DrawEdge();
    }
    
    #endregion
    
#if UNITY_EDITOR

    public void DrawEdge()
    {
        m_LineRenderer.startWidth = width;
        m_LineRenderer.endWidth = width;
        m_LineRenderer.material = material;
        
        m_LineRenderer.positionCount = subdivisions + 1;
        
        for (var i = 0; i <= subdivisions; i++)
        {
            var t = (float) i / subdivisions;
            var position = m_EdgeInternal.SplineContainer.EvaluatePosition(t);
            m_LineRenderer.SetPosition(i, position);
        }
        
        // Set Text position to the middle of the line
        m_CostText.transform.position = 
            (m_EdgeInternal.NodeInternalA.transform.position + m_EdgeInternal.NodeInternalB.transform.position) / 2 
            + new Vector3(0f, 0.5f, 0f);
        m_CostText.text = m_EdgeInternal.Cost.ToString();
        m_CostText.enabled = true;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(EdgeVisual)), CanEditMultipleObjects]
public class EdgeVisualEditor : Editor
{
    EdgeVisual m_Target;
    EdgeVisual[] m_Targets;

    private void OnEnable()
    {
        if (targets.Length == 1)
            m_Target = (EdgeVisual) target;
        else
        {
            m_Targets = new EdgeVisual[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                m_Targets[i] = (EdgeVisual) targets[i];
            }
        }
    }

    public override void OnInspectorGUI()
    {   
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Draw Edge"))
        {
            if (m_Targets == null)
                DrawEdgeFromEditor(m_Target);
            else
            {
                foreach (var edgeVisual in m_Targets)
                {
                    DrawEdgeFromEditor(edgeVisual);
                }
            }
        }
    }

    private void DrawEdgeFromEditor(EdgeVisual edgeVisual)
    {
        edgeVisual.DrawEdge();
            
        Undo.RecordObject(edgeVisual.m_LineRenderer, "Updated LineRenderer");
        PrefabUtility.RecordPrefabInstancePropertyModifications(edgeVisual.m_LineRenderer);
            
        Undo.RecordObject(edgeVisual.m_CostText, "Updated CostText");
        PrefabUtility.RecordPrefabInstancePropertyModifications(edgeVisual.m_CostText);
    }
}
#endif
