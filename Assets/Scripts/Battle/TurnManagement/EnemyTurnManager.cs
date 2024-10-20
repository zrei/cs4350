using System.Collections;
using UnityEngine;

public class EnemyTurnManager : TurnManager
{
    private EnemyUnit m_CurrUnit;

    public void PerformTurn(EnemyUnit enemyUnit)
    {
        m_CurrUnit = enemyUnit;
        Logger.Log(this.GetType().Name, "Start enemy turn with " + m_CurrUnit.name, LogLevel.LOG);

        m_CurrUnit.Tick();

        if (m_CurrUnit.IsDead)
        {
            GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(m_CurrUnit);
            CompleteTurn();
            return;
        }

        if (!PreTurn(enemyUnit))
        {
            CompleteTurn();
            return;
        }   

        GlobalEvents.Battle.EnemyTurnStartEvent?.Invoke();
        GlobalEvents.Battle.PreviewCurrentUnitEvent?.Invoke(m_CurrUnit);

        enemyUnit.PerformAction(m_MapLogic, CompleteTurn);
    }

    private void CompleteTurn()
    {
        m_CompleteTurnEvent?.Invoke(m_CurrUnit);
    }
}