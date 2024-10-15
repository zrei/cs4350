using UnityEngine;

public abstract class EnemyActionConditionSO : ScriptableObject
{
    public abstract bool IsConidtionMet(EnemyUnit enemyUnit, MapLogic mapLogic);
}
