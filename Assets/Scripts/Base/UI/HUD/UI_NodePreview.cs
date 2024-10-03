using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Temporary UI for node preview, to be integrated with UI_Manager System
public class UI_NodePreview : MonoBehaviour
{
    [Header("General Preview")]
    [SerializeField] GameObject m_NodePreviewPanel;
    [SerializeField] TextMeshProUGUI m_NodeNameText;
    [SerializeField] TextMeshProUGUI m_DescriptionText;
    
    [Header("Battle Preview")]
    [SerializeField] GameObject m_BattlePreviewPanel;
    [SerializeField] TextMeshProUGUI m_BattleNodeNameText;
    [SerializeField] TextMeshProUGUI m_BattleDescriptionText;
    [SerializeField] TextMeshProUGUI m_BattleEnemiesText;
    
    // Should access from camera manager in the future
    [SerializeField] Camera m_Camera;
    
    // Time to hover before showing the preview panel
    [SerializeField] float m_TimeToHover = 0.5f;
    private float m_HoverTimeLeft = 0f;
    private bool m_IsHovering = false;
    
    private GameObject m_CurrentPreviewPanel;

    private void Awake()
    {
        m_NodePreviewPanel.SetActive(false);
        m_BattlePreviewPanel.SetActive(false);
        GlobalEvents.Level.NodeHoverStartEvent += OnHoverStart;
        GlobalEvents.Level.NodeHoverEndEvent += OnHoverEnd;
        GlobalEvents.Level.BattleNodeHoverStartEvent += OnBattleHoverStart;
    }

    private void OnDestroy()
    {
        GlobalEvents.Level.NodeHoverStartEvent -= OnHoverStart;
        GlobalEvents.Level.NodeHoverEndEvent -= OnHoverEnd;
        GlobalEvents.Level.BattleNodeHoverStartEvent -= OnBattleHoverStart;
    }

    private void Update()
    {
        if (!m_IsHovering)
            return;
        
        m_HoverTimeLeft -= Time.deltaTime;
        if (m_HoverTimeLeft <= 0f)
        {
            m_IsHovering = false;
            Show();
        }
    }

    private void OnHoverStart(NodeInternal node)
    {
        SetUpGeneralPreviewPanel(node);
        
        // Start hover timer
        m_HoverTimeLeft = m_TimeToHover;
        m_IsHovering = true;
    }
    
    private void OnBattleHoverStart(BattleNode node)
    {
        SetUpBattlePreviewPanel(node);
        
        // Start hover timer
        m_HoverTimeLeft = m_TimeToHover;
        m_IsHovering = true;
    }
    
    private void OnHoverEnd()
    {
        m_IsHovering = false;
        Hide();
    }

    private void Show()
    {
        m_CurrentPreviewPanel.SetActive(true);
    }
    
    private void Hide()
    {
        m_CurrentPreviewPanel.SetActive(false);
    }
    
    private void SetUpGeneralPreviewPanel(NodeInternal node)
    {
        m_NodeNameText.text = node.NodeInfo.m_NodeName;
        m_DescriptionText.text = node.NodeInfo.m_NodeDescription;
        
        // Set position of preview panel to be at the node's position
        var nodePosition = node.transform.position;
        var screenPosition = m_Camera.WorldToScreenPoint(nodePosition);
        var viewportPosition = m_Camera.ScreenToViewportPoint(screenPosition);
        
        var rectTransform = m_NodePreviewPanel.GetComponent<RectTransform>();
        
        if (viewportPosition.x < 0.8f)
            rectTransform.anchoredPosition = screenPosition + Vector3.right * 240f;
        else
            rectTransform.anchoredPosition = screenPosition + Vector3.left * 240f;
        
        m_CurrentPreviewPanel = m_NodePreviewPanel;
    }
    
    private void SetUpBattlePreviewPanel(BattleNode node)
    {
        m_BattleNodeNameText.text = node.NodeInfo.m_NodeName;
        m_BattleDescriptionText.text = node.NodeInfo.m_NodeDescription;

        // Get number of enemies by class
        Dictionary<string, int> enemyClassCount = new Dictionary<string, int>();
        foreach (var enemy in node.BattleSO.m_EnemyUnitsToSpawn)
        {
            if (!enemyClassCount.TryAdd(enemy.m_EnemyCharacterData.m_StartingClass.m_ClassName, 1))
                enemyClassCount[enemy.m_EnemyCharacterData.m_StartingClass.m_ClassName]++;
        }
        
        // List enemies by their classes
        m_BattleEnemiesText.text = "Enemies: \n";
        foreach (var enemyClass in enemyClassCount)
        {
            m_BattleEnemiesText.text += $"{enemyClass.Key}\tx{enemyClass.Value}\n";
        }
        
        // Set position of preview panel to be at the node's position
        var nodePosition = node.transform.position;
        var screenPosition = m_Camera.WorldToScreenPoint(nodePosition);
        var viewportPosition = m_Camera.ScreenToViewportPoint(screenPosition);
        
        var rectTransform = m_BattlePreviewPanel.GetComponent<RectTransform>();
        
        if (viewportPosition.x < 0.8f)
            rectTransform.anchoredPosition = screenPosition + Vector3.right * 240f;
        else
            rectTransform.anchoredPosition = screenPosition + Vector3.left * 240f;
        
        m_CurrentPreviewPanel = m_BattlePreviewPanel;
    }
}