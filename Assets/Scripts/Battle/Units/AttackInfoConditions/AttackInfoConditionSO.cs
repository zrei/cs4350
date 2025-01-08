using UnityEngine;

public abstract class AttackInfoConditionSO : ScriptableObject
{
    public abstract bool IsConditionMet(AttackInfo attackInfo);
}
