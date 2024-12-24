using System.Collections.Generic;
using Level.Nodes;
using UnityEngine;

/// <summary>
/// Manages the visuals of the nodes in the level and their associated events
/// (like transitions between nodes and clearing nodes)
/// </summary>
public class LevelNodeVisualManager : MonoBehaviour
{
    // Graphics
    private Dictionary<LevelNode, LevelNodeVisual> m_NodeVisuals = new();

    #region Initialisation

    public void Initialise(List<LevelNode> levelNodes, List<EdgeInternal> levelEdges)
    {
        // Initialise the node visuals
        InitialiseNodeVisuals(levelNodes, levelEdges);
        
        // Initialise the event listeners
        InitialiseEventListeners();
    }
    
    public void InitialiseNodeVisuals(List<LevelNode> levelNodes, List<EdgeInternal> levelEdges)
    {
        foreach (var levelNode in levelNodes)
        {
            var nodeVisual = levelNode.GetComponent<LevelNodeVisual>();
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
    }

    #endregion
    
    #region Callbacks
    
    private void OnNodeEntered(LevelNode levelNode)
    {
        m_NodeVisuals[levelNode].UpdateNodeVisualState();
    }
    
    private void OnNodeCleared(LevelNode levelNode)
    {
        m_NodeVisuals[levelNode].UpdateNodeVisualState();
    }
    
    private void OnNodeExited(LevelNode levelNode)
    {
        m_NodeVisuals[levelNode].UpdateNodeVisualState();
    }
    
    private void OnNodeSelected(LevelNode levelNode)
    {
        m_NodeVisuals[levelNode].ToggleSelected(true);
    }
    
    private void OnNodeDeselected(LevelNode levelNode)
    {
        m_NodeVisuals[levelNode].ToggleSelected(false);
    }
    
    private void OnNodeMovement(LevelNode levelNode1, LevelNode levelNode2)
    {
        // Insert transition animations here
        Debug.Log("Animation: Transitioning between nodes: " + levelNode1 + " -> " + levelNode2);
    }

    private void OnDestroy()
    {
        GlobalEvents.Level.NodeEnteredEvent -= OnNodeEntered;
        GlobalEvents.Level.NodeClearedEvent -= OnNodeCleared;
        GlobalEvents.Level.NodeExitedEvent -= OnNodeExited;
        GlobalEvents.Level.NodeSelectedEvent -= OnNodeSelected;
        GlobalEvents.Level.NodeDeselectedEvent -= OnNodeDeselected;
    }

    #endregion
    
    #region Helper
    
    public LevelNodeVisual GetNodeVisual(LevelNode levelNode)
    {
        return m_NodeVisuals[levelNode];
    }
    
    public void DisplayMovableNodes(List<LevelNode> movableNodes)
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
