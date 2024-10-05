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
            return;
        }

        if (!PreTurn(enemyUnit))
            return;

        GlobalEvents.Battle.EnemyTurnStartEvent?.Invoke();
        GlobalEvents.Battle.PreviewCurrentUnitEvent?.Invoke(m_CurrUnit);

        IEnumerator PerformTurn()
        {
            yield return new WaitForSeconds(2f);
            enemyUnit.PerformAction(m_MapLogic, CompleteTurn);
        }
        StartCoroutine(PerformTurn());
    }

    private void CompleteTurn()
    {
        m_CompleteTurnEvent?.Invoke(m_CurrUnit);
    }
}