using UnityEngine;

/// <summary>
/// Can be used for decrease or increase health/mana
/// </summary>
[CreateAssetMenu(fileName = "MultPassiveChangeTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/MultPassiveChangeTokenTierSO")]
public class MultPassiveChangeTokenTierSO : PassiveChangeTokenTierSO
{
    public override TokenType TokenType => TokenType.MULT_PASSIVE_CHANGE;

    public void GetMultChange(int tier, out float multHealthChangeAmount, out float multManaChangeAmount)
    {
        multHealthChangeAmount = 0f;
        multManaChangeAmount = 0f;

        if (TryRetrieveTier(tier, out TokenSO tokenSO))
        {
            if (m_ChangeHealth)
            {
                multHealthChangeAmount = tokenSO.m_Amount;
            }

            if (m_ChangeMana)
            {
                multManaChangeAmount = tokenSO.m_Amount;
            }
        }
        
        return;
    }
}
