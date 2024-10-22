using System.Collections.Generic;
using UnityEngine;

public abstract class StatChangeTokenTierSO : TokenTierSO
{
    [Header("Stat")]
    public List<StatType> m_AffectedStats;
}
