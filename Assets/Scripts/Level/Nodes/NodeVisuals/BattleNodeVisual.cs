using TMPro;
using UnityEngine;

/// <summary>
/// Class for handling visuals for Battle Nodes
/// </summary>
[RequireComponent(typeof(BattleNode))]
public class BattleNodeVisual : NodeVisual
{
    private BattleNode m_BattleNode;
    
    
    public override void Initialise()
    {
        m_BattleNode = GetComponent<BattleNode>();
        
        if (m_BattleNode.IsGoalNode)
            ToggleStarOn();
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
