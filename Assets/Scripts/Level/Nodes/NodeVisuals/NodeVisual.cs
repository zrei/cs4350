using TMPro;
using UnityEngine;

/// <summary>
/// Base class that handles the visuals of a node, e.g. highlighting the node
/// </summary>
public abstract class NodeVisual : MonoBehaviour
{
    
    [SerializeField] public TextMeshPro m_NodeStateText;
    [SerializeField] TextMeshPro m_IsSelectedText;
    [SerializeField] TextMeshPro m_IsMovableText;
    
    // Puck Visuals
    [SerializeField] MeshRenderer m_MeshRenderer;
    [SerializeField] NodeColorSO m_NodeColorSO;

    #region Initialisation
    public abstract void Initialise();
    #endregion
    
    #region State
    public abstract void UpdateNodeVisualState();
    #endregion

    #region Graphics
    
    public void SetNodeState(NodePuckType puckType)
    {
        m_MeshRenderer.material = m_NodeColorSO.GetMaterial(puckType);
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
