using UnityEngine;

public abstract class EnemyActionSO : ScriptableObject
{
    public GridType m_TargetSide;

    public abstract bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic);

    public abstract void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent);
}

public class EnemyMoveActionSO : EnemyActionSO
{
    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        // check if there's any space to move to
    }

    public override void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {

    }
}

public class EnemyPassActionSO : EnemyActionSO
{
    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return true;
    }

    public override void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        completeActionEvent?.Invoke();
    }
}

// Actions under condition: [] --> perform when these conditions are true... have priority for which should come first if the same, it's randomised