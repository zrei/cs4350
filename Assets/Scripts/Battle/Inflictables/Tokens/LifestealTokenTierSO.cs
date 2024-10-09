using UnityEngine;

[CreateAssetMenu(fileName = "LifestealTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/LifestealTokenTierSO")]
public class LifestealTokenTierSO : TokenTierSO
{
    public override TokenType TokenType => TokenType.LIFESTEAL;

    public float GetLifestealProportion(int tier)
    {
        if (TryRetreiveTier(tier, out TokenSO tokenSO))
        {
            return tokenSO.m_Amount;
        }
        return 0f;
    }
}
