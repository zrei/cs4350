using UnityEngine;

[CreateAssetMenu(fileName = "MapHasUnitsTokenConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/MapHasUnitsTokenConditionSO")]
public class MapHasUnitsTokenConditionSO : ActionConditionSO
{
    public GridType m_GridType;
    public TokenType m_TokenType;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return m_IsInverted ^ mapLogic.HasAnyUnitWithToken(m_GridType, m_TokenType);
    }
}
