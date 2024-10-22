using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CharacterOverviewDisplay : MonoBehaviour
    {

        #region Component References
        [SerializeField]
        private FormattedTextDisplay pathDisplay;
        
        [SerializeField]
        private FormattedTextDisplay nameDisplay;
        
        [SerializeField]
        private FormattedTextDisplay levelDisplay;

        [SerializeField]
        private Image characterArt;
        
        [SerializeField]
        private FormattedTextDisplay hpDisplay;

        [SerializeField]
        private FormattedTextDisplay mpDisplay;

        [SerializeField]
        private FormattedTextDisplay phyAtkDisplay;

        [SerializeField]
        private FormattedTextDisplay mgcAtkDisplay;

        [SerializeField]
        private FormattedTextDisplay phyDefDisplay;

        [SerializeField]
        private FormattedTextDisplay mgcDefDisplay;

        [SerializeField]
        private FormattedTextDisplay spdDisplay;
        
        [SerializeField]
        private FormattedTextDisplay moveDisplay;
        
        [SerializeField]
        private FormattedTextDisplay classDisplay;
        
        [SerializeField]
        private NamedObjectButton skillsTabButton;
        
        [SerializeField]
        private NamedObjectButton weaponsTabButton;
        
        [SerializeField]
        private SkillsOverviewDisplay skillsOverviewDisplay;
        
        [SerializeField]
        private WeaponsOverviewDisplay weaponsOverviewDisplay;
        #endregion

        private void Awake()
        {
            skillsTabButton.onSubmit.AddListener(OnSkillsTabSubmit);
            weaponsTabButton.onSubmit.AddListener(OnWeaponsTabSubmit);
        }

        public void ViewUnit(PlayerCharacterData playerUnit)
        {
            pathDisplay?.SetValue($"{playerUnit.m_BaseData.m_PathGroup.m_PathName}");
            nameDisplay?.SetValue($"{playerUnit.m_BaseData.m_CharacterName}");
            levelDisplay?.SetValue($"{playerUnit.m_CurrLevel}");

            characterArt.sprite = playerUnit.m_BaseData.m_CharacterSprite;
            var color = characterArt.color;
            color.a = playerUnit.m_BaseData.m_CharacterSprite != null ? 1 : 0;
            characterArt.color = color;

            var currStats = playerUnit.m_CurrStats;
            hpDisplay?.SetValue(currStats.m_Health);
            mpDisplay?.SetValue(currStats.m_Mana);
            phyAtkDisplay?.SetValue(currStats.m_PhysicalAttack);
            mgcAtkDisplay?.SetValue(currStats.m_MagicAttack);
            phyDefDisplay?.SetValue(currStats.m_PhysicalDefence);
            mgcDefDisplay?.SetValue(currStats.m_MagicDefence);
            spdDisplay?.SetValue(currStats.m_Speed);
            moveDisplay?.SetValue(currStats.m_MovementRange);
            
            classDisplay?.SetValue($"{playerUnit.CurrClass.m_ClassName}");
            
            skillsOverviewDisplay.DisplayUnitSkills(playerUnit);
            weaponsOverviewDisplay.DisplayUnitWeapons(playerUnit);

            OnSkillsTabSubmit();
        }

        private void OnSkillsTabSubmit()
        {
            skillsOverviewDisplay.Show();
            weaponsOverviewDisplay.Hide();
            skillsTabButton.SetGlowActive(true);
            weaponsTabButton.SetGlowActive(false);
        }
        
        private void OnWeaponsTabSubmit()
        {
            skillsOverviewDisplay.Hide();
            weaponsOverviewDisplay.Show();
            skillsTabButton.SetGlowActive(false);
            weaponsTabButton.SetGlowActive(true);
        }
    }
}
