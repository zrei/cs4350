using Game.Input;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ActionMenu : MonoBehaviour
    {
        #region Component References
        [SerializeField]
        private FormattedTextDisplay skillHeader;

        [SerializeField]
        private TextMeshProUGUI skillDescription;

        [SerializeField]
        private ActionButton leftScrollButton;

        [SerializeField]
        private List<ActionButton> attackButtons = new();

        [SerializeField]
        private ActionButton rightScrollButton;

        [SerializeField]
        private ActionButton moveButton;

        [SerializeField]
        private ActionButton inspectButton;

        [SerializeField]
        private ActionButton passButton;
        #endregion

        private Unit currentUnit;
        private ActiveSkillSO SelectedSkill
        {
            set
            {
                if (selectedSkill == value) return;

                selectedSkill = value;
                UpdateSkillDisplay(selectedSkill);
            }
        }
        private ActiveSkillSO selectedSkill;
        private ActionButton SelectedActionButton
        {
            set
            {
                if (selectedActionButton == value) return;

                if (selectedActionButton != null)
                {
                    selectedActionButton.glow.CrossFadeAlpha(0, 0.2f, false);
                }
                selectedActionButton = value;
                if (selectedActionButton != null)
                {
                    selectedActionButton.glow.CrossFadeAlpha(1f, 0.2f, false);
                }
            }
        }
        private ActionButton selectedActionButton;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            isHidden = true;

            GlobalEvents.Battle.PreviewCurrentUnitEvent += OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent += OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;

            BindButtonEvents();
        }
        
        private void OnBattleEnd(UnitAllegiance _, int _2)
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;

            Hide();
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        }

        private void BindInputEvents(bool active)
        {
            if (active)
            {
                InputManager.Instance.Action1Input.OnPressEvent += OnAction1;
                InputManager.Instance.Action2Input.OnPressEvent += OnAction2;
                InputManager.Instance.Action3Input.OnPressEvent += OnAction3;
                InputManager.Instance.Action4Input.OnPressEvent += OnAction4;
                InputManager.Instance.Action5Input.OnPressEvent += OnAction5;
                InputManager.Instance.Action6Input.OnPressEvent += OnAction6;
                InputManager.Instance.Action7Input.OnPressEvent += OnAction7;
            }
            else
            {
                InputManager.Instance.Action1Input.OnPressEvent -= OnAction1;
                InputManager.Instance.Action2Input.OnPressEvent -= OnAction2;
                InputManager.Instance.Action3Input.OnPressEvent -= OnAction3;
                InputManager.Instance.Action4Input.OnPressEvent -= OnAction4;
                InputManager.Instance.Action5Input.OnPressEvent -= OnAction5;
                InputManager.Instance.Action6Input.OnPressEvent -= OnAction6;
                InputManager.Instance.Action7Input.OnPressEvent -= OnAction7;
            }
        }

        private void OnAction1(IInput input) { attackButtons[0].Select(); attackButtons[0].OnSubmit(null); }
        private void OnAction2(IInput input) { attackButtons[1].Select(); attackButtons[1].OnSubmit(null); }
        private void OnAction3(IInput input) { attackButtons[2].Select(); attackButtons[2].OnSubmit(null); }
        private void OnAction4(IInput input) { attackButtons[3].Select(); attackButtons[3].OnSubmit(null); }
        private void OnAction5(IInput input) { moveButton.Select(); moveButton.OnSubmit(null); }
        private void OnAction6(IInput input) { inspectButton.Select(); inspectButton.OnSubmit(null); }
        private void OnAction7(IInput input) { passButton.Select(); passButton.OnSubmit(null); }

        private void BindButtonEvents()
        {
            moveButton.onSelect.RemoveAllListeners();
            moveButton.onSelect.AddListener(() =>
            {
                if (selectedActionButton == null)
                {
                    SelectedSkill = null;
                    skillHeader.SetValue("Move");
                    skillDescription.gameObject.SetActive(true);
                    skillDescription.text = $"<sprite name=\"Steps\">: {currentUnit.GetTotalStat(StatType.MOVEMENT_RANGE)}";
                }
            });
            moveButton.onSubmit.RemoveAllListeners();
            moveButton.onSubmit.AddListener(() =>
            {
                SelectedSkill = null;
                skillHeader.SetValue("Move");
                skillDescription.gameObject.SetActive(true);
                skillDescription.text = $"<sprite name=\"Steps\">: {currentUnit.GetTotalStat(StatType.MOVEMENT_RANGE)}";

                BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.SELECTING_MOVEMENT_SQUARE);
                SelectedActionButton = moveButton;
            });

            inspectButton.onSelect.RemoveAllListeners();
            inspectButton.onSelect.AddListener(() =>
            {
                if (selectedActionButton == null)
                {
                    SelectedSkill = null;
                    skillHeader.SetValue("Inspect");
                    skillDescription.gameObject.SetActive(false);
                }
            });
            inspectButton.onSubmit.RemoveAllListeners();
            inspectButton.onSubmit.AddListener(() =>
            {
                SelectedSkill = null;
                skillHeader.SetValue("Inspect");
                skillDescription.gameObject.SetActive(false);

                BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.INSPECT);
                SelectedActionButton = inspectButton;
            });

            passButton.onSelect.RemoveAllListeners();
            passButton.onSelect.AddListener(() =>
            {
                if (selectedActionButton == null)
                {
                    SelectedSkill = null;
                    skillHeader.SetValue("End Turn");
                    skillDescription.gameObject.SetActive(false);
                }
            });
            passButton.onSubmit.RemoveAllListeners();
            passButton.onSubmit.AddListener(() =>
            {
                SelectedSkill = null;
                skillHeader.SetValue("End Turn");
                skillDescription.gameObject.SetActive(false);

                BattleManager.Instance.PlayerTurnManager.EndTurn();
            });
        }

        private void OnPreviewUnit(Unit unit)
        {
            UpdateSkillDisplay(selectedSkill, unit);
        }

        private void OnPreviewCurrentUnit(Unit currentUnit)
        {
            if (currentUnit is not PlayerUnit playerUnit)
            {
                if (!isHidden) Hide();
                return;
            }

            if (isHidden) Show();

            this.currentUnit = currentUnit;
            var skills = playerUnit.GetAvailableActiveSkills();
            var hasMultiPage = false;
            int i = 0;
            for (; i < skills.Count; i++)
            {
                if (i > attackButtons.Count)
                {
                    hasMultiPage = true;
                    break;
                }
                var button = attackButtons[i];
                var skill = skills[i];
                button.gameObject.SetActive(true);

                button.icon.sprite = skill.m_Icon;

                button.onSelect.RemoveAllListeners();
                button.onSelect.AddListener(() =>
                {
                    if (selectedActionButton == null)
                    {
                        UpdateSkillDisplay(skill);
                    }
                });

                button.onSubmit.RemoveAllListeners();
                button.onSubmit.AddListener(() =>
                {
                    BattleManager.Instance.PlayerTurnManager.SelectedSkill = skill;
                    BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.SELECTING_ACTION_TARGET);
                    SelectedSkill = skill;
                    SelectedActionButton = button;
                });
            }

            if (!hasMultiPage)
            {
                for (; i < attackButtons.Count; i++)
                {
                    var button = attackButtons[i];
                    button.gameObject.SetActive(false);
                }
            }

            leftScrollButton.gameObject.SetActive(hasMultiPage);
            rightScrollButton.gameObject.SetActive(hasMultiPage);

            if (canvasGroup.interactable)
            {
                inspectButton.Select();
            }
        }

        private void UpdateSkillDisplay(ActiveSkillSO skill, IHealth target = null)
        {
            if (skill == null) return;

            skillHeader.SetValue(skill.m_SkillName);
            var builder = new StringBuilder();
            if (skill.IsPhysicalAttack)
            {
                if (target != null)
                {
                    builder.AppendLine($"DMG: {DamageCalc.CalculateDamage(currentUnit, target, skill):G5} <sprite name=\"PhysicalAttack\">");
                }
                else
                {
                    builder.AppendLine($"DMG: {DamageCalc.CalculateDamage(currentUnit, skill):G5} <sprite name=\"PhysicalAttack\">");
                }
            }
            if (skill.IsMagicAttack)
            {
                if (target != null)
                {
                    builder.AppendLine($"DMG: {DamageCalc.CalculateDamage(currentUnit, target, skill):G5} <sprite name=\"MagicAttack\">");
                }
                else
                {
                    builder.AppendLine($"DMG: {DamageCalc.CalculateDamage(currentUnit, skill):G5} <sprite name=\"MagicAttack\">");
                }
            }
            if (skill.IsHeal)
            {
                builder.AppendLine($"HEAL: {skill.m_HealAmount:G5}");
            }
            if (!string.IsNullOrEmpty(skill.m_Description))
            {
                builder.AppendLine(skill.m_Description);
            }

            var descriptionText = builder.ToString();
            if (string.IsNullOrEmpty(descriptionText))
            {
                skillDescription.gameObject.SetActive(false);
            }
            else
            {
                skillDescription.gameObject.SetActive(true);
                skillDescription.text = descriptionText;
            }
        }

        private void Show()
        {
            isHidden = false;
            animator.enabled = true;
            animator.Play(UIConstants.ShowAnimHash);
        }

        private void Hide()
        {
            isHidden = true;
            animator.enabled = true;
            animator.Play(UIConstants.HideAnimHash);

            SelectedActionButton = null;
            SelectedSkill = null;
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
            canvasGroup.interactable = !isHidden;
            canvasGroup.blocksRaycasts = !isHidden;

            if (!isHidden)
            {
                inspectButton.Select();
            }
            BindInputEvents(!isHidden);
        }
    }
}
