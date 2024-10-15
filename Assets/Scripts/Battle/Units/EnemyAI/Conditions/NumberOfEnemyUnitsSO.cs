using UnityEngine;

public class NumberOfEnemyUnitsSO : EnemyActionConditionSO 
{
    public int m_Threshold;
    public bool m_LessThan;

    public override bool IsConidtionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        throw new System.NotImplementedException();
    }
}
