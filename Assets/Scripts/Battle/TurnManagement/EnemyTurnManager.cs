public class EnemyTurnManager : TurnManager
{
    public void PerformTurn(EnemyUnit enemyUnit)
    {
        Logger.Log(this.GetType().Name, "Start enemy turn with " + enemyUnit.name, LogLevel.LOG);
        GlobalEvents.Battle.EnemyTurnStartEvent?.Invoke();
        m_CompleteTurnEvent?.Invoke(enemyUnit);
    }
}