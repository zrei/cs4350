using System.Collections;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;

// General Node Information applicable for all types of Nodes
[System.Serializable]
public struct NodeInfo
{
    public string m_NodeName;
    public string m_NodeDescription;
}

public class NodePreviewData
{
    public string NodeName;
    public string NodeDescription;
    public bool IsMoralityLocked;
    public Threshold MoralityThreshold;
}

/// <summary>
/// Base class that maintains the internal representation of a node in the graph
/// (node information and connected nodes),
/// </summary>
public abstract class NodeInternal : MonoBehaviour
{
    #region Static Information
    
    // Static Node Information
    [SerializeField] private NodeInfo m_NodeInfo;
    public NodeInfo NodeInfo => m_NodeInfo;

    [SerializeField] private bool m_IsMoralityLocked;
    public bool IsMoralityLocked => m_IsMoralityLocked;
    
    [SerializeField] private Threshold m_MoralityThreshold;
    public Threshold MoralityThreshold => m_MoralityThreshold;

    [Tooltip("Tutorial to play upon first visiting the node - leave empty for no tutorial")]
    [SerializeField] private TutorialSO m_PreTutorial;
    [Tooltip("Tutorial to play upon finishing the node for the first time - leave empty for no tutorial")]
    [SerializeField] protected TutorialSO m_PostTutorial;

    protected bool m_HasPlayedPreTutorial = false;
    protected bool m_HasPlayedPostTutorial = false;
    
    // Adjacent nodes and their costs
    private Dictionary<NodeInternal, float> m_AdjacentNodes = new();

    public virtual NodePreviewData GetNodePreviewData()
    {
        return new NodePreviewData
        {
            NodeName = m_NodeInfo.m_NodeName,
            NodeDescription = m_NodeInfo.m_NodeDescription,
            IsMoralityLocked = m_IsMoralityLocked,
            MoralityThreshold = m_MoralityThreshold
        };
    }
    #endregion

    #region Node State Information

    // Whether is goal node
    private bool m_IsGoalNode = false;
    public bool IsGoalNode => m_IsGoalNode;
    
    // Whether the node has been cleared
    private bool m_IsCleared = false;
    public bool IsCleared => m_IsCleared;

    // Whether the node is the current node
    private bool m_IsCurrent = false;
    public bool IsCurrent => m_IsCurrent;

    #endregion
    
    #region Initialisation

    public virtual void Initialise() {}
    
    #endregion

    #region NodeState
    
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

    #region Callbacks

    public virtual void EnterNode()
    {
        Debug.Log("Entered Node: " + m_NodeInfo.m_NodeName);
        m_IsCurrent = true;
        GlobalEvents.Level.NodeEnteredEvent(this);
    }

    public void StartNodeEvent(VoidEvent postEvent = null)
    {
        Debug.Log("Starting Node: " + m_NodeInfo.m_NodeName);
        if (!m_HasPlayedPreTutorial)
        {
            m_HasPlayedPreTutorial = true;
            PlayTutorial(m_PreTutorial, () => PerformNode(postEvent));
        }
        else
        {
            PerformNode(postEvent);
        }
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

    protected abstract void PerformNode(VoidEvent postEvent = null);

    #endregion

    #region Tutorial
    protected void PlayTutorial(TutorialSO tutorial, VoidEvent postEvent)
    {
        if (tutorial == null)
        {
            postEvent?.Invoke();
        } 
        else
        {
            IUIScreen tutorialScreen = UIScreenManager.Instance.TutorialScreen;
            tutorialScreen.OnHideDone += PostTutorial;
            UIScreenManager.Instance.OpenScreen(tutorialScreen, false, tutorial);
        }

        void PostTutorial(IUIScreen screen)
        {
            screen.OnHideDone -= PostTutorial;
            postEvent?.Invoke();
        }
        
    }
    #endregion
}
