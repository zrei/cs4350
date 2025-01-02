using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only supports enemy side summons for now
/// </summary>
[CreateAssetMenu(fileName = "SummonUnitsTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/SummonUnitsTokenTierSO")]
public class SummonUnitsTokenTierSO : StatChangeTokenTierSO
{
    public override TokenType TokenType => TokenType.MULT_PASSIVE_CHANGE;

    public List<SummonWrapper> m_Summons;
}
