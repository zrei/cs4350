using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackerColLimitRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/AttackerColLimitRuleSO")]
public class AttackerColLimitRuleSO : AttackerLocationRuleSO
{
    public int[] m_AllowedAttackerCols;

    public override bool IsValidAttackerTile(CoordPair attackerPosition)
    {
        return m_AllowedAttackerCols.Contains(attackerPosition.m_Col);
    }
}
