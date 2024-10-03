using Game.Input;
using System.Collections;
using System.Collections.Generic;
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
            get => selectedSkill;
            set
            {
                selectedSkill = value;
                skillHeader.SetValue(selectedSkill.m_SkillName);
                if (selectedSkill.IsPhysicalAttack)
                {
                    skillDescription.text = $"DMG: {DamageCalc.CalculateDamage(currentUnit, selectedSkill):F0} <sprite name=\"PhysicalAttack\">";
                }
                else if (selectedSkill.IsMagicAttack)
                {
                    skillDescription.text = $"DMG: {DamageCalc.CalculateDamage(currentUnit, selectedSkill):F0} <sprite name=\"MagicAttack\">";
                }
                else if (selectedSkill.IsHeal)
                {
                    skillDescription.text = $"HEAL: {selectedSkill.m_HealAmount:F0}";
                }
                else
                {
                    skillDescription.text = selectedSkill.m_Description;
                }
            }
        }
        private ActiveSkillSO selectedSkill;

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
            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;

            BindButtonEvents();
        }
        
        private void OnBattleEnd(UnitAllegiance unitAllegiance)
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;

            Hide();
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
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
            moveButton.onSubmit.RemoveAllListeners();
            moveButton.onSubmit.AddListener(() =>
            {
                BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.SELECTING_MOVEMENT_SQUARE);
            });
            moveButton.onSelect.RemoveAllListeners();
            moveButton.onSelect.AddListener(() =>
            {
                skillHeader.SetValue("Move");
                skillDescription.text = $"<sprite name=\"Steps\">: {currentUnit.Stat.m_MovementRange}";
            });

            inspectButton.onSubmit.RemoveAllListeners();
            inspectButton.onSubmit.AddListener(() =>
            {
                BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.INSPECT);
            });
            inspectButton.onSelect.RemoveAllListeners();
            inspectButton.onSelect.AddListener(() =>
            {
                skillHeader.SetValue("Inspect");
                skillDescription.text = string.Empty;
            });

            passButton.onSubmit.RemoveAllListeners();
            passButton.onSubmit.AddListener(() =>
            {
                BattleManager.Instance.PlayerTurnManager.EndTurn();
            });
            passButton.onSelect.RemoveAllListeners();
            passButton.onSelect.AddListener(() =>
            {
                skillHeader.SetValue("End Turn");
                skillDescription.text = string.Empty;
            });
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
                    SelectedSkill = skill;
                });

                button.onSubmit.RemoveAllListeners();
                button.onSubmit.AddListener(PreviewMove);
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

        private void PreviewMove()
        {
            BattleManager.Instance.PlayerTurnManager.SelectedSkill = SelectedSkill;
            BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.SELECTING_ACTION_TARGET);
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
