using Game.Input;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(UIAnimator))]
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
        private List<SkillButton> skillButtons = new();

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

                leftScrollButton.interactable = hasMultiPage;
                rightScrollButton.interactable = hasMultiPage;
            }
        }
        private List<ActiveSkillSO> availableSkills = new();

        private int CurrentSkillPageIndex
        {
            get => currentSkillPageIndex;
            set
            {
                if (availableSkills.Count == 0)
                {
                    currentSkillPageIndex = 0;
                    skillButtons.ForEach(x => x.SetState(SkillButtonState.EMPTY));
                    return;
                }

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
                        button.SkillSprite = skill.m_Icon;

                        if (!currentUnit.HasEnoughManaForSkill(skill))
                        {
                            button.SetFill(0f);
                            button.statusText?.SetValue(skill.m_ConsumedMana, "<sprite name=\"Mana\" tint>");
                            button.SetState(SkillButtonState.LOCKED);
                        }
                        else
                        {
                            button.SetFill(currentUnit.GetSkillCooldownProportion(skill));
                            int cooldown = currentUnit.GetSkillCooldown(skill);
                            button.statusText?.SetValue("<sprite name=\"Turn\" tint>", cooldown);
                            button.SetState(cooldown > 0 ? SkillButtonState.LOCKED : SkillButtonState.NORMAL);
                        }
                    }
                    else
                    {
                        button.SetFill(1f);
                        button.SetState(SkillButtonState.EMPTY);
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

        private UIAnimator uiAnimator;

        private void Awake()
        {
            uiAnimator = GetComponent<UIAnimator>();
            uiAnimator.onAnimationEnd += OnAnimationFinish;

            GlobalEvents.Scene.BattleSceneLoadedEvent += OnSceneLoad;

            BindButtonEvents();
        }

        private void OnDestroy()
        {
            GlobalEvents.Scene.BattleSceneLoadedEvent -= OnSceneLoad;
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Scene.EarlyQuitEvent -= OnEarlyQuit;
        }

        #region Global Events
        private void OnSceneLoad()
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent += OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent += OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
            GlobalEvents.Scene.EarlyQuitEvent += OnEarlyQuit;
        }

        private void OnEarlyQuit()
        {
            HandleQuit();
        }

        private void OnBattleEnd(UnitAllegiance unitAllegiance, int numTurns)
        {
            HandleQuit();
        }

        private void HandleQuit()
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Scene.EarlyQuitEvent -= OnEarlyQuit;

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
            AvailableSkills = playerUnit.GetActiveSkills().ToList();

            if (uiAnimator.CanvasGroup.interactable)
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

            var skillIndex = index + currentSkillPageIndex * skillButtons.Count;
            if (skillIndex >= availableSkills.Count)
            {
                var diff = availableSkills.Count - 1 - skillIndex;
                skillIndex = availableSkills.Count - 1;
                index += diff;
                skillButtons[index].OnSelect(null);
                return;
            }

            UpdateSkillDisplay(availableSkills[skillIndex]);
        }

        private void OnSubmitSkill(int index)
        {
            var skillIndex = index + currentSkillPageIndex * skillButtons.Count;
            if (skillIndex >= availableSkills.Count)
            {
                var diff = availableSkills.Count - 1 - skillIndex;
                skillIndex = availableSkills.Count - 1;
                index += diff;
                skillButtons[index].OnSubmit(null);
                return;
            }

            var skill = availableSkills[index + currentSkillPageIndex * skillButtons.Count];
            BattleManager.Instance.PlayerTurnManager.SelectedSkill = skill;
            BattleManager.Instance.PlayerTurnManager.TransitToAction(PlayerTurnState.SELECTING_ACTION_TARGET);
            LockedInSkill = skill;
            LockedInActionButton = skillButtons[index];
        }

        private void OnScrollSkills(int delta)
        {
            CurrentSkillPageIndex += delta;

            if (lockedInActionButton == moveButton || lockedInActionButton == inspectButton || lockedInActionButton == passButton)
                return;

            foreach (var button in skillButtons)
            {
                if (button.interactable)
                {
                    button.OnSubmit(null);
                    return;
                }
            }
        }

        private void UpdateSkillDisplay(ActiveSkillSO skill, IHealth target = null)
        {
            if (skill == null) return;

            skillHeader.SetValue(skill.m_SkillName);
            var descriptionText = skill.GetDescription(currentUnit, target);

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
            uiAnimator.Show();
        }

        private void Hide()
        {
            uiAnimator.Hide();

            LockedInActionButton = null;
            LockedInSkill = null;
        }

        private void OnAnimationFinish(bool isHidden)
        {
            if (!isHidden)
            {
                inspectButton.Select();
            }
            BindInputEvents(!isHidden);
        }
        #endregion
    }
}
