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

    /// <summary>
    /// Performs any pre-turn logic and returns whether the turn can be performed
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    protected bool PreTurn(Unit unit)
    {
        bool canPerformTurn = unit.CanPerformTurn();
        unit.ClearTokens(TokenConsumptionType.CONSUME_ON_NEXT_TURN);
        Logger.Log(this.GetType().Name, $"Unit {unit.name} can perform turn: {canPerformTurn}", LogLevel.LOG);
        return canPerformTurn;
    }
}
