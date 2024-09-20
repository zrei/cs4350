using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackerRowLimitRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/AttackerRowLimitRuleSO")]
public class AttackerRowLimitRuleSO : SkillTargetRuleSO
{
    public int[] m_AllowedAttackerRows;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return m_AllowedAttackerRows.Contains(attackingUnit.CurrPosition.m_Row);
    }
}
