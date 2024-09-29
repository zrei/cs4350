using UnityEngine;

/// <summary>
/// Class for handling visuals for Reward Nodes
/// </summary>
[RequireComponent(typeof(RewardNode))]
public class RewardNodeVisual : NodeVisual
{
    private RewardNode m_RewardNode;
    
    // Chest token model
    [SerializeField] private GameObject m_chestToken;
    
    // Star token model for goal nodes
    [SerializeField] private GameObject m_starToken;
    
    public override void Initialise()
    {
        m_RewardNode = GetComponent<RewardNode>();
        
        if (m_RewardNode.IsGoalNode)
        {
            m_starToken.SetActive(true);
        }
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
        
            if (m_RewardNode.IsGoalNode)
            {
                m_NodeStateText.text = "Final Reward";
                m_NodeStateText.enabled = true;
            }
        }
    }
    #endregion
}
