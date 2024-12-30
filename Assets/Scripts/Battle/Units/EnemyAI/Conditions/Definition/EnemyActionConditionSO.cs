using UnityEngine;

public abstract class EnemyActionConditionSO : ScriptableObject
{
    public bool m_IsInverted;

    public abstract bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic);
}
