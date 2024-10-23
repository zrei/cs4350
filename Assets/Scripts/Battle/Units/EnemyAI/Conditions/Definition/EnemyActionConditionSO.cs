using UnityEngine;

public abstract class EnemyActionConditionSO : ScriptableObject
{
    public abstract bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic);
}
