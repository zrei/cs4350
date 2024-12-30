using UnityEngine;

[CreateAssetMenu(fileName = "MapHasUnitsTokenConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/MapHasUnitsTokenConditionSO")]
public class MapHasUnitsTokenConditionSO : EnemyActionConditionSO
{
    public GridType m_GridType;
    public TokenType m_TokenType;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_IsInverted ^ mapLogic.HasAnyUnitWithToken(m_GridType, m_TokenType);
    }
}
