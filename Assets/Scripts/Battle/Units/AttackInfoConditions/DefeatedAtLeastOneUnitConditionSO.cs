using UnityEngine;

[CreateAssetMenu(fileName = "DefeatedAtLeastNUnitsConditionSO", menuName = "ScriptableObject/Battle/AttackInfoConditions/DefeatedAtLeastNUnitsConditionSO")]
public class DefeatedAtLeastNUnitsConditionSO : AttackInfoConditionSO
{
    public int m_NumberOfUnitsToDefeat = 1;

    public override bool IsConditionMet(AttackInfo attackInfo)
    {
        int numUnitsDefeated = 0;

        foreach (Unit unit in attackInfo.m_Targets)
        {
            if (unit.IsDead)
                ++numUnitsDefeated;
        }

        return numUnitsDefeated >= m_NumberOfUnitsToDefeat;
    }
}
