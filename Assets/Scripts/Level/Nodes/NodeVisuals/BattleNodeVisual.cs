using Level;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for handling visuals for Battle Nodes
/// </summary>
[RequireComponent(typeof(BattleNode))]
public class BattleNodeVisual : LevelNodeVisual
{
    [Header("Tokens")]
    [SerializeField] EnemyToken m_CharacterToken;
    [SerializeField] Transform m_EnemyTokenTransform;
    [SerializeField] Transform m_PlayerTokenTransform;
    
    private const float ENTRY_ANIM_TIME = 0.3f;
    private const float CLEAR_ANIM_TIME = 0.3f;

    public override float NodeRadiusOffset => 0.3f;

    private BattleNode m_BattleNode;
    private EnemyToken m_EnemyUnitToken;
    
    public override void Initialise()
    {
        m_BattleNode = GetComponent<BattleNode>();
        
        if (m_BattleNode.IsGoalNode)
            ToggleStarOn();
        
        if (m_BattleNode.IsMoralityLocked)
            SetMoralityThresholdText(m_BattleNode.MoralityThreshold);
        
        var enemyUnitData = m_BattleNode.BattleSO.m_EnemyUnitsToSpawn[0].m_EnemyCharacterData;
        
        m_EnemyUnitToken = Instantiate(m_CharacterToken, transform, true);
        m_EnemyUnitToken.Initialise(enemyUnitData);
        m_EnemyUnitToken.SetPositionToNode(this);
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

    #region Token
    
    public override bool HasEntryAnimation()
    {
        return !m_BattleNode.IsCleared;
    }

    public override void PlayEntryAnimation(PlayerToken playerToken, VoidEvent onComplete)
    {
        playerToken.MoveToPosition(m_PlayerTokenTransform.position, 
            m_PlayerTokenTransform.rotation, null, ENTRY_ANIM_TIME);
        m_EnemyUnitToken.MoveToPosition(m_EnemyTokenTransform.position, 
            m_EnemyTokenTransform.rotation, onComplete, ENTRY_ANIM_TIME);
    }
    
    public override bool HasClearAnimation()
    {
        return !m_BattleNode.IsCleared;
    }
    
    public override void PlayClearAnimation(PlayerToken playerToken, VoidEvent onComplete)
    {
        m_EnemyUnitToken.Die(OnEnemyDeathComplete);
        return;
        
        void OnEnemyDeathComplete()
        {
            m_EnemyUnitToken.gameObject.SetActive(false);
            playerToken.MoveToPosition(GetPlayerTargetPosition(), onComplete, CLEAR_ANIM_TIME);
        }
    }
    
    public override bool HasFailureAnimation()
    {
        return true;
    }
    
    public override void PlayFailureAnimation(PlayerToken playerToken, VoidEvent onComplete)
    {
        playerToken.Defeat(onComplete);
    }

    #endregion

    #region Hover
    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered Battle Node");
        GlobalEvents.Level.NodeHoverStartEvent?.Invoke(m_BattleNode);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited Battle Node");
        GlobalEvents.Level.NodeHoverEndEvent?.Invoke();
    }

    

    #endregion
}
