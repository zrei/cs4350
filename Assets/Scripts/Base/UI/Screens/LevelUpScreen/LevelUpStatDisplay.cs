using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class LevelUpStatDisplay : MonoBehaviour
    {
        [SerializeField] private FormattedTextDisplay m_LevelDisplay;
        [SerializeField] private FormattedTextDisplay m_HpDisplay;

        [SerializeField] private FormattedTextDisplay m_MpDisplay;

        [SerializeField] private FormattedTextDisplay m_PhyAtkDisplay;

        [SerializeField] private FormattedTextDisplay m_MgcAtkDisplay;

        [SerializeField] private FormattedTextDisplay m_PhyDefDisplay;

        [SerializeField] private FormattedTextDisplay m_MgcDefDisplay;

        [SerializeField] private FormattedTextDisplay m_SpdDisplay;

        [SerializeField] private FormattedTextDisplay m_MoveDisplay;

        private const float DELAY = 0.4f;

        public void DisplayLevelUp(LevelUpSummary levelUpSummary, VoidEvent completeAnimationEvent = null)
        {
            StopAllCoroutines();
            m_LevelDisplay?.SetValue(levelUpSummary.m_FinalLevel - levelUpSummary.m_LevelGrowth);

            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                GetFormattedTextDisplay(statType)?.SetValue(GetPreviousStatValue(statType, levelUpSummary.m_FinalStats, levelUpSummary.m_TotalStatGrowths));
            }

            StartCoroutine(BeginLevelUpAnimation(levelUpSummary, completeAnimationEvent));
        }

        private IEnumerator BeginLevelUpAnimation(LevelUpSummary levelUpSummary, VoidEvent completeAnimationEvent = null)
        {
            yield return new WaitForSeconds(DELAY);
            m_LevelDisplay?.SetValue($"{levelUpSummary.m_FinalLevel} <color=#C7B258>(+{levelUpSummary.m_LevelGrowth})</color>");
            yield return new WaitForSeconds(DELAY);
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                if (levelUpSummary.m_TotalStatGrowths.ContainsKey(statType) && levelUpSummary.m_TotalStatGrowths[statType] > 0)
                {
                    GetFormattedTextDisplay(statType)?.SetValue($"{levelUpSummary.m_FinalStats.GetStat(statType)} <color=#C7B258>(+{levelUpSummary.m_TotalStatGrowths[statType]})</color>");
                    yield return new WaitForSeconds(DELAY);
                }
            }
            completeAnimationEvent?.Invoke();
        }

        private FormattedTextDisplay GetFormattedTextDisplay(StatType statType)
        {
            return statType switch
            {
                StatType.HEALTH => m_HpDisplay,
                StatType.MANA => m_MpDisplay,
                StatType.PHYS_ATTACK => m_PhyAtkDisplay,
                StatType.MAG_ATTACK => m_MgcAtkDisplay,
                StatType.PHYS_DEFENCE => m_PhyDefDisplay,
                StatType.MAG_DEFENCE => m_MgcDefDisplay,
                StatType.SPEED => m_SpdDisplay,
                StatType.MOVEMENT_RANGE => m_MoveDisplay,
                _ => default
            };
        }

        private float GetPreviousStatValue(StatType statType, Stats finalStats, Dictionary<StatType, int> totalStatGrowth)
        {
            return finalStats.GetStat(statType) - totalStatGrowth.GetValueOrDefault(statType, 0);
        }
    }
}
