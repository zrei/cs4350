using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackerColLimitRuleSO", menuName = "ScriptableObject/Classes/ActiveSkills/TargetRules/AttackerColLimitRuleSO")]
public class AttackerColLimitRuleSO : SkillTargetRuleSO
{
    public int[] m_AllowedAttackerCols;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return m_AllowedAttackerCols.Contains(attackingUnit.CurrPosition.m_Col);
    }
}