using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPassActionSO", menuName = "ScriptableObject/Battle/EnemyAI/Actions/EnemyPassActionSO")]
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
