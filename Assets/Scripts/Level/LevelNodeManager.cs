using System.Collections.Generic;
using Level.Nodes;
using UnityEngine;

/// <summary>
/// Manages the internal representation of the level graph
/// </summary>
public class LevelNodeManager : MonoBehaviour
{
    // Graph Information
    private List<LevelNode> m_LevelNodes = new();
    public List<LevelNode> LevelNodes => m_LevelNodes;
    
    private List<EdgeInternal> m_LevelEdges = new();
    
    private LevelNode m_GoalNode;
    
    // Current State
    private LevelNode m_CurrentLevelNode;

    #region Initialisation

    public void Initialise(List<LevelNode> levelNodes, List<EdgeInternal> levelEdges)
    {
        // Initialise the internal graph representation of the level
        InitialiseMap(levelNodes, levelEdges);
    }
    
    /// <summary>
    /// Initialise the level map with the nodes and edges in scene
    /// </summary>
    public void InitialiseMap(List<LevelNode> levelNodes, List<EdgeInternal> levelEdges)
    {
        // Retrieve all nodes in the level
        foreach (var levelNode in levelNodes)
        {
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
            levelEdge.LevelNodeA.AddAdjacentNode(levelEdge.LevelNodeB, levelEdge.Cost);
            levelEdge.LevelNodeB.AddAdjacentNode(levelEdge.LevelNodeA, levelEdge.Cost);
            
            m_LevelEdges.Add(levelEdge);
        }
    }

    #endregion

    #region Graph
    public LevelNode CurrentNode => m_CurrentLevelNode;
    
    public void SetStartNode(LevelNode startNode)
    {
        m_CurrentLevelNode = startNode;
        m_CurrentLevelNode.EnterNode();
    }

    public void MoveToNode(LevelNode destNode, out float cost)
    {
        m_CurrentLevelNode.ExitNode();
            
        // Retrieve cost to move to the node
        cost = m_CurrentLevelNode.AdjacentNodes[destNode];
        
        m_CurrentLevelNode = destNode;
        m_CurrentLevelNode.EnterNode();
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
    public bool TryRetrieveNode(Ray ray, out LevelNode node)
    {
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask.GetMask("LevelMap"));
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.white, 100f, false); 
        foreach (RaycastHit raycastHit in raycastHits)
        {
            node = raycastHit.collider.gameObject.GetComponentInParent<LevelNode>();

            if (node)
                return true;
        }
        node = default;
        
        return false;
    }
    
   public EdgeInternal GetEdgeToNode(LevelNode destNode)
    {
        foreach (var edge in m_LevelEdges)
        {
            if (edge.LevelNodeA == m_CurrentLevelNode && edge.LevelNodeB == destNode ||
                edge.LevelNodeB == m_CurrentLevelNode && edge.LevelNodeA == destNode)
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
    public bool CanMoveToNode(LevelNode destNode)
    {
        if (destNode.IsMoralityLocked && !destNode.MoralityThreshold.IsSatisfied(MoralityManager.Instance.CurrMorality))
        {
            return false;
        }
        
        return m_CurrentLevelNode.AdjacentNodes.ContainsKey(destNode);
    }
    
    public List<LevelNode> GetCurrentMovableNodes()
    {
        List<LevelNode> movableNodes = new();
        
        // If the node is not cleared, only the current node should be movable
        if (m_CurrentLevelNode.IsCleared == false)
        {
            movableNodes.Add(m_CurrentLevelNode);
            return movableNodes;
        }
        
        foreach (var node in m_CurrentLevelNode.AdjacentNodes.Keys)
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
        return m_CurrentLevelNode.IsCleared;
    }
    
    public void ClearCurrentNode()
    {
        m_CurrentLevelNode.ClearNode();
    }

    public void SetGoalNode(LevelNode goalNode)
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
