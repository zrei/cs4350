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

    public void Initialise(List<NodeInternal> levelNodes, List<EdgeInternal> levelEdges, float timeLimit)
    {
        // Initialise the internal graph representation of the level
        InitialiseMap(levelNodes, levelEdges);
        
        // Initialise the time limit
        m_TimeRemaining = timeLimit;
    }
    
    /// <summary>
    /// Initialise the level map with the nodes and edges in scene
    /// </summary>
    public void InitialiseMap(List<NodeInternal> levelNodes, List<EdgeInternal> levelEdges)
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
    public NodeInternal CurrentNode => m_CurrentNodeInternal;
    
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
            // if (!m_CurrentNodeInternal.AdjacentNodes.ContainsKey(nodeInternal))
            // {
            //     Debug.Log("Node is not reachable");
            //     return;
            // } 
            
            if (m_CurrentNodeInternal == nodeInternal)
            {
                Debug.Log("Already at the node");
                return;
            }
            
            m_CurrentNodeInternal.OnClearNode();
            m_CurrentNodeInternal.OnExitNode();
            m_CurrentNodeInternal.SetCurrent(false);
            
            // Retrieve cost to move to the node
            // float cost = m_CurrentNodeInternal.AdjacentNodes[nodeInternal];
            // m_TimeRemaining -= cost;
        }
        
        m_CurrentNodeInternal = nodeInternal;
        m_CurrentNodeInternal.OnEnterNode();
        m_CurrentNodeInternal.SetCurrent(true);
        
        Debug.Log($"Time remaining: {m_TimeRemaining}");
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
            node = raycastHit.collider.gameObject.GetComponent<NodeInternal>();

            if (node)
                return true;
        }
        node = default;
        
        return false;
    }
    

    #endregion
}
