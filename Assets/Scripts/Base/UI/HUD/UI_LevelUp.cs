using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Temporary UI for character level up,
// to be integrated with UI_Manager System
public class UI_LevelUp : MonoBehaviour
{
    [FormerlySerializedAs("m_RewardPanel")] [SerializeField] GameObject m_LevelUpPanel;
    [SerializeField] TextMeshProUGUI m_CharacterNameText;
    [SerializeField] TextMeshProUGUI m_ResultText;
    [SerializeField] Button m_ReturnButton;
    
    // Temporary List of level up summaries
    private List<LevelUpSummary> m_LevelUpSummaries;

    private void Awake()
    {
        m_ReturnButton.onClick.AddListener(CloseResults);
        m_LevelUpPanel.SetActive(false);
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
        
        m_ResultText.text = $"Level: \t\t\t{levelUpSummary.m_FinalLevel} (+{levelUpSummary.m_LevelGrowth})\n";
        
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
                    m_ResultText.text += $"{stat}: \t\t\t";
                    break;
                case StatType.PHYS_ATTACK:
                case StatType.MAG_ATTACK:
                    m_ResultText.text += $"{stat}: \t\t";
                    break;
                case StatType.PHYS_DEFENCE:
                case StatType.MAG_DEFENCE:
                    m_ResultText.text += $"{stat}: \t";
                    break;
            }
            
            m_ResultText.text += (statGrowth == 0)
                ? $"{finalStat}\n"
                : $"{finalStat} (+{statGrowth})\n";
        }
        
        m_LevelUpPanel.SetActive(true);
    }
    
    private void CloseResults()
    {
        if (m_LevelUpSummaries.Count > 0)
        {
            DisplayLevelUp();
            return;
        }
        
        m_LevelUpPanel.SetActive(false);
        GlobalEvents.Level.CloseLevellingScreenEvent?.Invoke();
    }
}