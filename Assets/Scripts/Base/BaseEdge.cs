using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public abstract class BaseEdge : MonoBehaviour
{
    [Header("Gizmos")]
    [SerializeField] private float m_SphereRadius = 0.2f;
    [SerializeField] private Vector3 m_Offset = new Vector3(0, 0.5f, 0);

    protected virtual Transform StartingPoint => null;
    protected virtual Transform EndPoint => null;

    private void OnDrawGizmosSelected()
    {
        if (StartingPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(StartingPoint.position + m_Offset, m_SphereRadius);
        }
        
        if (EndPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(EndPoint.position + m_Offset, m_SphereRadius);
        }
            
        if (StartingPoint != null && EndPoint != null)
        {
            Gizmos.color = Color.blue;
            Vector3[] positions = new Vector3[2];
            positions[0] = StartingPoint.position + m_Offset;
            positions[1] = EndPoint.position + m_Offset;
            Gizmos.DrawLineList(positions);
        }
    }
}