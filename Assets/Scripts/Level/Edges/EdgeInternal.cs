using UnityEngine;

/// <summary>
/// Class that maintains the internal representation of an edge in the graph
/// (edge cost and connecting nodes),
/// </summary>
public class EdgeInternal : MonoBehaviour
{
    [SerializeField] private LevelNodeInternal nodeInternalA;
    public LevelNodeInternal NodeInternalA => nodeInternalA;
    
    [SerializeField] private LevelNodeInternal nodeInternalB;
    public LevelNodeInternal NodeInternalB => nodeInternalB;
    
    [SerializeField] private float m_Cost;
    public float Cost => m_Cost;
}
