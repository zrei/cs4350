using System.Collections.Generic;
using UnityEngine;

public class LevelNodeManager : MonoBehaviour
{
    // Graph Information
    private List<NodeInternal> m_LevelNodes = new();
    public List<NodeInternal> LevelNodes => m_LevelNodes;
    
    // Current State
    private float m_TimeRemaining;
    private NodeInternal m_CurrentNodeInternal;

    #region Initialisation

    public void Initialise(List<NodeInternal> levelNodes, List<LevelEdge> levelEdges, float timeLimit)
    {
        // Initialise the internal graph representation of the level
        InitialiseMap(levelNodes, levelEdges);
        
        // Initialise the time limit
        m_TimeRemaining = timeLimit;
    }
    
    /// <summary>
    /// Initialise the level map with the nodes and edges in scene
    /// </summary>
    public void InitialiseMap(List<NodeInternal> levelNodes, List<LevelEdge> levelEdges)
    {
        // Retrieve all nodes in the level
        foreach (var levelNode in levelNodes)
        {
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
    
    public void SetStartNode(NodeInternal startNode)
    {
        m_CurrentNodeInternal = startNode;
        m_CurrentNodeInternal.SetCurrent(true);
        m_CurrentNodeInternal.OnEnterNode();
    }

    public void MoveToNode(NodeInternal nodeInternal)
    {
        if (m_CurrentNodeInternal)
        {
            if (!m_CurrentNodeInternal.AdjacentNodes.ContainsKey(nodeInternal))
            {
                Debug.Log("Node is not reachable");
                return;
            } 
            
            if (m_CurrentNodeInternal == nodeInternal)
            {
                Debug.Log("Already at the node");
                return;
            }
            
            m_CurrentNodeInternal.OnClearNode();
            m_CurrentNodeInternal.OnExitNode();
            
            // Retrieve cost to move to the node
            float cost = m_CurrentNodeInternal.AdjacentNodes[nodeInternal];
            m_TimeRemaining -= cost;
        }
        
        m_CurrentNodeInternal = nodeInternal;
        m_CurrentNodeInternal.OnEnterNode();
        
        Debug.Log($"Time remaining: {m_TimeRemaining}");
    }

    #endregion
}
