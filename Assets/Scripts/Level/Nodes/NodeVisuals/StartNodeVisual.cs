using UnityEngine;

/// <summary>
/// Class for handling visuals for Start Nodes
/// </summary>
[RequireComponent(typeof(StartNode))]
public class StartNodeVisual : NodeVisual
{
    private StartNode m_StartNode;
    
    public override void Initialise()
    {
        m_StartNode = GetComponent<StartNode>();
    }

    #region Graphics
    public override void UpdateNodeVisualState()
    {
        SetNodeState(m_StartNode.IsCurrent ? "Current" : "Start");
    }
    #endregion
}
