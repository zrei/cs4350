using System;
using System.Collections.Generic;
using Level;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Manages the tokens of the player and enemy units in the level
/// </summary>
public class LevelTokenManager : MonoBehaviour
{
    // Character Token Prefab
    [SerializeField] PlayerToken m_PlayerToken;
    
    // Animation time to move to a node
    private const float MOVE_TO_NODE_TIME = 0.8f;
    private const float NODE_EXIT_TIME = 0.15f;
    private const float NODE_ENTRY_TIME = 0.15f;
    private const float MOVE_SPEED = 3.0f;
    
    private PlayerToken m_PlayerUnitToken;
    private LevelNodeVisual m_CurrentNodeVisual;

    #region Initialisation

    public void Initialise(PlayerCharacterBattleData characterBattleData, LevelNodeVisual currNodeVisual)
    {
        m_PlayerUnitToken = Instantiate(m_PlayerToken);
        m_PlayerUnitToken.Initialise(characterBattleData);
        m_PlayerUnitToken.transform.position = currNodeVisual.GetPlayerTargetPosition();
        
        m_CurrentNodeVisual = currNodeVisual;
    }

    public void UpdateAppearance(PlayerCharacterBattleData characterBattleData)
    {
        m_PlayerUnitToken.UpdateAppearance(characterBattleData);
    }
    
    private void OnBattleNodeEnd(BattleNode battleNode, UnitAllegiance victor, int numTurns)
    {
        if (victor == UnitAllegiance.PLAYER)
        {
            LevelNodeVisual battleNodeVisual = battleNode.GetComponent<LevelNodeVisual>();
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
    private Vector3 GetNodeEdgePos(LevelNodeVisual startNode, LevelNodeVisual destNode)
    {
        Vector3 origin = startNode.transform.position;
        Vector3 dest = destNode.GetPlayerTargetPosition();
        var direction = (dest - origin).normalized;
        return dest - direction * destNode.NodeRadiusOffset;
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
    public void MovePlayerToNode(LevelNodeVisual destNodeVisual, VoidEvent onMoveComplete)
    {
        if (destNodeVisual.HasEntryAnimation())
        {
            Vector3 destPos = GetNodeEdgePos(m_CurrentNodeVisual, destNodeVisual);
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
    
    public void MovePlayerToNode(SplineContainer pathSpline, LevelNodeVisual destNodeVisual, VoidEvent onMoveComplete)
    {
        var initialDirection = GetInitialSplineForwardDirection(pathSpline);
        var initialRotation = Quaternion.LookRotation(-initialDirection, m_PlayerToken.transform.up);
        var pathOffset = Vector3.zero;
        var finalDirection = GetFinalSplineForwardDirection(pathSpline);
        var finalRotation = Quaternion.LookRotation(-finalDirection, m_PlayerToken.transform.up);
        var startOffset = m_CurrentNodeVisual.NodeRadiusOffset;
        var endOffset = destNodeVisual.NodeRadiusOffset;

        // Exiting the current node
        var t = startOffset / pathSpline.CalculateLength();
        var pathStartPos = (Vector3) pathSpline.EvaluatePosition(t);
        m_PlayerUnitToken.MoveToPosition(pathStartPos, initialRotation, OnMoveToPathStartPos, NODE_EXIT_TIME);

        // Moving along spline
        void OnMoveToPathStartPos()
        {
            m_PlayerUnitToken.MoveAlongSpline( pathSpline, pathOffset, finalRotation, OnMoveToEdgeComplete, startOffset, endOffset, MOVE_SPEED);
        }
        
        // Entering the destination node
        void OnMoveToEdgeComplete()
        {
            if (destNodeVisual.HasEntryAnimation())
                destNodeVisual.PlayEntryAnimation(m_PlayerUnitToken, onMoveComplete);
            else
            {
                Vector3 destPos = destNodeVisual.GetPlayerTargetPosition();
                m_PlayerUnitToken.MoveToPosition(destPos, finalRotation, onMoveComplete, NODE_ENTRY_TIME);
            }
        }
        
        m_CurrentNodeVisual = destNodeVisual;
    }
    
    public void PlayClearAnimation(LevelNodeVisual nodeVisual, VoidEvent onComplete)
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
    
    public void PlayFailureAnimation(LevelNodeVisual nodeVisual, VoidEvent onComplete, bool resetOnComplete = false)
    {
        if (nodeVisual.HasFailureAnimation())
        {
            nodeVisual.PlayFailureAnimation(m_PlayerUnitToken, onComplete, resetOnComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (m_PlayerUnitToken)
            Destroy(m_PlayerUnitToken.gameObject);
    }

    private Vector3 GetInitialSplineForwardDirection(SplineContainer splineContainer)
    {
        return ((Vector3) (splineContainer.Spline[1].Position - splineContainer.Spline[0].Position)).normalized;
    }

    private Vector3 GetFinalSplineForwardDirection(SplineContainer splineContainer)
    {
        return ((Vector3) (splineContainer.Spline[^1].Position - splineContainer.Spline[^2].Position)).normalized;
    }

    #endregion

}
