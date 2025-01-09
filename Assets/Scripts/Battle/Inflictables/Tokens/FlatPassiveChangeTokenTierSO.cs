using UnityEngine;

/// <summary>
/// Can be used for decrease or increase health/mana
/// </summary>
[CreateAssetMenu(fileName = "FlatPassiveChangeTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/FlatPassiveChangeTokenTierSO")]
public class FlatPassiveChangeTokenTierSO : PassiveChangeTokenTierSO
{
    public override TokenType TokenType => TokenType.FLAT_PASSIVE_CHANGE;

    public void GetFlatChange(int tier, out float flatHealthChangeAmount, out float flatManaChangeAmount)
    {
        flatHealthChangeAmount = 0f;
        flatManaChangeAmount = 0f;

        if (TryRetrieveTier(tier, out TokenSO tokenSO))
        {
            if (m_ChangeHealth)
            {
                flatHealthChangeAmount = tokenSO.m_Amount;
            }

            if (m_ChangeMana)
            {
                flatManaChangeAmount = tokenSO.m_Amount;
            }
        }
        
        return;
    }
}
