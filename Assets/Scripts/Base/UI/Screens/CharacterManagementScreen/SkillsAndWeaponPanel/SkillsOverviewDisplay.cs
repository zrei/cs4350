using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UI
{
    public class SkillsOverviewDisplay : MonoBehaviour
    {

        #region Component References
        private CanvasGroup canvasGroup;
        
        [SerializeField]
        private List<ActionButton> activeSkillButtons = new();
        
        [SerializeField]
        private List<ActionButton> passiveSkillButtons = new();

        [SerializeField] 
        private FormattedTextDisplay skillHeaderText;

        [SerializeField] 
        private FormattedTextDisplay skillDescriptionText;
        
        #endregion

        private List<ActiveSkillSO> activeSkills;

        // TODO: Add a way to display passive skills
        public ActiveSkillSO LockedInSkill
        {
            set
            {
                if (lockedInSkill == value) return;

                lockedInSkill = value;
                UpdateSkillDisplay(lockedInSkill);
            }
        }
        private ActiveSkillSO lockedInSkill;

        private PlayerCharacterData m_PlayerUnit;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Hide();
            
            for (int i = 0; i < activeSkillButtons.Count; i++)
            {
                var button = activeSkillButtons[i];
                var index = i;
                button.onSelect.AddListener(() => OnSelectSkill(index));
            }
        }

        public void DisplayUnitSkills(PlayerCharacterData playerUnit)
        {
            // Reset Display
            LockedInSkill = null;
            UpdateSkillDisplay(null);
            
            m_PlayerUnit = playerUnit;
            activeSkills = playerUnit.CurrClass.m_ActiveSkills.ToList();
            
            for (int i = 0; i < activeSkillButtons.Count; i++)
            {
                if (i < activeSkills.Count)
                {
                    var skill = activeSkills[i];
                        
                    activeSkillButtons[i].gameObject.SetActive(true);
                    activeSkillButtons[i].icon.sprite = skill.m_Icon;
                }
                else
                {
                    activeSkillButtons[i].gameObject.SetActive(false);
                }
            }

            var passiveSkills = playerUnit.CurrClass.m_PassiveEffects;
            
            for (int i = 0; i < passiveSkillButtons.Count; i++)
            {
                passiveSkillButtons[i].gameObject.SetActive(i < passiveSkills.Count);
            }
        }

        private void OnSelectSkill(int index)
        {
            LockedInSkill = activeSkills[index];
        }

        private void UpdateSkillDisplay(ActiveSkillSO skill)
        {
            if (skill == null)
            {
                skillHeaderText.SetValue(string.Empty);
                skillDescriptionText.SetValue(string.Empty);
                return;
            }
            
            skillHeaderText.SetValue(skill.m_SkillName);
            skillDescriptionText.SetValue(skill.GetDescription(m_PlayerUnit, null));
        }

        public void Hide()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
        }
        
        public void Show()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1;
        }
    }
}
