using UnityEngine;

[CreateAssetMenu(fileName = "HasTokenConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/HasTokenConditionSO")]
public class HasTokenConditionSO : EnemyActionConditionSO 
{
    public TokenType m_TokenType;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return enemyUnit.HasToken(m_TokenType);
    }
}
