using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/StatusEffectTokenTierSO")]
public class StatusEffectTokenTierSO : TargetOtherTilesTokenTierSO 
{
    public override TokenType TokenType => TokenType.INFLICT_STATUS;

    [Header("Status effect")]
    public StatusEffectSO m_StatusEffect;

    public StatusEffect GetInflictedStatusEffect(int tier)
    {
        if (TryRetrieveTier(tier, out TokenSO tokenSO))
        {
            return new StatusEffect(m_StatusEffect, (int) tokenSO.m_Amount);
        }
        return default;
    }
}
