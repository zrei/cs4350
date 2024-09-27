using UnityEngine;

public class LevellingManager : MonoBehaviour
{
    [SerializeField] LevellingSO m_LevellingSO;

    public void LevelCharacter(CharacterData characterData, int expGained, out bool hasLevelledUp)
    {
        hasLevelledUp = false;

        if (characterData.m_CurrLevel == LevellingSO.MAX_LEVEL)
        {
            return;
        }

        int finalExp = Mathf.Min(characterData.m_CurrExp + expGained, m_LevellingSO.GetRequiredExpAmount(LevellingSO.MAX_LEVEL));
        
        while (characterData.m_CurrLevel < LevellingSO.MAX_LEVEL)
        {
            if (finalExp >= m_LevellingSO.GetRequiredExpAmount(characterData.m_CurrLevel + 1))
            {
                hasLevelledUp = true;
                characterData.m_CurrLevel += 1;
                characterData.m_CurrStats = LevelUpStats(characterData.m_CurrStats, characterData.GrowthRate);
            }
            else
            {
                break;
            }
        }
        
    }

    public Stats LevelUpStats(Stats previousStats, Stats growthRate)
    {
        return new Stats(previousStats.m_name, previousStats.m_class, previousStats.m_Health + growthRate.m_Health, previousStats.m_Mana + growthRate.m_Mana, previousStats.m_PhysicalAttack + growthRate.m_PhysicalAttack, previousStats.m_MagicAttack + growthRate.m_MagicAttack, previousStats.m_PhysicalDefence + growthRate.m_PhysicalDefence, previousStats.m_MagicDefence + growthRate.m_MagicDefence, previousStats.m_Speed + growthRate.m_Speed, previousStats.m_MovementRange);
    }
}
