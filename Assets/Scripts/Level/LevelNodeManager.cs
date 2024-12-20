using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the internal representation of the level graph
/// </summary>
public class LevelNodeManager : MonoBehaviour
{
    // Graph Information
    private List<NodeInternal> m_LevelNodes = new();
    public List<NodeInternal> LevelNodes => m_LevelNodes;
    
    private List<EdgeInternal> m_LevelEdges = new();
    
    private NodeInternal m_GoalNode;
    
    // Current State
    private NodeInternal m_CurrentNodeInternal;

    #region Initialisation

    public void Initialise(List<NodeInternal> levelNodes, List<EdgeInternal> levelEdges)
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
            
            // Check if goal node
            if (levelNode.IsGoalNode)
            {
                m_GoalNode = levelNode;
            }
        }
        
        // Building the graph from the scene objects
        foreach (var levelEdge in levelEdges)
        {
            levelEdge.NodeInternalA.AddAdjacentNode(levelEdge.NodeInternalB, levelEdge.Cost);
            levelEdge.NodeInternalB.AddAdjacentNode(levelEdge.NodeInternalA, levelEdge.Cost);
            
            m_LevelEdges.Add(levelEdge);
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

    public void MoveToNode(NodeInternal destNode, out float cost)
    {
        m_CurrentNodeInternal.ClearNode();
        m_CurrentNodeInternal.ExitNode();
            
        // Retrieve cost to move to the node
        cost = m_CurrentNodeInternal.AdjacentNodes[destNode];
        
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
    
   public EdgeInternal GetEdgeToNode(NodeInternal destNode)
    {
        foreach (var edge in m_LevelEdges)
        {
            if (edge.NodeInternalA == m_CurrentNodeInternal && edge.NodeInternalB == destNode ||
                edge.NodeInternalB == m_CurrentNodeInternal && edge.NodeInternalA == destNode)
            {
                return edge;
            }
        }

        return null;
    }
    
    /// <summary>
    /// Check if the player can move to the given node
    /// </summary>
    /// <param name="destNode"></param>
    /// <returns></returns>
    public bool CanMoveToNode(NodeInternal destNode)
    {
        if (destNode.IsMoralityLocked && !destNode.MoralityThreshold.IsSatisfied(MoralityManager.Instance.CurrMorality))
        {
            return false;
        }
        
        return m_CurrentNodeInternal.AdjacentNodes.ContainsKey(destNode);
    }
    
    public List<NodeInternal> GetCurrentMovableNodes()
    {
        List<NodeInternal> movableNodes = new();
        
        // If the node is not cleared, only the current node should be movable
        if (m_CurrentNodeInternal.IsCleared == false)
        {
            movableNodes.Add(m_CurrentNodeInternal);
            return movableNodes;
        }
        
        foreach (var node in m_CurrentNodeInternal.AdjacentNodes.Keys)
        {
            if (CanMoveToNode(node))
            {
                movableNodes.Add(node);
            }
        }

        return movableNodes;
    }
    
    public bool IsCurrentNodeCleared()
    {
        return m_CurrentNodeInternal.IsCleared;
    }
    
    public void ClearCurrentNode()
    {
        m_CurrentNodeInternal.ClearNode();
    }
    
    public void StartCurrentNodeEvent()
    {
        m_CurrentNodeInternal.StartNodeEvent();
    }

    public void SetGoalNode(NodeInternal goalNode)
    {
        m_GoalNode = goalNode;
        m_GoalNode.SetGoalNode();
    }
    
    public bool IsGoalNodeCleared()
    {
        return m_GoalNode.IsCleared;
    }

    #endregion
}
