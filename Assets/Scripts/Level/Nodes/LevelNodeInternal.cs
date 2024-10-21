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
/// Base class that maintains the internal representation of a node in the graph
/// (node information and connected nodes),
/// </summary>
public abstract class LevelNodeInternal : MonoBehaviour
{
    // Static Node Information
    [SerializeField] private NodeInfo m_NodeInfo;
    
    // Whether is goal node
    private bool m_IsGoalNode = false;
    
    // Whether the node has been cleared
    private bool m_IsCleared = false;

    // Whether the node is the current node
    private bool m_IsCurrent = false;
    
    // Adjacent nodes and their costs
    private Dictionary<LevelNodeInternal, float> m_AdjacentNodes = new();
    
    #region Initialisation

    public virtual void Initialise() {}
    
    #endregion

    #region StaticInformation
    
    public NodeInfo NodeInfo => m_NodeInfo;

    #endregion

    #region NodeState
    public bool IsGoalNode => m_IsGoalNode;
    public bool IsCleared => m_IsCleared;
    public bool IsCurrent => m_IsCurrent;
    
    public void SetGoalNode()
    {
        m_IsGoalNode = true;
    }
    
    public void SetCleared()
    {
        m_IsCleared = true;
    }
    
    public void SetCurrent(bool isCurrent)
    {
        m_IsCurrent = isCurrent;
    }

    #endregion

    #region Graph
    public Dictionary<LevelNodeInternal, float> AdjacentNodes => m_AdjacentNodes;

    public void AddAdjacentNode(LevelNodeInternal nodeInternal, float cost)
    {
        m_AdjacentNodes.TryAdd(nodeInternal, cost);
    }
    
    public void RemoveAdjacentNode(LevelNodeInternal nodeInternal)
    {
        m_AdjacentNodes.Remove(nodeInternal);
    }

    #endregion

    #region Callbacks

    public virtual void EnterNode()
    {
        Debug.Log("Entered Node: " + m_NodeInfo.m_NodeName);
        m_IsCurrent = true;
        GlobalEvents.Level.NodeEnteredEvent(this);
    }

    public virtual void StartNodeEvent()
    {
        Debug.Log("Starting Node: " + m_NodeInfo.m_NodeName);
    }
    
    public virtual void ClearNode()
    {
        Debug.Log("Cleared Node: " + m_NodeInfo.m_NodeName);
        SetCleared();
        GlobalEvents.Level.NodeClearedEvent(this);
    }
    
    public virtual void ExitNode()
    {
        Debug.Log("Exited Node: " + m_NodeInfo.m_NodeName);
        m_IsCurrent = false;
        GlobalEvents.Level.NodeExitedEvent(this);
    }

    #endregion
    
}
