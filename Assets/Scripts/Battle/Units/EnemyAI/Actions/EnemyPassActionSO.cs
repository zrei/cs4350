using System.Collections.Generic;
using UnityEngine;

public class EnemyPassActionWrapper : EnemyActionWrapper
{
    public override bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return false;
    }

    public override void Run(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        completeActionEvent?.Invoke();
    }

    public override HashSet<ActiveSkillSO> GetNestedActiveSkills()
    {
        return new();
    }
}

[CreateAssetMenu(fileName = "EnemyPassActionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Actions/EnemyPassActionSO")]
public class EnemyPassActionSO : EnemyActionSO
{
    public override IConcreteAction GenerateConcreteAction()
    {
        return new EnemyPassActionWrapper {m_Action = this};
    }   
}
