using UnityEngine;

[CreateAssetMenu(fileName = "NumberOfEnemyUnitsConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/NumberOfEnemyUnitsConditionSO")]
public class NumberOfEnemyUnitsConditionSO : EnemyActionConditionSO 
{
    public int m_Threshold;
    [Tooltip("Whether the threshold is met if the number of units falls below or above the threshold")]
    public bool m_LessThan;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_LessThan)
            return mapLogic.GetNumberOfUnitsOnGrid(GridType.ENEMY) < m_Threshold;
        else
            return mapLogic.GetNumberOfUnitsOnGrid(GridType.ENEMY) > m_Threshold;
    }
}
