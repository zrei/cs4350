using TMPro;
using UnityEngine;

/// <summary>
/// Class for handling visuals for Battle Nodes
/// </summary>
[RequireComponent(typeof(BattleNode))]
public class BattleNodeVisual : NodeVisual
{
    private BattleNode m_BattleNode;
    
    // Star token model for goal nodes
    [SerializeField] private GameObject m_starToken;
    
    public override void Initialise()
    {
        m_BattleNode = GetComponent<BattleNode>();
        
        if (m_BattleNode.IsGoalNode)
        {
            m_starToken.SetActive(true);
        }
    }

    #region Graphics
    public override void UpdateNodeVisualState()
    {
        if (m_BattleNode.IsCurrent)
        {
            SetNodeState(NodePuckType.CURRENT);
        }
        else
        {
            SetNodeState(m_BattleNode.IsCleared ? NodePuckType.CLEARED : NodePuckType.BATTLE);
        }
        
    }
    #endregion
}
