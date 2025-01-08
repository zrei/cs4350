public class EnemyTurnManager : TurnManager
{
    private EnemyUnit m_CurrUnit;

    public void PerformTurn(EnemyUnit enemyUnit)
    {
        m_CurrUnit = enemyUnit;
        Logger.Log(this.GetType().Name, "Start enemy turn with " + m_CurrUnit.name, LogLevel.LOG);

        m_CurrUnit.PreTick();
        m_MapLogic.ApplyTileEffectOnUnit(GridType.ENEMY, m_CurrUnit);

        if (m_CurrUnit.IsDead)
        {
            m_CurrUnit.Die();
            EndTurn();
            return;
        }

        if (!PreTurn(enemyUnit))
        {
            EndTurn();
            return;
        }   

        m_CurrUnit.StartTurn();

        GlobalEvents.Battle.EnemyTurnStartEvent?.Invoke();

        ChooseAction();
    }

    private void ChooseAction()
    {
        GlobalEvents.Battle.PreviewCurrentUnitEvent?.Invoke(m_CurrUnit);
        m_CurrUnit.PerformAction(m_MapLogic, CompleteAction);
    }

    private void CompleteAction(bool canExtendTurn)
    {
        if (!canExtendTurn)
            EndTurn();
        else
            ChooseAction();
    }

    private void EndTurn()
    {
        m_CurrUnit.PostTick();
        m_CompleteTurnEvent?.Invoke(m_CurrUnit);
    }
}
