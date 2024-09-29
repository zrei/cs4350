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
        var nodeAPosition = m_EdgeInternal.NodeInternalA.transform.position;
        m_LineRenderer.SetPosition(0, nodeAPosition);
        var nodeBPosition = m_EdgeInternal.NodeInternalB.transform.position;
        m_LineRenderer.SetPosition(1, nodeBPosition);
        
        // Set Text position to the middle of the line
        m_CostText.transform.position = (nodeAPosition + nodeBPosition) / 2 + new Vector3(0f, 0.5f, 0f);
        m_CostText.text = m_EdgeInternal.Cost.ToString();
        m_CostText.enabled = true;
    }
    
    #endregion
}
