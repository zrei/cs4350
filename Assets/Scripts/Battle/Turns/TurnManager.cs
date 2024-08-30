using UnityEngine;

public abstract class TurnManager : MonoBehaviour
{
    protected GlobalEvents.Battle.UnitEvent m_CompleteTurnEvent;
    protected MapLogic m_MapLogic;

    public void Initialise(GlobalEvents.Battle.UnitEvent completeTurnEvent, MapLogic mapLogic)
    {
        m_CompleteTurnEvent = completeTurnEvent;
        m_MapLogic = mapLogic;
    }
}