using Level;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for handling visuals for Battle Nodes
/// </summary>
[RequireComponent(typeof(BattleNode))]
public class BattleNodeVisual : NodeVisual
{
    [Header("Tokens")]
    [SerializeField] CharacterToken m_CharacterToken;
    [SerializeField] Transform m_EnemyTokenTransform;
    [SerializeField] Transform m_PlayerTokenTransform;
    
    private const float ENTRY_ANIM_TIME = 0.3f;
    
    private BattleNode m_BattleNode;
    private CharacterToken m_EnemyUnitToken;
    
    public override void Initialise()
    {
        m_BattleNode = GetComponent<BattleNode>();
        
        if (m_BattleNode.IsGoalNode)
            ToggleStarOn();
        
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
            
            if (m_BattleNode.IsCleared)
            {
                m_EnemyUnitToken.gameObject.SetActive(false);
            }
            else
            {
                m_EnemyUnitToken.gameObject.SetActive(true);
                
                // Set position to be facing off with player token
                var tokenTransform = m_EnemyUnitToken.transform;
                tokenTransform.localScale = Vector3.one * 0.4f;
                tokenTransform.position = m_EnemyTokenTransform.position;
                tokenTransform.rotation = m_EnemyTokenTransform.rotation;
            }
        }
        else
        {
            if (m_BattleNode.IsCleared)
            {
                SetNodeState(NodePuckType.CLEARED);
                m_EnemyUnitToken.gameObject.SetActive(false);
            }
            else
            {
                SetNodeState(NodePuckType.BATTLE);
            }
        }
    }
    
    public void SetPlayerToken(GameObject playerToken)
    {
        // Set position to be facing off with player token
        var tokenTransform = playerToken.transform;
        tokenTransform.localScale = Vector3.one * 0.4f;
        tokenTransform.position = m_PlayerTokenTransform.position;
        tokenTransform.rotation = m_PlayerTokenTransform.rotation;
    }
    #endregion

    #region Token
    
    public override bool HasEntryAnimation()
    {
        return !m_BattleNode.IsCleared;
    }

    public override void PlayEntryAnimation(CharacterToken playerToken, VoidEvent onComplete)
    {
        playerToken.MoveToPosition(m_PlayerTokenTransform.position, 
            m_PlayerTokenTransform.rotation, null, ENTRY_ANIM_TIME);
        m_EnemyUnitToken.MoveToPosition(m_EnemyTokenTransform.position, 
            m_EnemyTokenTransform.rotation, onComplete, ENTRY_ANIM_TIME);
    }

    #endregion

    #region Hover
    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered Battle Node");
        GlobalEvents.Level.BattleNodeHoverStartEvent?.Invoke(m_BattleNode);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited Battle Node");
        GlobalEvents.Level.NodeHoverEndEvent?.Invoke();
    }

    

    #endregion
}
