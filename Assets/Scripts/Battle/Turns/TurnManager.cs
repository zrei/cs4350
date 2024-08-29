using UnityEngine;

public abstract class TurnManager : MonoBehaviour
{
    protected VoidEvent m_CompleteTurnEvent;
    protected MapLogic m_MapLogic;

    public void Initialise(VoidEvent completeTurnEvent, MapLogic mapLogic)
    {
        m_CompleteTurnEvent = completeTurnEvent;
        m_MapLogic = mapLogic;
    }
}