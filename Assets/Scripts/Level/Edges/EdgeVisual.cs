using TMPro;
using UnityEngine;

public class EdgeVisual : MonoBehaviour
{
    EdgeInternal m_EdgeInternal;
    
    [SerializeField] private LineRenderer m_LineRenderer;
    [SerializeField] private TextMeshPro m_CostText;
    
    #region Initialisation
    
    public void Initialise(EdgeInternal edgeInternal)
    {
        m_EdgeInternal = edgeInternal;
        
        // Draws a line to connect the nodes
        m_LineRenderer.positionCount = 2;
        m_LineRenderer.SetPosition(0, m_EdgeInternal.NodeInternalA.transform.position);
        m_LineRenderer.SetPosition(1, m_EdgeInternal.NodeInternalB.transform.position);
        
        m_CostText.text = m_EdgeInternal.Cost.ToString();
    }
    
    #endregion
}
