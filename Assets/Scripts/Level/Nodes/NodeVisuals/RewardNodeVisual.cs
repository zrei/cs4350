using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for handling visuals for Reward Nodes
/// </summary>
[RequireComponent(typeof(RewardNode))]
public class RewardNodeVisual : NodeVisual
{
    private RewardNode m_RewardNode;
    
    // Chest token model
    [SerializeField] private GameObject m_chestToken;
    
    public override void Initialise()
    {
        m_RewardNode = GetComponent<RewardNode>();
        
        if (m_RewardNode.IsGoalNode)
            ToggleStarOn();
    }
    
    

    #region Graphics
    public override void UpdateNodeVisualState()
    {
        if (m_RewardNode.IsCurrent)
        {
            SetNodeState(NodePuckType.CURRENT);
            m_chestToken.SetActive(false);
        }
        else
        {
            SetNodeState(m_RewardNode.IsCleared ? NodePuckType.CLEARED : NodePuckType.REWARD);
            m_chestToken.SetActive(!m_RewardNode.IsCleared);
        }
    }

    #endregion

    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered Reward Node");
        GlobalEvents.Level.NodeHoverStartEvent?.Invoke(m_RewardNode);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited Reward Node");
        GlobalEvents.Level.NodeHoverEndEvent?.Invoke(m_RewardNode);
    }
}
