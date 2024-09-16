using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// General Node Information applicable for all types of Nodes
[System.Serializable]
public struct NodeInfo
{
    public string m_NodeName;
    public string m_NodeDescription;
}

/// <summary>
/// Base class that maintains the internal representation of the nodes in the graph
/// (node information and connected nodes),
/// </summary>
public abstract class NodeInternal : MonoBehaviour
{
    // Static Node Information
    [SerializeField] private NodeInfo m_NodeInfo;
    
    // Whether the node has been cleared
    private bool m_IsCleared = false;

    // Whether the node is the current node
    private bool m_IsCurrent = false;
    
    // Adjacent nodes and their costs
    private Dictionary<NodeInternal, float> m_AdjacentNodes = new();
    
    #region Initialisation

    public virtual void Initialise() {}
    
    #endregion

    #region StaticInformation
    
    public NodeInfo NodeInfo => m_NodeInfo;

    #endregion

    #region NodeState
    public bool IsCleared => m_IsCleared;
    public bool IsCurrent => m_IsCurrent;
    
    public void ClearNode()
    {
        m_IsCleared = true;
    }
    
    public void SetCurrent(bool isCurrent)
    {
        m_IsCurrent = isCurrent;
    }

    #endregion

    #region Graph
    public Dictionary<NodeInternal, float> AdjacentNodes => m_AdjacentNodes;

    public void AddAdjacentNode(NodeInternal nodeInternal, float cost)
    {
        m_AdjacentNodes.TryAdd(nodeInternal, cost);
    }
    
    public void RemoveAdjacentNode(NodeInternal nodeInternal)
    {
        m_AdjacentNodes.Remove(nodeInternal);
    }

    #endregion

    #region Events
    
    public VoidEvent OnEnterNodeEvent;
    public VoidEvent OnClearNodeEvent;
    public VoidEvent OnExitNodeEvent;

    #endregion

    #region Callbacks

    public virtual void OnEnterNode()
    {
        Debug.Log("Entered Node: " + m_NodeInfo.m_NodeName);
    }
    
    public virtual void OnClearNode()
    {
        ClearNode();
        Debug.Log("Cleared Node: " + m_NodeInfo.m_NodeName);
    }
    
    public virtual void OnExitNode()
    {
        Debug.Log("Exited Node: " + m_NodeInfo.m_NodeName);
    }

    #endregion
    
}
