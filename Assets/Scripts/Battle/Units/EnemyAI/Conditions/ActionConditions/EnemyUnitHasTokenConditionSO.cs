using UnityEngine;

[CreateAssetMenu(fileName = "EnemyUnitHasTokenConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/ActionConditions/EnemyUnitHasTokenConditionSO")]
public class EnemyUnitHasTokenConditionSO : EnemyActionConditionSO 
{
    public TokenType m_TokenType;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return m_IsInverted ^ enemyUnit.HasToken(m_TokenType);
    }
}
