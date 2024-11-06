using UnityEngine;

namespace Game.UI
{
    public class CharacterStatDisplay : MonoBehaviour
    {
        [SerializeField] private FormattedTextDisplay m_HpDisplay;

        [SerializeField] private FormattedTextDisplay m_MpDisplay;

        [SerializeField] private FormattedTextDisplay m_PhyAtkDisplay;

        [SerializeField] private FormattedTextDisplay m_MgcAtkDisplay;

        [SerializeField] private FormattedTextDisplay m_PhyDefDisplay;

        [SerializeField] private FormattedTextDisplay m_MgcDefDisplay;

        [SerializeField] private FormattedTextDisplay m_SpdDisplay;

        [SerializeField] private FormattedTextDisplay m_MoveDisplay;

        private PlayerCharacterData m_CurrChara;

        public void SetDisplay(PlayerCharacterData playerCharacterData)
        {
            Stats currStats = playerCharacterData.TotalBaseStats;
            m_CurrChara = playerCharacterData;
            m_HpDisplay?.SetValue(currStats.m_Health);
            m_MpDisplay?.SetValue(currStats.m_Mana);
            m_PhyAtkDisplay?.SetValue(currStats.m_PhysicalAttack);
            m_MgcAtkDisplay?.SetValue(currStats.m_MagicAttack);
            m_PhyDefDisplay?.SetValue(currStats.m_PhysicalDefence);
            m_MgcDefDisplay?.SetValue(currStats.m_MagicDefence);
            m_SpdDisplay?.SetValue(currStats.m_Speed);
            m_MoveDisplay?.SetValue(currStats.m_MovementRange);
        }

        public void SetComparisonDisplay(PlayerClassSO comparedClass)
        {
            Stats newStats = m_CurrChara.m_CurrStats.FlatAugment(comparedClass.m_StatAugments);
            Stats currStats = m_CurrChara.TotalBaseStats;

            SetComparisonStat(m_HpDisplay, currStats.m_Health, newStats.m_Health);
            SetComparisonStat(m_MpDisplay, currStats.m_Mana, newStats.m_Mana);
            SetComparisonStat(m_PhyAtkDisplay, currStats.m_PhysicalAttack, newStats.m_PhysicalAttack);
            SetComparisonStat(m_MgcAtkDisplay, currStats.m_MagicAttack, newStats.m_MagicAttack);
            SetComparisonStat(m_PhyDefDisplay, currStats.m_PhysicalDefence, newStats.m_PhysicalDefence);
            SetComparisonStat(m_MgcDefDisplay, currStats.m_MagicDefence, newStats.m_MagicDefence);
            SetComparisonStat(m_SpdDisplay, currStats.m_Speed, newStats.m_Speed);
            SetComparisonStat(m_MoveDisplay, currStats.m_MovementRange, newStats.m_MovementRange);
        }

        private void SetComparisonStat(FormattedTextDisplay formattedTextDisplay, float currStat, float newStat)
        {
            if (currStat == newStat)
            {
                formattedTextDisplay?.SetValue(newStat);
            }
            else if (currStat > newStat)
            {
                formattedTextDisplay?.SetValue($"{newStat} (<color=red>-{currStat - newStat}</color>)");
            }
            else
            {
                formattedTextDisplay?.SetValue($"{newStat} (<color=blue>+{newStat - currStat}</color>)");
            }
        }
    }
}
