using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the visuals of the nodes in the level and their associated events
/// (like transitions between nodes and clearing nodes)
/// </summary>
public class LevelNodeVisualManager : MonoBehaviour
{
    // Graphics
    private Dictionary<NodeInternal, NodeVisual> m_NodeVisuals = new();

    #region Initialisation

    public void Initialise(List<NodeInternal> levelNodes, List<EdgeInternal> levelEdges)
    {
        // Initialise the node visuals
        InitialiseNodeVisuals(levelNodes, levelEdges);
        
        // Initialise the event listeners
        InitialiseEventListeners();
    }
    
    public void InitialiseNodeVisuals(List<NodeInternal> levelNodes, List<EdgeInternal> levelEdges)
    {
        foreach (var levelNode in levelNodes)
        {
            var nodeVisual = levelNode.GetComponent<NodeVisual>();
            m_NodeVisuals.Add(levelNode, nodeVisual);
            
            nodeVisual.Initialise();
            nodeVisual.UpdateNodeVisualState();
            nodeVisual.ToggleSelected(false);
            nodeVisual.ToggleMovable(false);
        }

        foreach (var edge in levelEdges)
        {
            var edgeVisual = edge.GetComponent<EdgeVisual>();
            edgeVisual.Initialise(edge);
        }
    }
    
    private void InitialiseEventListeners()
    {
        GlobalEvents.Level.NodeEnteredEvent += OnNodeEntered;
        GlobalEvents.Level.NodeClearedEvent += OnNodeCleared;
        GlobalEvents.Level.NodeExitedEvent += OnNodeExited;
        GlobalEvents.Level.NodeSelectedEvent += OnNodeSelected;
        GlobalEvents.Level.NodeDeselectedEvent += OnNodeDeselected;
        GlobalEvents.Level.NodeMovementEvent += OnNodeMovement;
    }

    #endregion
    
    #region Callbacks
    
    private void OnNodeEntered(NodeInternal nodeInternal)
    {
        m_NodeVisuals[nodeInternal].UpdateNodeVisualState();
    }
    
    private void OnNodeCleared(NodeInternal nodeInternal)
    {
        m_NodeVisuals[nodeInternal].UpdateNodeVisualState();
    }
    
    private void OnNodeExited(NodeInternal nodeInternal)
    {
        m_NodeVisuals[nodeInternal].UpdateNodeVisualState();
    }
    
    private void OnNodeSelected(NodeInternal nodeInternal)
    {
        m_NodeVisuals[nodeInternal].ToggleSelected(true);
    }
    
    private void OnNodeDeselected(NodeInternal nodeInternal)
    {
        m_NodeVisuals[nodeInternal].ToggleSelected(false);
    }
    
    private void OnNodeMovement(NodeInternal nodeInternal1, NodeInternal nodeInternal2)
    {
        // Insert transition animations here
        Debug.Log("Animation: Transitioning between nodes: " + nodeInternal1 + " -> " + nodeInternal2);
    }

    private void OnDestroy()
    {
        GlobalEvents.Level.NodeEnteredEvent -= OnNodeEntered;
        GlobalEvents.Level.NodeClearedEvent -= OnNodeCleared;
        GlobalEvents.Level.NodeExitedEvent -= OnNodeExited;
        GlobalEvents.Level.NodeSelectedEvent -= OnNodeSelected;
        GlobalEvents.Level.NodeDeselectedEvent -= OnNodeDeselected;
        GlobalEvents.Level.NodeMovementEvent -= OnNodeMovement;
    }

    #endregion
    
    #region Helper
    
    public NodeVisual GetNodeVisual(NodeInternal nodeInternal)
    {
        return m_NodeVisuals[nodeInternal];
    }
    
    public void DisplayMovableNodes(List<NodeInternal> movableNodes)
    {
        foreach (var node in movableNodes)
        {
            m_NodeVisuals[node].ToggleMovable(true);
        }
    }
    
    public void ClearMovableNodes()
    {
        foreach (var nodeVisual in m_NodeVisuals.Values)
        {
            nodeVisual.ToggleMovable(false);
        }
    }
    
    #endregion
}
