using System.Collections.Generic;
using Game.Input;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class LevelUpResultScreen : BaseUIScreen
    {
        [SerializeField] TextMeshProUGUI m_CharacterNameText;
        [SerializeField] LevelUpStatDisplay m_LevelUpStatDisplay;
        [SerializeField] SelectableBase m_ReturnButton;
        
        // Temporary List of level up summaries
        private List<LevelUpSummary> m_LevelUpSummaries;
        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            ShowMassLevelling((List<LevelUpSummary>) args[0]);
            base.Show();
        }

        private void ShowMassLevelling(List<LevelUpSummary> levelUpSummaries)
        {
            m_LevelUpSummaries = levelUpSummaries;
        
            DisplayLevelUp();
            
            m_ReturnButton.onSubmit.AddListener(CloseResults);
        }
        
        private void DisplayLevelUp()
        {
            m_ReturnButton.interactable = false;

            if (m_LevelUpSummaries.Count == 0)
            {
                CloseResults();
                return;
            }
            
            var levelUpSummary = m_LevelUpSummaries[0];
            m_CharacterNameText.text = $"{levelUpSummary.m_CharacterSO.m_CharacterName}";
            m_LevelUpStatDisplay.DisplayLevelUp(levelUpSummary, CompleteLevelUpAnimation);
            m_LevelUpSummaries.RemoveAt(0);
            
            void CompleteLevelUpAnimation()
            {
                m_ReturnButton.interactable = true;
            }
        }

        private void CloseResults()
        {
            if (m_LevelUpSummaries.Count > 0)
            {
                DisplayLevelUp();
                return;
            }
        
            UIScreenManager.Instance.CloseScreen();
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
