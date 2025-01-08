using UnityEngine;

[CreateAssetMenu(fileName = "LifestealTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/LifestealTokenTierSO")]
public class LifestealTokenTierSO : TokenTierSO
{
    public override TokenType TokenType => TokenType.LIFESTEAL;
    public override bool m_ResetConditionMet => true;

    public float GetLifestealProportion(int tier)
    {
        if (TryRetrieveTier(tier, out TokenSO tokenSO))
        {
            return tokenSO.m_Amount;
        }
        return 0f;
    }
}
