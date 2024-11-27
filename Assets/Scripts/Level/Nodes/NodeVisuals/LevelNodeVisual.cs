using Level;
using Level.Nodes.NodeVisuals;
using UnityEngine;

/// <summary>
/// Base class that handles the visuals of a level node, e.g. highlighting the node
/// </summary>
public abstract class LevelNodeVisual : BaseNodeVisual
{
    public override Vector3 TokenOffset => new Vector3(0f, 0.1f, 0f);
    public override float NodeRadiusOffset => 0.25f;

    // Puck Visuals
    [SerializeField] NodeColorSO m_NodeColorSO;
    
    // Common Tokens for nodes
    
    // Star token model for goal nodes
    [SerializeField] private GameObject m_starToken;
    // Cursor token model for movable nodes
    [SerializeField] private GameObject m_movableCursorToken;
    // Cursor token model for selected nodes
    
    [SerializeField] private MoralityThresholdDisplay m_MoralityThresholdDisplay;

    #region Graphics
    
    public void SetNodeState(NodePuckType puckType)
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
    
    public void SetMoralityThresholdText(Threshold moralityThreshold)
    {
        m_MoralityThresholdDisplay.SetMoralityThresholdText(moralityThreshold);
        m_MoralityThresholdDisplay.Show();
    }
    #endregion

    #region Token
    public virtual bool HasEntryAnimation()
    {
        return false;
    }

    public virtual void PlayEntryAnimation(PlayerToken playerToken, VoidEvent onComplete)
    {
        // No animation by default
        onComplete?.Invoke();
    }
    
    public virtual bool HasClearAnimation()
    {
        return false;
    }

    public virtual void PlayClearAnimation(PlayerToken playerToken, VoidEvent onComplete)
    {
        // No animation by default
        onComplete?.Invoke();
    }
    
    public virtual bool HasFailureAnimation()
    {
        return false;
    }

    public virtual void PlayFailureAnimation(PlayerToken playerToken, VoidEvent onComplete, bool resetOnComplete = false)
    {
        // No animation by default
        onComplete?.Invoke();
    }

    #endregion
}
