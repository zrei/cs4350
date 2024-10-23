using UnityEngine;

[CreateAssetMenu(fileName = "EnemyUnitManaThresholdConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/EnemyUnitManaThresholdConditionSO")]
public class EnemyUnitManaThresholdConditionSO : EnemyActionConditionSO
{
    public Threshold m_ManaThreshold;
    [Tooltip("Whether this is checking the flat mana amounts or not")]
    public bool m_ChecksFlat;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_ManaThreshold.IsSatisfied(m_ChecksFlat ? enemyUnit.CurrentMana : enemyUnit.CurrentManaProportion);
    }
}
