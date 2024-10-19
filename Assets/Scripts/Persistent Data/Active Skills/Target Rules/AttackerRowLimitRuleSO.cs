using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackerRowLimitRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/AttackerRowLimitRuleSO")]
public class AttackerRowLimitRuleSO : AttackerLocationRuleSO
{
    public int[] m_AllowedAttackerRows;

    public override bool IsValidAttackerTile(CoordPair attackerPosition)
    {
        return m_AllowedAttackerRows.Contains(attackerPosition.m_Row);
    }
}
