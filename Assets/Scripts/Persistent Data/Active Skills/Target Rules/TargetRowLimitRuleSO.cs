using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetRowLimitRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetRowLimitRuleSO")]
public class TargetRowLimitRuleSO : TargetLocationRuleSO
{
    public int[] m_AllowedTargetRows;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return m_AllowedTargetRows.Contains(targetTile.m_Row);
    }
}
