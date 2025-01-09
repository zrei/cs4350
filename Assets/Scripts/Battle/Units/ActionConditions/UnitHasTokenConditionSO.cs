using UnityEngine;

[CreateAssetMenu(fileName = "UnitHasTokenConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/UnitHasTokenConditionSO")]
public class UnitHasTokenConditionSO : ActionConditionSO 
{
    public TokenType m_TokenType;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ unit.HasToken(m_TokenType);
    }
}
