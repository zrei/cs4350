using Level.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Class that maintains the internal representation of an edge in the graph
/// (edge cost and connecting nodes),
/// </summary>
public class EdgeInternal : BaseEdge
{
    [Header("Level Edge")]
    [SerializeField] private LevelNode levelNodeA;
    public LevelNode LevelNodeA => levelNodeA;
    
    [SerializeField] private LevelNode levelNodeB;
    public LevelNode LevelNodeB => levelNodeB;
    
    [SerializeField] private float m_Cost;
    public float Cost => m_Cost;
    
    [Tooltip("The spline container that holds the forward path between the two nodes")]
    [SerializeField] private SplineContainer m_SplineContainer;
    public SplineContainer SplineContainer => m_SplineContainer;
    
    [Tooltip("The spline container that holds the reverse path between the two nodes")]
    [SerializeField] private SplineContainer m_ReverseSplineContainer;
    public SplineContainer ReverseSplineContainer => m_ReverseSplineContainer;

    protected override Transform EndPoint => levelNodeA.transform;
    protected override Transform StartingPoint => levelNodeB.transform;

    public SplineContainer GetPathSplineTo(LevelNode destNode)
    {
        if (destNode == LevelNodeB)
            return GetPathSpline();
        if (destNode == LevelNodeA)
            return GetPathSpline(true);

        Debug.LogError("LevelNode: Destination node is not connected to this edge");
        return null;
    }
    
    public SplineContainer GetPathSpline(bool reverse = false)
    {
        if (!reverse)
        {
            if (m_SplineContainer == null || m_SplineContainer.Spline == null)
            {
                Debug.LogError("EdgeInternal: Spline container is null");
                return null;
            }
            return m_SplineContainer;
        }
        else
        {
            if (m_ReverseSplineContainer == null || m_ReverseSplineContainer.Spline == null)
            {
                Debug.LogError("EdgeInternal: Reverse spline container is null");
                return null;
            }
            return m_ReverseSplineContainer;
        }
    }
    
    
    
#if UNITY_EDITOR
    
    public void UpdateSpline()
    {
        if (m_SplineContainer == null)
            return;

        if (m_SplineContainer.Spline.Count == 0)
        {
            m_SplineContainer.Spline.Add(new BezierKnot() {});
            m_SplineContainer.Spline.Add(new BezierKnot() {});
        }
        else if (m_SplineContainer.Spline.Count == 1)
        {
            m_SplineContainer.Spline.Add(new BezierKnot() {});
        }
        
        m_SplineContainer.Spline[0] = new BezierKnot() {Position = Vector3.zero};
        
        if (LevelNodeA != null)
            transform.position = LevelNodeA.transform.position;
        
        if (LevelNodeB != null)
        {
            m_SplineContainer.Spline[^1] = new BezierKnot()
            {
                Position = m_SplineContainer.transform.InverseTransformPoint(LevelNodeB.transform.position)
            };
        }
        
        m_SplineContainer.Spline.SetTangentMode(TangentMode.AutoSmooth);
    }
    
    public void UpdateReverseSpline()
    {
        if (m_SplineContainer == null || m_SplineContainer.Spline == null || m_ReverseSplineContainer == null)
            return;

        if (m_ReverseSplineContainer.Spline == null)
            m_ReverseSplineContainer.AddSpline(new Spline(m_SplineContainer.Spline));
        else
            m_ReverseSplineContainer.Spline = new Spline(m_SplineContainer.Spline);
        
        m_ReverseSplineContainer.ReverseFlow(0);
        
        m_ReverseSplineContainer.Spline.SetTangentMode(TangentMode.AutoSmooth);
    }
    
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(EdgeInternal)), CanEditMultipleObjects]
public class EdgeInternalEditor : Editor
{
    EdgeInternal m_Target;
    EdgeInternal[] m_TargetEdgeInternals;

    private void OnEnable()
    {
        if (targets.Length == 1)
            m_Target = (EdgeInternal) target;
        else
        {
            m_TargetEdgeInternals = new EdgeInternal[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                m_TargetEdgeInternals[i] = (EdgeInternal)targets[i];
            }
        }
    }

    public override void OnInspectorGUI()
    {   
        base.OnInspectorGUI();

        if (GUILayout.Button("Update Spline"))
        {
            if (m_TargetEdgeInternals == null)
                UpdateTargetSpline(m_Target);
            else
            {
                foreach (var edgeInternal in m_TargetEdgeInternals)
                    UpdateTargetSpline(edgeInternal);
            }
        }
        
        if (GUILayout.Button("Update Reverse Spline"))
        {
            if (m_TargetEdgeInternals == null)
                UpdateTargetReverseSpline(m_Target);
            else
            {
                foreach (var edgeInternal in m_TargetEdgeInternals)
                    UpdateTargetReverseSpline(edgeInternal);
            }
        }
    }
    
    private void UpdateTargetSpline(EdgeInternal edgeInternal)
    {
        edgeInternal.UpdateSpline();
        
        Undo.RecordObject(edgeInternal.SplineContainer, "Updated Spline");
        
        PrefabUtility.RecordPrefabInstancePropertyModifications(edgeInternal.SplineContainer);
    }

    private void UpdateTargetReverseSpline(EdgeInternal edgeInternal)
    {
        edgeInternal.UpdateReverseSpline();

        Undo.RecordObject(edgeInternal.ReverseSplineContainer, "Updated Reverse Spline");

        PrefabUtility.RecordPrefabInstancePropertyModifications(edgeInternal.ReverseSplineContainer);
    }
}
#endif
