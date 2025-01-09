using UnityEngine;

/// <summary>
/// Applies flat number of tokens. The tier to apply is taken from the tier of this token
/// </summary>
[CreateAssetMenu(fileName = "ApplyTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/ApplyTokenTierSO")]
public class ApplyTokenTierSO : TargetOtherTilesTokenTierSO
{
    public override TokenType TokenType => TokenType.APPLY_TOKEN;

    [Header("Token")]
    public TokenTierSO m_TokenToApply;
    public int m_NumberToApply = 1;
    public bool m_IsPermanent = false;

    public InflictedToken GetInflictedToken(int tier)
    {
        if (TryRetrieveTier(tier, out TokenSO tokenSO))
        {
            return new InflictedToken {m_TokenTierData = m_TokenToApply, m_Tier = (int) tokenSO.m_Amount, m_Number = m_NumberToApply, m_IsPermanent = m_IsPermanent};
        }
        return default;
    }
}
