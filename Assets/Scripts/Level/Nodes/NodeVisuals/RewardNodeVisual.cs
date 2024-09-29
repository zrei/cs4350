using UnityEngine;

/// <summary>
/// Class for handling visuals for Reward Nodes
/// </summary>
[RequireComponent(typeof(RewardNode))]
public class RewardNodeVisual : NodeVisual
{
    private RewardNode m_RewardNode;
    
    public override void Initialise()
    {
        m_RewardNode = GetComponent<RewardNode>();
    }

    #region Graphics
    public override void UpdateNodeVisualState()
    {
        if (m_RewardNode.IsCurrent)
        {
            SetNodeState(NodePuckType.CURRENT);
        }
        else
        {
            SetNodeState(m_RewardNode.IsCleared ? NodePuckType.CLEARED : NodePuckType.REWARD);
        
            if (m_RewardNode.IsGoalNode)
            {
                m_NodeStateText.text = "Final Reward";
                m_NodeStateText.enabled = true;
            }
        }
    }
    #endregion
}
