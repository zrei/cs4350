using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Base class that handles the visuals of a node, e.g. highlighting the node
/// </summary>
public abstract class BaseNodeVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public virtual Vector3 TokenOffset => new Vector3(0f, 0.1f, 0f);
    public virtual float NodeRadiusOffset => 0.3f;

    // Puck Visuals
    [SerializeField] protected MeshRenderer m_MeshRenderer;
    
    // Common Tokens for nodes
    // Cursor token model for selected nodes
    [SerializeField] protected GameObject m_selectedCursorToken;

    #region Initialisation
    public abstract void Initialise();
    #endregion
    
    #region State
    public abstract void UpdateNodeVisualState();
    #endregion
    
    #region Pointer Events
    
    public abstract void OnPointerEnter(PointerEventData eventData);
    public abstract void OnPointerExit(PointerEventData eventData);
    
    #endregion

    #region Graphics
    public void ToggleSelected(bool isSelected)
    {
        m_selectedCursorToken.SetActive(isSelected);
    }
    #endregion

    #region Token

    /// <summary>
    /// Get Player token's target final position after entering the node.
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 GetPlayerTargetPosition()
    {
        return transform.position + TokenOffset;
    }
    #endregion
}
