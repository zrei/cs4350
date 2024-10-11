using UnityEngine;

public class EnemyPassActionWrapper : EnemyActionWrapper
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

[CreateAssetMenu(fileName = "EnemyPassActionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Actions/EnemyPassActionSO")]
public class EnemyPassActionSO : EnemyActionSO
{
    public override EnemyActionWrapper GetWrapper(int priority)
    {
        return new EnemyPassActionWrapper {m_Action = this, m_Priority = priority};
    }   
}
