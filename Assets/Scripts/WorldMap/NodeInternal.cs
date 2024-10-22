using UnityEngine;

public abstract class NodeInternal : MonoBehaviour
{
    protected bool m_IsCurrent;

    public abstract void OnNodeEnter();
    
    public abstract void OnNodeExit();
}