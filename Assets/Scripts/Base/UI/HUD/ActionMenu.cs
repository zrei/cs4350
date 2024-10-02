using System.Collections;
using System.Collections.Generic;
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

        private ActiveSkillSO SelectedSkill
        {
            get => selectedSkill;
            set
            {
                selectedSkill = value;
                skillHeader.SetValue(selectedSkill.m_SkillName);
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
        }
    }
}
