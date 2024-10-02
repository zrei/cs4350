using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPassActionSO", menuName = "ScriptableObject/Battle/EnemyAI/Actions/EnemyPassActionSO")]
public class EnemyPassActionSO : EnemyActionSO
{
    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return true;
    }

    public void PassTurn(VoidEvent completeActionEvent)
    {
        completeActionEvent?.Invoke();
    }
}
