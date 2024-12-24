using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for handling visuals for Dialogue Nodes
/// </summary>
[RequireComponent(typeof(DialogueNode))]
public class DialogueNodeVisual : LevelNodeVisual
{
    private DialogueNode m_DialogueNode;
    
    [SerializeField] private GameObject m_dialogueToken;
    
    public override void Initialise()
    {
        m_DialogueNode = GetComponent<DialogueNode>();
        
        if (m_DialogueNode.IsGoalNode)
            ToggleStarOn();
        
        if (m_DialogueNode.IsMoralityLocked)
            SetMoralityThresholdText(m_DialogueNode.MoralityThreshold);
    }

    #region Graphics
    public override void UpdateNodeVisualState()
    {
        if (m_DialogueNode.IsCurrent)
        {
            SetNodeColor(NodePuckType.CURRENT);
            m_dialogueToken.SetActive(false);
        }
        else
        {
            SetNodeColor(m_DialogueNode.IsCleared ? NodePuckType.CLEARED : NodePuckType.REWARD);
            m_dialogueToken.SetActive(!m_DialogueNode.IsCleared);
        }
    }

    #endregion

    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered Dialogue Node");
        // GlobalEvents.Level.NodeHoverStartEvent?.Invoke(m_DialogueNode);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited Dialogue Node");
        GlobalEvents.Level.NodeHoverEndEvent?.Invoke();
    }
}
