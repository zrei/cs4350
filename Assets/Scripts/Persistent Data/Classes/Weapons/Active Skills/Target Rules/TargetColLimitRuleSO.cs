using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetColLimitRuleSO", menuName = "ScriptableObject/Classes/ActiveSkills/TargetRules/TargetColLimitRuleSO")]
public class TargetColLimitRuleSO : SkillTargetRuleSO
{
    public int[] m_AllowedTargetCols;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return m_AllowedTargetCols.Contains(targetTile.m_Col);
    }
}
