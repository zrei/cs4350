using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for handling visuals for Battle Nodes
/// </summary>
[RequireComponent(typeof(BattleNode))]
public class BattleNodeVisual : NodeVisual
{
    private BattleNode m_BattleNode;
    
    [Header("Tokens")]
    [SerializeField] EnemyUnit m_EnemyUnit;
    [SerializeField] Transform m_EnemyTokenTransform;
    [SerializeField] Transform m_PlayerTokenTransform;
    public Transform PlayerTokenTransform => m_PlayerTokenTransform;
    
    private EnemyUnit m_EnemyUnitTokenInstance;
    
    [Header("HoverPreview")]
    [SerializeField] GameObject m_HoverPreview;
    
    
    public override void Initialise()
    {
        m_BattleNode = GetComponent<BattleNode>();
        
        if (m_BattleNode.IsGoalNode)
            ToggleStarOn();
        
        var unitPlacement = m_BattleNode.BattleSO.m_EnemyUnitsToSpawn[0];
        
        m_EnemyUnitTokenInstance = Instantiate(m_EnemyUnit);
        m_EnemyUnitTokenInstance.Initialise(unitPlacement.m_Stats, unitPlacement.m_Class, unitPlacement.m_Actions, unitPlacement.m_EnemySprite, unitPlacement.GetUnitModelData());
        var tokenTransform = m_EnemyUnitTokenInstance.transform;
        tokenTransform.localScale = Vector3.one * 0.45f;
        tokenTransform.SetParent(transform);
        tokenTransform.localPosition = Vector3.up * 0.1f;
    }

    #region Graphics
    public override void UpdateNodeVisualState()
    {
        if (m_BattleNode.IsCurrent)
        {
            SetNodeState(NodePuckType.CURRENT);
            
            if (m_BattleNode.IsCleared)
            {
                m_EnemyUnitTokenInstance.gameObject.SetActive(false);
            }
            else
            {
                m_EnemyUnitTokenInstance.gameObject.SetActive(true);
                
                // Set position to be facing off with player token
                var tokenTransform = m_EnemyUnitTokenInstance.transform;
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
                m_EnemyUnitTokenInstance.gameObject.SetActive(false);
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
}
