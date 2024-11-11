using System;
using System.Collections.Generic;
using Game.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LevelUpResultScreen : BaseUIScreen
    {
        [SerializeField] TextMeshProUGUI m_CharacterNameText;
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] SelectableBase m_ReturnButton;
        
        // Temporary List of level up summaries
        private List<LevelUpSummary> m_LevelUpSummaries;

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.Level.MassLevellingEvent += OnMassLevelling;
        }

        private void OnDestroy()
        {
            GlobalEvents.Level.MassLevellingEvent -= OnMassLevelling;
        }

        private void OnMassLevelling(List<LevelUpSummary> levelUpSummaries)
        {
            m_LevelUpSummaries = levelUpSummaries;
        
            DisplayLevelUp();
            
            m_ReturnButton.onSubmit.AddListener(CloseResults);
        }
        
        private void DisplayLevelUp()
        {
            if (m_LevelUpSummaries.Count == 0)
            {
                CloseResults();
                return;
            }
            
            var levelUpSummary = m_LevelUpSummaries[0];
            m_LevelUpSummaries.RemoveAt(0);

            m_CharacterNameText.text = $"{levelUpSummary.m_CharacterSO.m_CharacterName}";
            
            m_ResultText.text = $"Level: \t\t\t\t{levelUpSummary.m_FinalLevel} (+{levelUpSummary.m_LevelGrowth})\n";
            
            // Iterate for every stat in StatType
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                // Skip movement range
                if (stat == StatType.MOVEMENT_RANGE)
                    continue;
                
                var statGrowth = levelUpSummary.m_TotalStatGrowths.GetValueOrDefault(stat, 0);
                var finalStat = levelUpSummary.m_FinalStats.GetStat(stat);

                // Hacky way to format the text temporarily
                switch (stat)
                {
                    case StatType.HEALTH:
                    case StatType.MANA:
                    case StatType.SPEED:
                        m_ResultText.text += $"{FormatStatName(stat)}: \t\t\t\t";
                        break;
                    case StatType.PHYS_ATTACK:
                    case StatType.MAG_ATTACK:
                        m_ResultText.text += $"{FormatStatName(stat)}: \t\t\t";
                        break;
                    case StatType.PHYS_DEFENCE:
                    case StatType.MAG_DEFENCE:
                        m_ResultText.text += $"{FormatStatName(stat)}: \t\t";
                        break;
                }
                
                m_ResultText.text += (statGrowth == 0)
                    ? $"{finalStat}\n"
                    : $"{finalStat} (+{statGrowth})\n";
            }
        }

        private string FormatStatName(StatType stat)
        {
            return stat.ToString().Replace("_", " ");
        }
    
        private void CloseResults()
        {
            if (m_LevelUpSummaries.Count > 0)
            {
                DisplayLevelUp();
                return;
            }
        
            UIScreenManager.Instance.CloseScreen();
            GlobalEvents.Level.CloseLevellingScreenEvent?.Invoke();
            m_ReturnButton.onSubmit.RemoveListener(CloseResults);
        }
        
        public override void ScreenUpdate()
        {
        }
        
        public override void OnCancel(IInput input)
        {
            CloseResults();
        }
    }
}
