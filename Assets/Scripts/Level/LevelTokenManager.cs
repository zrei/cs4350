using System.Collections.Generic;
using Level;
using UnityEngine;

/// <summary>
/// Manages the tokens of the player and enemy units in the level
/// </summary>
public class LevelTokenManager : MonoBehaviour
{
    // Character Token Prefab
    [SerializeField] CharacterToken m_CharacterToken;
    
    // Animation time to move to a node
    private const float MOVE_TO_NODE_TIME = 1f;
    
    private CharacterToken m_PlayerUnitToken;
    private NodeVisual m_CurrentNodeVisual;

    #region Initialisation

    public void Initialise(PlayerCharacterBattleData characterBattleData, NodeVisual currNodeVisual)
    {
        m_PlayerUnitToken = Instantiate(m_CharacterToken);
        m_PlayerUnitToken.Initialise(characterBattleData);
        m_PlayerUnitToken.transform.position = currNodeVisual.GetPlayerTargetPosition();
        
        m_CurrentNodeVisual = currNodeVisual;
    }
    
    private void OnBattleNodeEnd(BattleNode battleNode, UnitAllegiance victor, int numTurns)
    {
        if (victor == UnitAllegiance.PLAYER)
        {
            NodeVisual battleNodeVisual = battleNode.GetComponent<NodeVisual>();
            if (battleNodeVisual && battleNodeVisual.HasClearAnimation())
            {
                battleNodeVisual.PlayClearAnimation(m_PlayerUnitToken, null);
            }
        }
    }

    #endregion

    #region Node Movement

    /// <summary>
    /// Get the position right before entering the destination node 
    /// </summary>
    /// <param name="origin">Origin node position</param>
    /// <param name="dest">Destination node position</param>
    /// <returns></returns>
    private Vector3 GetNodeEdgePos(Vector3 origin, Vector3 dest)
    {
        var direction = (dest - origin).normalized;
        return dest - direction * NodeVisual.NODE_RADIUS_OFFSET;
    }

    #endregion
    
    #region Helper
    
    public Transform GetPlayerTokenTransform()
    {
        return m_PlayerUnitToken.transform;
    }
    
    /// <summary>
    /// Move the player token to the destination node.
    /// If the destination node has an entry animation, move to the edge of the node and
    /// play the node's entry animation.
    /// </summary>
    /// <param name="destNodeVisual"></param>
    /// <param name="onMoveComplete"></param>
    public void MovePlayerToNode(NodeVisual destNodeVisual, VoidEvent onMoveComplete)
    {
        if (destNodeVisual.HasEntryAnimation())
        {
            Vector3 destPos = GetNodeEdgePos(m_CurrentNodeVisual.transform.position, destNodeVisual.GetPlayerTargetPosition());
            m_PlayerUnitToken.MoveToPosition(destPos, OnMoveToEdgeComplete, MOVE_TO_NODE_TIME);
            
            void OnMoveToEdgeComplete()
            {
                destNodeVisual.PlayEntryAnimation(m_PlayerUnitToken, onMoveComplete);
            }
        }
        else
        {
            Vector3 destPos = destNodeVisual.GetPlayerTargetPosition();
            m_PlayerUnitToken.MoveToPosition(destPos, onMoveComplete, MOVE_TO_NODE_TIME);
        }
        
        m_CurrentNodeVisual = destNodeVisual;
    }
    
    public void PlayClearAnimation(NodeVisual nodeVisual, VoidEvent onComplete)
    {
        if (nodeVisual.HasClearAnimation())
        {
            nodeVisual.PlayClearAnimation(m_PlayerUnitToken, onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    public void PlayFailureAnimation(NodeVisual nodeVisual, VoidEvent onComplete)
    {
        if (nodeVisual.HasFailureAnimation())
        {
            nodeVisual.PlayFailureAnimation(m_PlayerUnitToken, onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    #endregion

}
