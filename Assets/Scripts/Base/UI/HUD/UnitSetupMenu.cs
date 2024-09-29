using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UnitSetupMenu : MonoBehaviour
    {
        [SerializeField]
        private SelectableBase button;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;

            button.onSubmit.RemoveAllListeners();
            button.onSubmit.AddListener(EndSetup);

            GlobalEvents.Battle.PlayerUnitSetupStartEvent += OnSetupStart;
        }

        private void OnSetupStart()
        {
            Show();
        }

        private void EndSetup()
        {
            BattleManager.Instance.PlayerUnitSetup.EndSetup();
            Hide();
        }

        private void Show()
        {
            animator.enabled = true;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            animator.Play(UIConstants.ShowAnimHash);
        }

        private void Hide()
        {
            animator.enabled = true;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            animator.Play(UIConstants.HideAnimHash);
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
        }

        private void OnDisable()
        {
            GlobalEvents.Battle.PlayerUnitSetupStartEvent -= OnSetupStart;
        }
    }
}
