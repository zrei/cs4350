using UnityEngine;

public abstract class TurnManager : MonoBehaviour
{
    protected VoidEvent m_CompleteTurnEvent;

    public void Initialise(VoidEvent completeTurnEvent)
    {
        m_CompleteTurnEvent = completeTurnEvent;
    }
}