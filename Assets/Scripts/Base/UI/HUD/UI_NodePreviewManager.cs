using Level.Nodes;
using UnityEngine;

// Manages display of node previews, both anchored and hover
public class UI_NodePreviewManager : MonoBehaviour
{
    [SerializeField] UI_NodePreview m_HoverNodePreview;
    [SerializeField] UI_NodePreview m_AnchoredNodePreview;
    
    private GameObject m_CurrentPreviewPanel;
    private LevelNode m_CurrSelectedNode;

    private void Awake()
    {
        Hide();
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;
    }

    private void OnSceneLoad(SceneEnum fromScene, SceneEnum toScene)
    {
        if (toScene != SceneEnum.LEVEL)
            return;

        GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;
        
        EnablePreview();
        GlobalEvents.Dialogue.DialogueStartEvent += DisablePreview;
        GlobalEvents.Dialogue.DialogueEndEvent += EnablePreview;
    }

    private void OnDestroy()
    {
        DisablePreview();
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;
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

    private void OnHoverStart(LevelNode node)
    {
        if (m_CurrSelectedNode == node) return;
        
        m_HoverNodePreview.SetUpPreview(node);
        m_HoverNodePreview.Show();
    }
    
    private void OnHoverEnd()
    {
        m_HoverNodePreview.Hide();
    }
    
    private void OnNodeSelected(LevelNode node)
    {
        m_CurrSelectedNode = node;
        m_AnchoredNodePreview.SetUpPreview(node);
        m_AnchoredNodePreview.Show();
        m_HoverNodePreview.Hide();
    }
    
    private void OnNodeDeselected(LevelNode node)
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