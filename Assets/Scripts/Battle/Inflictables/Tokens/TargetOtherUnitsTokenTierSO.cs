using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Target rules
/// </summary>
public class TargetOtherUnitsTokenTierSO : TokenTierSO
{
    [Header("What units to target")]
    [Tooltip("Limits on location of targets - includes side")]
    public List<SkillTargetRuleSO> m_TargetLocationRules;
    [Tooltip("Limits on units to target")]
    public List<ActionConditionSO> m_TargetRules;
}
