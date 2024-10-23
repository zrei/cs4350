using UnityEngine;

public abstract class WorldNodeInternal : MonoBehaviour
{
    protected bool m_IsCurrent;

    public abstract void OnNodeEnter();
    
    public abstract void OnNodeExit();
}