using TMPro;
using UnityEngine;

// Temporary UI for node preview, to be integrated with UI_Manager System
public class UI_NodePreview : MonoBehaviour
{
    [SerializeField] GameObject m_NodePreviewPanel;
    [SerializeField] TextMeshProUGUI m_NodeNameText;
    [SerializeField] TextMeshProUGUI m_DescriptionText;
    
    // Should access from camera manager in the future
    [SerializeField] Camera m_Camera;
    
    // Time to hover before showing the preview panel
    [SerializeField] float m_TimeToHover = 0.5f;
    private float m_HoverTimeLeft = 0f;
    private bool m_IsHovering = false;

    private void Awake()
    {
        m_NodePreviewPanel.SetActive(false);
        GlobalEvents.Level.NodeHoverStartEvent += OnHoverStart;
        GlobalEvents.Level.NodeHoverEndEvent += OnHoverEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.Level.NodeHoverStartEvent -= OnHoverStart;
        GlobalEvents.Level.NodeHoverEndEvent -= OnHoverEnd;
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
        SetUpPreviewPanel(node);
        
        // Start hover timer
        m_HoverTimeLeft = m_TimeToHover;
        m_IsHovering = true;
    }
    
    public void OnHoverEnd(NodeInternal node)
    {
        m_IsHovering = false;
        m_NodeNameText.text = "";
        m_DescriptionText.text = "";
        Hide();
    }

    private void Show()
    {
        m_NodePreviewPanel.SetActive(true);
    }
    
    private void Hide()
    {
        m_NodePreviewPanel.SetActive(false);
    }
    
    private void SetUpPreviewPanel(NodeInternal node)
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
    }
}