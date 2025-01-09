using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Target rules
/// </summary>
public class TargetOtherTilesTokenTierSO : TokenTierSO
{
    [Header("What units to target")]
    [Tooltip("Limits on location of targets")]
    public List<TargetLocationRuleSO> m_TargetLocationRules;
    [Tooltip("Limits on side of targets")]
    public List<TargetSideLimitRuleSO> m_TargetSideRules;
    [Tooltip("Limits that apply directly on targeted units")]
    public List<ActionConditionSO> m_TargetRules;

    [Tooltip("This means that only 'filtered' units, such as those that are being targeted in an attack, or those that are attacking the unit, are considered instead of everyone - should only be used for tokens that will be utilised in these situations!")]
    public bool m_TargetFilteredUnitsOnly = false;
    public virtual bool RequiresTargetedSquares => true;
}
