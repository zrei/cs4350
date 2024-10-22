using Game.Input;
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
        private GameObject actionConfirmKeys;

        [SerializeField]
        private ActionButton leftScrollButton;

        [SerializeField]
        private List<ActionButton> skillButtons = new();

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

        private List<ActiveSkillSO> AvailableSkills
        {
            set
            {
                availableSkills = value;
                var hasMultiPage = SkillPageCount > 1;

                CurrentSkillPageIndex = 0;

                leftScrollButton.gameObject.SetActive(hasMultiPage);
                rightScrollButton.gameObject.SetActive(hasMultiPage);
            }
        }
        private List<ActiveSkillSO> availableSkills = new();

        private int CurrentSkillPageIndex
        {
            get => currentSkillPageIndex;
            set
            {
                var skillPageCount = SkillPageCount;
                value %= skillPageCount;
                if (value < 0) value += skillPageCount;

                currentSkillPageIndex = value;
                var startIndex = currentSkillPageIndex * skillButtons.Count;
                for (int i = 0; i < skillButtons.Count; i++)
                {
                    var button = skillButtons[i];

                    if (startIndex + i < availableSkills.Count)
                    {
                        var skill = availableSkills[startIndex + i];
                        
                        button.gameObject.SetActive(true);
                        button.icon.sprite = skill.m_Icon;
                    }
                    else
                    {
                        button.gameObject.SetActive(false);
                    }
                }
            }
        }
        private int currentSkillPageIndex = 0;
        private int SkillPageCount => Mathf.CeilToInt((float)availableSkills.Count / skillButtons.Count);

        private ActiveSkillSO LockedInSkill
        {
            set
            {
                if (lockedInSkill == value) return;

                lockedInSkill = value;
                UpdateSkillDisplay(lockedInSkill);
            }
        }
        private ActiveSkillSO lockedInSkill;

        private ActionButton LockedInActionButton
        {
            set
            {
                if (lockedInActionButton == value) return;

                if (lockedInActionButton != null)
                {
                    lockedInActionButton.SetGlowActive(false);

                    if (lockedInActionButton == moveButton)
                    {
                        BattleManager.Instance.PlayerTurnManager.OnMovementRangeRemainingChange -= OnMovementRangeRemainingChange;
                    }
                }
                lockedInActionButton = value;
                if (lockedInActionButton != null)
                {
                    lockedInActionButton.SetGlowActive(true);

                    if (lockedInActionButton == moveButton)
                    {
                        BattleManager.Instance.PlayerTurnManager.OnMovementRangeRemainingChange += OnMovementRangeRemainingChange;
                    }
                }

                actionConfirmKeys.SetActive(lockedInActionButton != null && lockedInActionButton != inspectButton);
            }
        }
        private ActionButton lockedInActionButton;

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

            GlobalEvents.Scene.BattleSceneLoadedEvent += OnSceneLoad;

            BindButtonEvents();
        }

        private void OnDestroy()
        {
            GlobalEvents.Scene.BattleSceneLoadedEvent -= OnSceneLoad;
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        }

        #region Global Events
        private void OnSceneLoad()
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent += OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent += OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        }
        
        private void OnBattleEnd(UnitAllegiance unitAllegiance, int numTurns)
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;

            Hide();
        }

        private void OnPreviewUnit(Unit unit)
        {
            UpdateSkillDisplay(lockedInSkill, unit);
        }

        private void OnPreviewCurrentUnit(Unit unit)
        {
            if (unit is not PlayerUnit playerUnit)
            {
                Hide();
                return;
            }

            Show();

            currentUnit = unit;
            AvailableSkills = playerUnit.GetAvailableActiveSkills();

            if (canvasGroup.interactable)
            {
                inspectButton.Select();
            }
        }
        #endregion

        #region Inputs
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
                InputManager.Instance.SwitchTabInput.OnPressEvent += OnSwitchTab;
                InputManager.Instance.CancelActionInput.OnPressEvent += OnCancelAction;
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
                InputManager.Instance.SwitchTabInput.OnPressEvent -= OnSwitchTab;
                InputManager.Instance.CancelActionInput.OnPressEvent -= OnCancelAction;
            }
        }

        private void OnAction1(IInput input) { skillButtons[0].Select(); skillButtons[0].OnSubmit(null); }
        private void OnAction2(IInput input) { skillButtons[1].Select(); skillButtons[1].OnSubmit(null); }
        private void OnAction3(IInput input) { skillButtons[2].Select(); skillButtons[2].OnSubmit(null); }
        private void OnAction4(IInput input) { skillButtons[3].Select(); skillButtons[3].OnSubmit(null); }
        private void OnAction5(IInput input) { moveButton.Select(); moveButton.OnSubmit(null); }
        private void OnAction6(IInput input) { inspectButton.Select(); inspectButton.OnSubmit(null); }
        private void OnAction7(IInput input) { passButton.Select(); passButton.OnSubmit(null); }
        private void OnSwitchTab(IInput input)
        {
            var delta = input.GetValue<float>();
            OnScrollSkills((int)delta);
        }
        private void OnCancelAction(IInput input)
        {
            if (lockedInActionButton == null) return;

            if (BattleManager.Instance.PlayerTurnManager.TryCancelCurrentAction())
            {
                lockedInActionButton.OnSelect(null);
                LockedInActionButton = null;
            }
        }
        #endregion

        private void BindButtonEvents()
        {
            moveButton.onSelect.AddListener(OnSelectMove);
            moveButton.onSubmit.AddListener(OnSubmitMove);

            inspectButton.onSelect.AddListener(OnSelectInspect);
            inspectButton.onSubmit.AddListener(OnSubmitInspect);

            passButton.onSelect.AddListener(OnSelectPass);
            passButton.onSubmit.AddListener(OnSubmitPass);

            for (int i = 0; i < skillButtons.Count; i++)
            {
                var button = skillButtons[i];
                var index = i;
                button.onSelect.AddListener(() => OnSelectSkill(index));
                button.onSubmit.AddListener(() => OnSubmitSkill(index));
            }

            rightScrollButton.onSubmit.AddListener(() => OnScrollSkills(1));
            leftScrollButton.onSubmit.AddListener(() => OnScrollSkills(-1));
        }

        #region Move Action
        private void OnSelectMove()
        {
            if (lockedInActionButton != null) return;
            
            ShowMoveDescription();
        }

        private void OnSubmitMove()
        {
            ShowMoveDescription();

            if (lockedInActionButton != moveButton)
            {
                LockedInActionButton = moveButton;
                BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.SELECTING_MOVEMENT_SQUARE);
            }
        }

        private void ShowMoveDescription()
        {
            LockedInSkill = null;
            skillHeader.SetValue("Move");
            skillDescription.gameObject.SetActive(true);
            skillDescription.text = $"<sprite name=\"Steps\"> {BattleManager.Instance.PlayerTurnManager.MovementRangeRemaining}/{currentUnit.GetTotalStat(StatType.MOVEMENT_RANGE)}";
        }

        private void OnMovementRangeRemainingChange(int stepsLeft)
        {
            skillDescription.text = $"<sprite name=\"Steps\"> {stepsLeft}/{currentUnit.GetTotalStat(StatType.MOVEMENT_RANGE)}";
        }
        #endregion

        #region Inspect Action
        private void OnSelectInspect()
        {
            if (lockedInActionButton != null) return;

            ShowInspectDescription();
        }

        private void OnSubmitInspect()
        {
            ShowInspectDescription();

            if (lockedInActionButton != inspectButton)
            {
                LockedInActionButton = inspectButton;
                BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.INSPECT);
            }
        }

        private void ShowInspectDescription()
        {
            LockedInSkill = null;
            skillHeader.SetValue("Inspect");
            skillDescription.gameObject.SetActive(false);
        }
        #endregion

        #region Pass Action
        private void OnSelectPass()
        {
            if (lockedInActionButton != null) return;

            ShowPassDescription();
        }

        private void OnSubmitPass()
        {
            ShowPassDescription();

            if (lockedInActionButton != passButton)
            {
                LockedInActionButton = passButton;
            }
            else
            {
                BattleManager.Instance.PlayerTurnManager.EndTurn();
            }
        }

        private void ShowPassDescription()
        {
            LockedInSkill = null;
            skillHeader.SetValue("End Turn");
            skillDescription.gameObject.SetActive(false);
        }
        #endregion

        #region Skill Action
        private void OnSelectSkill(int index)
        {
            if (lockedInActionButton != null) return;

            UpdateSkillDisplay(availableSkills[index + currentSkillPageIndex * skillButtons.Count]);
        }

        private void OnSubmitSkill(int index)
        {
            var skill = availableSkills[index + currentSkillPageIndex * skillButtons.Count];
            BattleManager.Instance.PlayerTurnManager.SelectedSkill = skill;
            BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.SELECTING_ACTION_TARGET);
            LockedInSkill = skill;
            LockedInActionButton = skillButtons[index];
        }

        private void OnScrollSkills(int delta)
        {
            CurrentSkillPageIndex += delta;

            if (lockedInSkill != null)
            {
                lockedInActionButton.OnSubmit(null);
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
                builder.AppendLine($"HEAL: {DamageCalc.CalculateHealAmount(currentUnit, skill):G5}");
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
        #endregion

        #region Animations
        private void Show()
        {
            if (!isHidden) return;

            isHidden = false;
            animator.enabled = true;
            animator.Play(UIConstants.ShowAnimHash);
        }

        private void Hide()
        {
            if (isHidden) return;

            isHidden = true;
            animator.enabled = true;
            animator.Play(UIConstants.HideAnimHash);

            LockedInActionButton = null;
            LockedInSkill = null;
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
        #endregion
    }
}
