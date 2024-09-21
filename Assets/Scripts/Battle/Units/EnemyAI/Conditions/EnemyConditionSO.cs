using UnityEngine;

public abstract class EnemyConditionSO : ScriptableObject
{
    public abstract bool IsConidtionMet(EnemyUnit enemyUnit, MapLogic mapLogic);
}

public interface ICondition
{
    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic);
}

[System.Serializable]
public struct EnemyCondition : ICondition
{
    public EnemyConditionSO m_Condition;
    public float m_MultProportion;

    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic) => m_Condition.IsConidtionMet(enemyUnit, mapLogic);
}

[System.Serializable]
public struct ActiveSkillCondition : ICondition
{
    public EnemyConditionSO m_Condition;
    public float m_MultProportion;
    public SkillType m_AffectedSkillType;

    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic) => m_Condition.IsConidtionMet(enemyUnit, mapLogic);
}
