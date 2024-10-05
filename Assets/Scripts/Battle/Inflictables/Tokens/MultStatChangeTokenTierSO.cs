using UnityEngine;

[CreateAssetMenu(fileName = "MultStatChangeTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/MultStatChangeTokenTierSO")]
public class MultStatChangeTokenTierSO : StatChangeTokenTierSO
{
    public override TokenType TokenType => TokenType.MULT_STAT_CHANGE;

    public float GetMultStatChange(int tier)
    {
        if (TryRetreiveTier(tier, out TokenSO tokenSO))
        {
            return tokenSO.m_Amount;
        }
        return 1f;
    }
}
