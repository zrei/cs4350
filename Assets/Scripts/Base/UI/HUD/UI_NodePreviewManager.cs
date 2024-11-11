using UnityEngine;

// Manages display of node previews, both anchored and hover
public class UI_NodePreviewManager : MonoBehaviour
{
    [SerializeField] UI_NodePreview m_HoverNodePreview;
    [SerializeField] UI_NodePreview m_AnchoredNodePreview;
    
    private GameObject m_CurrentPreviewPanel;
    private NodeInternal m_CurrSelectedNode;

    private void Awake()
    {
        Hide();
        GlobalEvents.Scene.LevelSceneLoadedEvent += OnSceneLoad;
    }

    private void OnSceneLoad()
    {
        GlobalEvents.Scene.LevelSceneLoadedEvent -= OnSceneLoad;
        
        EnablePreview();
        GlobalEvents.Dialogue.DialogueStartEvent += DisablePreview;
        GlobalEvents.Dialogue.DialogueEndEvent += EnablePreview;
    }

    private void OnDestroy()
    {
        DisablePreview();
        GlobalEvents.Scene.LevelSceneLoadedEvent -= OnSceneLoad;
        GlobalEvents.Dialogue.DialogueStartEvent -= DisablePreview;
        GlobalEvents.Dialogue.DialogueEndEvent -= EnablePreview;
    }

    private void EnablePreview()
    {
        GlobalEvents.Level.NodeHoverStartEvent += OnHoverStart;
        GlobalEvents.Level.NodeHoverEndEvent += OnHoverEnd;
        GlobalEvents.Level.NodeSelectedEvent += OnNodeSelected;
        GlobalEvents.Level.NodeDeselectedEvent += OnNodeDeselected;
    }

    private void DisablePreview()
    {
        GlobalEvents.Level.NodeHoverStartEvent -= OnHoverStart;
        GlobalEvents.Level.NodeHoverEndEvent -= OnHoverEnd;
        GlobalEvents.Level.NodeSelectedEvent -= OnNodeSelected;
        GlobalEvents.Level.NodeDeselectedEvent -= OnNodeDeselected;
        Hide();
    }

    private void OnHoverStart(NodeInternal node)
    {
        if (m_CurrSelectedNode == node) return;
        
        m_HoverNodePreview.SetUpPreview(node);
        m_HoverNodePreview.Show();
    }
    
    private void OnHoverEnd()
    {
        m_HoverNodePreview.Hide();
    }
    
    private void OnNodeSelected(NodeInternal node)
    {
        m_CurrSelectedNode = node;
        m_AnchoredNodePreview.SetUpPreview(node);
        m_AnchoredNodePreview.Show();
        m_HoverNodePreview.Hide();
    }
    
    private void OnNodeDeselected(NodeInternal node)
    {
        m_CurrSelectedNode = null;
        m_AnchoredNodePreview.Hide();
    }
    
    private void Hide()
    {
        m_HoverNodePreview.Hide();
        m_AnchoredNodePreview.Hide();
    }
}