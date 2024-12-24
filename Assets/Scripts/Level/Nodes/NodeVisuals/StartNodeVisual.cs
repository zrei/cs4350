using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for handling visuals for Start Nodes
/// </summary>
public class StartNodeVisual : LevelNodeVisual
{
    private StartNode m_StartNode;
    
    public override void Initialise()
    {
        m_StartNode = GetComponent<StartNode>();
    }

    #region Graphics
    public override void UpdateNodeVisualState()
    {
        SetNodeColor(m_StartNode.IsCurrent ? NodePuckType.CURRENT : NodePuckType.CLEARED);
    }
    #endregion
    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered Start Node");
        // GlobalEvents.Level.NodeHoverStartEvent?.Invoke(m_StartNode);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited Start Node");
        GlobalEvents.Level.NodeHoverEndEvent?.Invoke();
    }
}
