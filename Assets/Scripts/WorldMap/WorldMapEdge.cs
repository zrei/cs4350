using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Will be used for set-up only
/// </summary>
[ExecuteInEditMode]
public class WorldMapEdge : MonoBehaviour
{
    [SerializeField] private WorldMapNode m_StartingNode;
    [SerializeField] private WorldMapNode m_EndNode;
    [SerializeField] private SplineContainer m_SplineContainer;

    public SplineContainer Spline => m_SplineContainer;

#if UNITY_EDITOR
    private void Update()
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
        
        if (m_StartingNode != null)
            this.transform.position = m_StartingNode.transform.position;
        
        if (m_EndNode != null)
        {
            m_SplineContainer.Spline[m_SplineContainer.Spline.Count - 1] = new BezierKnot() {Position = m_SplineContainer.transform.InverseTransformPoint(m_EndNode.transform.position)};
        }
    }
#endif
}

