using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// Manages the internal representation of the level graph
/// </summary>
public class LevelNodeManager : MonoBehaviour
{
    // Graph Information
    private List<NodeInternal> m_LevelNodes = new();
    public List<NodeInternal> LevelNodes => m_LevelNodes;
    
    // Current State
    private NodeInternal m_CurrentNodeInternal;

    #region Initialisation

    public void Initialise(List<NodeInternal> levelNodes, List<EdgeInternal> levelEdges, float timeLimit)
    {
        // Initialise the internal graph representation of the level
        InitialiseMap(levelNodes, levelEdges);
    }
    
    /// <summary>
    /// Initialise the level map with the nodes and edges in scene
    /// </summary>
    public void InitialiseMap(List<NodeInternal> levelNodes, List<EdgeInternal> levelEdges)
    {
        // Retrieve all nodes in the level
        foreach (var levelNode in levelNodes)
        {
            levelNode.Initialise();
            m_LevelNodes.Add(levelNode);
        }
        
        // Building the graph from the scene objects
        foreach (var levelEdge in levelEdges)
        {
            levelEdge.NodeInternalA.AddAdjacentNode(levelEdge.NodeInternalB, levelEdge.Cost);
            levelEdge.NodeInternalB.AddAdjacentNode(levelEdge.NodeInternalA, levelEdge.Cost);
        }
    }

    #endregion

    #region Graph
    public NodeInternal CurrentNode => m_CurrentNodeInternal;
    
    public void SetStartNode(NodeInternal startNode)
    {
        m_CurrentNodeInternal = startNode;
        m_CurrentNodeInternal.EnterNode();
    }

    public void MoveToNode(NodeInternal destNode, out float timeCost)
    {
        m_CurrentNodeInternal.ClearNode();
        m_CurrentNodeInternal.ExitNode();
            
        // Retrieve cost to move to the node
        timeCost = m_CurrentNodeInternal.AdjacentNodes[destNode];
        
        GlobalEvents.Level.NodeMovementEvent(m_CurrentNodeInternal, destNode);
        
        m_CurrentNodeInternal = destNode;
        m_CurrentNodeInternal.EnterNode();
    }

    #endregion

    #region Helper

    /// <summary>
    /// Given a ray, tries to retrieve a node that the ray connects with.
    /// Nodes must have a collider and be on the layer "LevelMap"
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool TryRetrieveNode(Ray ray, out NodeInternal node)
    {
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask.GetMask("LevelMap"));
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.white, 100f, false); 
        foreach (RaycastHit raycastHit in raycastHits)
        {
            node = raycastHit.collider.gameObject.GetComponentInParent<NodeInternal>();

            if (node)
                return true;
        }
        node = default;
        
        return false;
    }
    
    /// <summary>
    /// Check if the player can move to the given node
    /// </summary>
    /// <param name="destNode"></param>
    /// <returns></returns>
    public bool CanMoveToNode(NodeInternal destNode)
    {
        return m_CurrentNodeInternal.AdjacentNodes.ContainsKey(destNode);
    }
    
    public bool IsCurrentNodeCleared()
    {
        return m_CurrentNodeInternal.IsCleared;
    }
    
    public void ClearCurrentNode()
    {
        m_CurrentNodeInternal.ClearNode();
    }
    

    #endregion
}
