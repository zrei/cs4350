using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetRowLimitRuleSO", menuName = "ScriptableObject/Classes/ActiveSkills/TargetRules/TargetRowLimitRuleSO")]
public class TargetRowLimitRuleSO : SkillTargetRuleSO
{
    public int[] m_AllowedTargetRows;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return m_AllowedTargetRows.Contains(targetTile.m_Row);
    }
}
