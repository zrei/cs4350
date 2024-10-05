using UnityEngine;

[CreateAssetMenu(fileName = "CritTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/CritTokenTierSO")]
public class CritTokenTierSO : TokenTierSO
{
    public override TokenType TokenType => TokenType.CRIT;

    public float GetFinalDamageModifier(int tier)
    {
        if (TryRetreiveTier(tier, out TokenSO tokenSO))
        {
            return tokenSO.m_Amount;
        }
        return 1f;
    }
}
