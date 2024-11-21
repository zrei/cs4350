using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class WorldMapEdge : BaseEdge
{
    [Header("World Map Edge")]
    [SerializeField] private Transform m_StartingPoint;
    [SerializeField] private Transform m_EndPoint;
    [SerializeField] private SplineContainer m_SplineContainer;
    [SerializeField] private WorldMapPathNode m_NodeObj;

    public SplineContainer Spline => m_SplineContainer;

    private const float NODE_INTERVALS = 2f;
    private const float NODE_SPAWN_DELAY = 0.3f;

    protected override Transform EndPoint => m_EndPoint;
    protected override Transform StartingPoint => m_StartingPoint;

    #region Path
    public void InstantiatePath(float offset, bool instant = true, VoidEvent onCompleteInstantiation = null)
    {
        float totalDistance = m_SplineContainer.CalculateLength();
        float endingDistance = totalDistance - offset;
        if (instant)
        {
            InstantiateAll(offset, endingDistance, totalDistance);
            onCompleteInstantiation?.Invoke();
        }
        else
            StartCoroutine(SpawnPathCoroutine(offset, endingDistance, totalDistance, onCompleteInstantiation));
    }

    private void InstantiateAll(float startingDistance, float endingDistance, float totalDistance)
    {
        while (startingDistance < endingDistance)
        {
            InstantiatePathNode(GetPathNodePosition(startingDistance / totalDistance));
            startingDistance += NODE_INTERVALS;
        }
    }

    private IEnumerator SpawnPathCoroutine(float pathStartLength, float pathEndLength, float totalPathLength, VoidEvent onCompleteInstantiation)
    {
        while (pathStartLength < pathEndLength)
        {
            yield return new WaitForSeconds(NODE_SPAWN_DELAY);
            InstantiatePathNode(GetPathNodePosition(pathStartLength / totalPathLength), false, NODE_SPAWN_DELAY);
            pathStartLength += NODE_INTERVALS;
        }
        onCompleteInstantiation?.Invoke();
    }
    #endregion

    #region Helper
    private Vector3 GetPathNodePosition(float proportionOfDistance)
    {
        return m_SplineContainer.EvaluatePosition(proportionOfDistance);
    }

    private void InstantiatePathNode(Vector3 position, bool instant = true, float appearTime = 0.1f)
    {
        WorldMapPathNode node = Instantiate(m_NodeObj, position, Quaternion.identity, this.transform);
        
        if (instant)
            node.SetToMaxSize();
        else
            node.Expand(appearTime);
    }

    public Vector3 GetInitialSplineForwardDirection()
    {
        return ((Vector3) (Spline[0][1].Position - Spline[0][0].Position)).normalized;
    }
    #endregion
    

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
        
        if (m_StartingPoint != null)
            this.transform.position = m_StartingPoint.position;
        
        if (m_EndPoint != null)
        {
            m_SplineContainer.Spline[m_SplineContainer.Spline.Count - 1] = new BezierKnot() {Position = m_SplineContainer.transform.InverseTransformPoint(m_EndPoint.position)};
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldMapEdge)), CanEditMultipleObjects]
public class WorldMapEdgeEditor : Editor
{
    WorldMapEdge m_Target;
    WorldMapEdge[] m_Targets;

    private void OnEnable()
    {
        if (targets.Length == 1)
            m_Target = (WorldMapEdge) target;
        else
        {
            m_Targets = new WorldMapEdge[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                m_Targets[i] = (WorldMapEdge)targets[i];
            }
        }
    }

    public override void OnInspectorGUI()
    {   
        base.OnInspectorGUI();

        if (GUILayout.Button("Update spline"))
        {
            if (m_Targets == null)
                m_Target.UpdateSpline();
            else
            {
                foreach (var worldMapEdge in m_Targets)
                    worldMapEdge.UpdateSpline();
            }
        }
    }
}
#endif
