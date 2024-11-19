using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SkillsOverviewDisplay : MonoBehaviour
    {

        #region Component References
        private CanvasGroup canvasGroup;
        
        [SerializeField]
        private List<SkillButton> activeSkillButtons = new();
        
        [SerializeField]
        private List<ActionButton> passiveSkillButtons = new();

        [SerializeField] 
        private FormattedTextDisplay skillHeaderText;

        [SerializeField] 
        private FormattedTextDisplay skillDescriptionText;

        [SerializeField]
        private Image m_AoESprite;

        [SerializeField]
        private Image m_SelfPositioningSprite;

        [SerializeField]
        private Image m_TargetPositioningSprite;
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

        private ICanAttack m_PlayerUnit;

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

        public void DisplayUnitSkills(Unit unit)
        {
            DisplayUnitSkills(unit, unit.GetActiveSkills());
        }

        public void DisplayUnitSkills(PlayerCharacterData playerUnit)
        {
            DisplayUnitSkills(playerUnit, playerUnit.CurrClass.m_ActiveSkills);
        }

        public void DisplayUnitSkills(ICanAttack unit, IEnumerable<ActiveSkillSO> skills)
        {
            // Reset Display
            LockedInSkill = null;
            UpdateSkillDisplay(null);

            m_PlayerUnit = unit;
            activeSkills = skills.ToList();

            for (int i = 0; i < activeSkillButtons.Count; i++)
            {
                if (i < activeSkills.Count)
                {
                    var skill = activeSkills[i];

                    var button = activeSkillButtons[i];
                    button.gameObject.SetActive(true);
                    button.SkillSprite = skill.m_Icon;

                    if (unit is Unit)
                    {
                        Unit castUnit = (Unit) unit;
                        if (!castUnit.HasEnoughManaForSkill(skill))
                        {
                            button.SetFill(0f);
                            button.statusText?.SetValue(skill.m_ConsumedMana, "<sprite name=\"Mana\" tint>");
                            button.SetState(SkillButtonState.LOCKED);
                        }
                        else
                        {
                            button.SetFill(castUnit.GetSkillCooldownProportion(skill));
                            int cooldown = castUnit.GetSkillCooldown(skill);
                            button.statusText?.SetValue("<sprite name=\"Turn\" tint>", cooldown);
                            button.SetState(cooldown > 0 ? SkillButtonState.LOCKED : SkillButtonState.NORMAL);
                        }
                    }
                    else
                    {
                        button.SetFill(1f);
                        button.SetState(SkillButtonState.NORMAL);
                    }
                }
                else
                {
                    activeSkillButtons[i].gameObject.SetActive(false);
                }
            }

            // handle passive skills elsewhere
            //var passiveSkills = playerUnit.CurrClass.m_PassiveEffects;

            //for (int i = 0; i < passiveSkillButtons.Count; i++)
            //{
            //    passiveSkillButtons[i].gameObject.SetActive(i < passiveSkills.Count);
            //}

            m_AoESprite.gameObject.SetActive(false);
            m_SelfPositioningSprite.gameObject.SetActive(false);
            m_TargetPositioningSprite.gameObject.SetActive(false);
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

            m_AoESprite.sprite = skill.m_TargetSO.m_TargetRepSprite;
            m_SelfPositioningSprite.sprite = skill.GetCasterPositioningSprite;
            var targetPosDisplayInfo = skill.GetTargetPositioningSprite;
            m_TargetPositioningSprite.sprite = targetPosDisplayInfo.sprite;
            m_TargetPositioningSprite.gameObject.transform.localEulerAngles = targetPosDisplayInfo.isAlly
                ? new(0, 0, 180)
                : Vector3.zero;
            m_TargetPositioningSprite.color = targetPosDisplayInfo.isAlly
                ? ColorUtils.AllyColor
                : ColorUtils.EnemyColor;

            m_AoESprite.gameObject.SetActive(m_AoESprite.sprite != null);
            m_SelfPositioningSprite.gameObject.SetActive(m_SelfPositioningSprite.sprite != null);
            m_TargetPositioningSprite.gameObject.SetActive(m_TargetPositioningSprite.sprite != null);
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
