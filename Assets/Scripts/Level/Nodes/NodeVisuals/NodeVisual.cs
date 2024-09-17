using TMPro;
using UnityEngine;

/// <summary>
/// Base class that handles the visuals of a node, e.g. highlighting the node
/// </summary>
public abstract class NodeVisual : MonoBehaviour
{
    
    [SerializeField] TextMeshPro m_NodeStateText;
    [SerializeField] TextMeshPro m_IsSelectedText;
    [SerializeField] TextMeshPro m_IsMovableText;

    #region Initialisation
    public abstract void Initialise();
    #endregion
    
    #region State
    public abstract void UpdateNodeVisualState();
    #endregion

    #region Graphics
    
    public void SetNodeState(string state)
    {
        m_NodeStateText.text = state;
    }

    public void ToggleSelected(bool isSelected)
    {
        m_IsSelectedText.gameObject.SetActive(isSelected);
    }

    public void ToggleMovable(bool isMovable)
    {
        m_IsMovableText.gameObject.SetActive(isMovable);
    }
    #endregion
}
