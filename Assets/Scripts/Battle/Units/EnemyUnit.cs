public class EnemyUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.ENEMY;

    private EnemyActionSetSO m_Actions;

    public void Initialise(Stats stats, ClassSO enemyClass, EnemyActionSetSO actionSet)
    {
        base.Initialise(stats, enemyClass);
        m_Actions = actionSet;
    }

    public void PerformAction(MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        m_Actions.PerformAction(this, mapLogic, completeActionEvent);
    }
}
