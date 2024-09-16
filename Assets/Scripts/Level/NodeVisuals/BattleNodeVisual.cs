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
    }

    #region Graphics
    public override void UpdateNodeVisualState()
    {
        if (m_BattleNode.IsCurrent)
        {
            SetNodeState("Current");
        }
        else
        {
            SetNodeState(m_BattleNode.IsCleared ? "Cleared" : "Battle");
        }
    }
    #endregion
}
