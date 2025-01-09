using System.Collections.Generic;
using UnityEngine;

public abstract class StatChangeTokenTierSO : TokenTierSO
{
    public override bool m_ResetConditionMet => true;

    [Header("Stat")]
    public List<StatType> m_AffectedStats;
}
