using System.Collections.Generic;
using UnityEngine;

public class EnemyPassActionWrapper : EnemyActionWrapper
{
    public override bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return false;
    }

    public override void Run(EnemyUnit enemyUnit, MapLogic mapLogic, BoolEvent completeActionEvent)
    {
        completeActionEvent?.Invoke(false);
    }

    public override HashSet<ActiveSkillSO> GetNestedActiveSkills()
    {
        return new();
    }
}

public class EnemyPassAction : EnemyActionInstance
{
    public override IConcreteAction GenerateConcreteAction()
    {
        return new EnemyPassActionWrapper {m_Action = this};
    }   
}
