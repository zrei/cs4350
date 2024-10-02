using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/// <summary>
/// Base class that handles the visuals of a node, e.g. highlighting the node
/// </summary>
public abstract class NodeVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Puck Visuals
    [SerializeField] MeshRenderer m_MeshRenderer;
    [SerializeField] NodeColorSO m_NodeColorSO;
    
    // Common Tokens for nodes
    
    // Star token model for goal nodes
    [SerializeField] private GameObject m_starToken;
    // Cursor token model for movable nodes
    [SerializeField] private GameObject m_movableCursorToken;
    // Cursor token model for selected nodes
    [SerializeField] private GameObject m_selectedCursorToken;

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
    
    public void SetNodeState(NodePuckType puckType)
    {
        m_MeshRenderer.material = m_NodeColorSO.GetMaterial(puckType);
    }

    public void ToggleSelected(bool isSelected)
    {
        m_selectedCursorToken.SetActive(isSelected);
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
    #endregion
}
