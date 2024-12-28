using System;
using Level;
using Level.Nodes;
using Level.Nodes.NodeVisuals;
using Level.Tokens;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/// <summary>
/// Base class that handles the visuals of a level node, e.g. highlighting the node
/// </summary>
public class LevelNodeVisual : BaseNodeVisual
{
    private LevelNode m_LevelNode;
    private NodeDataSO m_NodeData => m_LevelNode.NodeData;

    public override float NodeRadiusOffset => 0.25f;
    
    [Header("Node Puck Configuration")]
    public float nodeRadius = 0.25f;
    public float nodeExpandedRadius = 0.3f;
    [Tooltip("SO containing colors for the respective node types")]
    [SerializeField] NodeColorSO m_NodeColorSO;
    
    [Header("Common Node Tokens")]
    // Star token model for goal nodes
    [SerializeField] private GameObject m_starToken;
    // Cursor token model for movable nodes
    [SerializeField] private GameObject m_movableCursorToken;
    
    [Header("Type-Specific Node Tokens")]
    public GameObject m_RationTokenPrefab;
    public GameObject m_ChestTokenPrefab;
    public GameObject m_DialogueTokenPrefab;

    [Header("Type-Specific Token Animators")] 
    public StaticLevelNodeTokenAnimator staticTokenAnimatorPrefab;
    public BattleNodeTokenAnimator battleTokenAnimatorPrefab;

    private LevelNodeTokenAnimator m_DefaultTokenAnimator;
    private LevelNodeTokenAnimator m_NodeTokenAnimator;
    
    [Header("Morality")]
    [SerializeField] private MoralityThresholdDisplay m_MoralityThresholdDisplay;

    #region Initialisation

    public override void Initialise()
    {
        m_LevelNode = GetComponent<LevelNode>();
        SetUpTokenAnimator();
        
        if (m_LevelNode.IsGoalNode)
            ToggleStarOn();
        
        m_MoralityThresholdDisplay.Initialise();
        
        if (m_LevelNode.IsMoralityLocked)
            SetMoralityThresholdText(m_LevelNode.MoralityCondition);
    }

    private void SetUpTokenAnimator()
    {
        // Set up default token animator
        var emptyTokenAnimator = gameObject.AddComponent<EmptyLevelNodeTokenAnimator>();
        emptyTokenAnimator.Initialise(this);
        m_DefaultTokenAnimator = emptyTokenAnimator;
        
        if (m_NodeData.nodeType == NodeType.BATTLE)
        {
            var battleNodeTokenAnimator = Instantiate(battleTokenAnimatorPrefab, transform, false);
            battleNodeTokenAnimator.Initialise(((BattleNodeDataSO) m_NodeData).GetDisplayEnemyUnit(), this);
            m_NodeTokenAnimator = battleNodeTokenAnimator;
        }
        else
        {
            GameObject staticTokenPrefab = null;
            switch (m_NodeData.nodeType)
            {
                case NodeType.DIALOGUE:
                    staticTokenPrefab = m_DialogueTokenPrefab;
                    break;
                case NodeType.REWARD:
                {
                    var rewardNodeData = (RewardNodeDataSO) m_NodeData;
                    staticTokenPrefab = rewardNodeData.rewardType switch
                    {
                        RewardType.RATION => m_RationTokenPrefab,
                        RewardType.WEAPON => m_ChestTokenPrefab,
                        _ => null
                    };
                    break;
                }
            }

            if (staticTokenPrefab != null)
            {
                var staticTokenAnimator = Instantiate(staticTokenAnimatorPrefab, transform, false);
                staticTokenAnimator.Initialise(staticTokenPrefab, this);
                m_NodeTokenAnimator = staticTokenAnimator;
            }
            else
            {
                // If node type is EMPTY, use the default token animator
                m_NodeTokenAnimator = m_DefaultTokenAnimator;
            }
        }
    }

    #endregion
    
    #region Graphics
    
    public override void UpdateNodeVisualState()
    {
        if (m_LevelNode.IsCurrent)
        {
            SetNodeColor(NodePuckType.CURRENT);
        }
        else if (m_LevelNode.IsCleared)
        {
            SetNodeColor(NodePuckType.CLEARED);
        }
        else
        {
            var nodeColorType = m_NodeData.nodeType switch
            {
                NodeType.DIALOGUE => NodePuckType.DIALOGUE,
                NodeType.REWARD => NodePuckType.REWARD,
                NodeType.BATTLE => NodePuckType.BATTLE,
                NodeType.EMPTY => NodePuckType.CLEARED,
                _ => NodePuckType.CLEARED,
            };
            SetNodeColor(nodeColorType);
            m_NodeTokenAnimator.ShowToken();
        }
    }
    
    public void SetNodeColor(NodePuckType puckType)
    {
        m_MeshRenderer.material = m_NodeColorSO.GetMaterial(puckType);
    }

    public void ToggleMovable(bool isMovable)
    {
        m_movableCursorToken.SetActive(isMovable);
    }
    
    public void ToggleStarOn()
    {
        m_starToken.SetActive(true);

        // Add offsets for cursor tokens
        var position = m_starToken.transform.position;
        m_movableCursorToken.transform.position = position + new Vector3(0, 0.3f, 0.3f);
        m_selectedCursorToken.transform.position = position + new Vector3(0, 0.3f, 0.3f);
    }
    
    public void SetMoralityThresholdText(MoralityCondition moralityCondition)
    {
        m_MoralityThresholdDisplay.SetMoralityThresholdText(moralityCondition);
        m_MoralityThresholdDisplay.Show();
    }
    
    #endregion

    #region Token Animations

    public void PlayEntryAnimation(PlayerToken playerToken, VoidEvent onComplete)
    {
        if (m_LevelNode.IsCleared)
            m_NodeTokenAnimator = m_DefaultTokenAnimator;
        
        m_NodeTokenAnimator.PlayEntryAnimation(playerToken, onComplete);
    }

    public void PlayClearAnimation(PlayerToken playerToken, VoidEvent onComplete)
    {
        if (m_LevelNode.IsCleared)
            m_NodeTokenAnimator = m_DefaultTokenAnimator;

        m_NodeTokenAnimator.PlayClearAnimation(playerToken, onComplete);
    }

    public void PlayFailureAnimation(PlayerToken playerToken, VoidEvent onComplete, bool resetOnComplete)
    {
        if (m_LevelNode.IsCleared)
            m_NodeTokenAnimator = m_DefaultTokenAnimator;

        m_NodeTokenAnimator.PlayFailureAnimation(playerToken, onComplete, resetOnComplete);
    }
    
    public void PlayBattleSkipAnimation(PlayerToken playerToken, VoidEvent onComplete)
    {
        if (m_NodeTokenAnimator is not BattleNodeTokenAnimator battleNodeTokenAnimator)
        {
            Debug.LogError($"{name}: No Battle Node Animator to play skip animation");
            onComplete?.Invoke();
            return;
        }

        battleNodeTokenAnimator.PlayBattleSkipAnimation(playerToken, onComplete);
    }

    #endregion

    #region Hover

    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Pointer entered Node: {m_NodeData.name}");
        GlobalEvents.Level.NodeHoverStartEvent?.Invoke(m_LevelNode);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Pointer exited Node: {m_NodeData.name}");
        GlobalEvents.Level.NodeHoverEndEvent?.Invoke();
    }

    #endregion
}
