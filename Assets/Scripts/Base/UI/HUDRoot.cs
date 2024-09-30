using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class HUDRoot : Singleton<HUDRoot>
    {
        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

        protected override void HandleAwake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            if (!isHidden) return;

            isHidden = false;
            animator.enabled = true;
            animator.Play(UIConstants.ShowAnimHash);
        }

        public void Hide()
        {
            if (isHidden) return;

            isHidden = true;
            animator.enabled = true;
            animator.Play(UIConstants.HideAnimHash);
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
            canvasGroup.interactable = !isHidden;
            canvasGroup.blocksRaycasts = !isHidden;
        }
    }
}
