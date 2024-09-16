using System.Collections.Generic;
using UnityEngine;

public class LevelGraphicsManager : MonoBehaviour
{
    // Graphics
    private Dictionary<NodeInternal, NodeVisual> m_NodeVisuals = new();

    #region Initialisation

    public void Initialise(List<NodeInternal> levelNodes, List<LevelEdge> levelEdges)
    {
        // Initialise the node visuals
        InitialiseNodeVisuals(levelNodes, levelEdges);
        
        // Initialise the event listeners
        InitialiseEventListeners();
    }
    
    public void InitialiseNodeVisuals(List<NodeInternal> levelNodes, List<LevelEdge> levelEdges)
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
    }
    
    private void InitialiseEventListeners()
    {
        GlobalEvents.Level.NodeEnteredEvent += OnNodeEntered;
        GlobalEvents.Level.NodeExitedEvent += OnNodeExited;
        GlobalEvents.Level.NodeSelectedEvent += OnNodeSelected;
        GlobalEvents.Level.NodeDeselectedEvent += OnNodeDeselected;
        
    }

    #endregion
    
    #region Callbacks
    
    private void OnNodeEntered(NodeInternal nodeInternal)
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
    
    
    #endregion
}
