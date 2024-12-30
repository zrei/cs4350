using UnityEngine;

[CreateAssetMenu(fileName = "NumberOfUnitsConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/NumberOfUnitsConditionSO")]
public class NumberOfUnitsConditionSO : EnemyActionConditionSO 
{
    public Threshold m_NumberUnitsThreshold;
    public GridType m_GridType;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_IsInverted ^ m_NumberUnitsThreshold.IsSatisfied(mapLogic.GetNumberOfUnitsOnGrid(m_GridType));
    }
}
