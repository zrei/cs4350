using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the internal representation of the level graph
/// </summary>
public class LevelNodeManager : MonoBehaviour
{
    // Graph Information
    private List<LevelNodeInternal> m_LevelNodes = new();
    public List<LevelNodeInternal> LevelNodes => m_LevelNodes;
    
    private LevelNodeInternal m_GoalNode;
    
    // Current State
    private LevelNodeInternal m_CurrentNodeInternal;

    #region Initialisation

    public void Initialise(List<LevelNodeInternal> levelNodes, List<EdgeInternal> levelEdges, float timeLimit)
    {
        // Initialise the internal graph representation of the level
        InitialiseMap(levelNodes, levelEdges);
    }
    
    /// <summary>
    /// Initialise the level map with the nodes and edges in scene
    /// </summary>
    public void InitialiseMap(List<LevelNodeInternal> levelNodes, List<EdgeInternal> levelEdges)
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
        }
    }

    #endregion

    #region Graph
    public LevelNodeInternal CurrentNode => m_CurrentNodeInternal;
    
    public void SetStartNode(LevelNodeInternal startNode)
    {
        m_CurrentNodeInternal = startNode;
        m_CurrentNodeInternal.EnterNode();
    }

    public void MoveToNode(LevelNodeInternal destNode, out float timeCost)
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
    public bool TryRetrieveNode(Ray ray, out LevelNodeInternal node)
    {
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask.GetMask("LevelMap"));
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.white, 100f, false); 
        foreach (RaycastHit raycastHit in raycastHits)
        {
            node = raycastHit.collider.gameObject.GetComponentInParent<LevelNodeInternal>();

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
    public bool CanMoveToNode(LevelNodeInternal destNode)
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
    
    public void StartCurrentNodeEvent()
    {
        m_CurrentNodeInternal.StartNodeEvent();
    }

    public void SetGoalNode(LevelNodeInternal goalNode)
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
