using UnityEngine;

[CreateAssetMenu(fileName = "MapHasUnitsManaThresholdConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/MapHasUnitsManaThresholdConditionSO")]
public class MapHasUnitsManaThresholdConditionSO : EnemyActionConditionSO
{
    public GridType m_GridType;
    public Threshold m_ManaThreshold;
    [Tooltip("Whether this is checking the flat mana amounts or not")]
    public bool m_IsFlat;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_IsInverted ^ mapLogic.HasAnyUnitWithManaThreshold(m_GridType, m_ManaThreshold, m_IsFlat);
    }
}
