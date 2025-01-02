using UnityEngine;

[CreateAssetMenu(fileName = "MeetsIntervalConditionSO", menuName = "ScriptableObject/Battle/ActionConditions/MeetsIntervalConditionSO")]
public class MeetsIntervalConditionSO : ActionConditionSO
{
    public int m_TurnInterval;

    public override bool IsConditionMet(Unit unit, MapLogic mapLogic)
    {
        return unit.CurrTurnCount % m_TurnInterval == 0;
    }
}
