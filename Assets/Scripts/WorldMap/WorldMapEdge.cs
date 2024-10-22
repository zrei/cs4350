using UnityEngine;

public class WorldMapEdge : MonoBehaviour
{
    [SerializeField] WorldMapNode m_NodeA;
    [SerializeField] WorldMapNode m_NodeB;
    [SerializeField] LineRenderer m_LineRenderer;

    private Vector3[] GetMovementPoints()
    {
        Vector3[] positions = new Vector3[m_LineRenderer.positionCount];
        int numPositions = m_LineRenderer.GetPositions(positions);
        return positions;
    }
}
