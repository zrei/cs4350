using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only supports enemy side summons for now, with tiers indicating which element in the list to return. No need to fill in tiers.
/// </summary>
[CreateAssetMenu(fileName = "SummonUnitsTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/SummonUnitsTokenTierSO")]
public class SummonUnitsTokenTierSO : TokenTierSO
{
    public override TokenType TokenType => TokenType.SUMMON;
    public override int NumTiers => m_Summons.Count;

    [Header("Summons")]
    [Tooltip("Each entry in this list is for one tier")]
    public List<TieredSummons> m_Summons;

    public List<SummonWrapper> GetSummonWrappers(int tier)
    {
        if (tier <= NumTiers)
        {
            return m_Summons[tier - 1].m_Summons;
        }
        return default;
    }
}

[System.Serializable]
public struct TieredSummons
{
    public List<SummonWrapper> m_Summons;
}
