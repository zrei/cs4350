using UnityEngine;

[CreateAssetMenu(fileName = "FlatStatChangeTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/FlatStatChangeTokenTierSO")]
public class FlatStatChangeTokenTierSO : StatChangeTokenTierSO
{
    public override TokenType TokenType => TokenType.FLAT_STAT_CHANGE;

    public float GetFlatStatChange(int tier)
    {
        if (TryRetrieveTier(tier, out TokenSO tokenSO))
        {
            return tokenSO.m_Amount;
        }
        return 0f;
    }
}
