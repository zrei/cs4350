using UnityEngine;

public class EnemyUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.ENEMY;

    private EnemyActionSetSO m_Actions;

    public void Initialise(Stats stats, ClassSO enemyClass, EnemyActionSetSO actionSet, Sprite enemySprite)
    {
        base.Initialise(stats, enemyClass, enemySprite);
        m_Actions = actionSet;
    }

    public void PerformAction(MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        m_Actions.PerformAction(this, mapLogic, completeActionEvent);
    }
}
