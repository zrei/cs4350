using Game.Input;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    public abstract class BaseUIScreen : MonoBehaviour, IUIScreen
    {
        [SerializeField]
        private bool applyBackgroundBlur;

        private Animator animator;
        private AnimatorCallbackExecuter animatorCallbackExecuter;
        private RectTransform rectTransform;
        private BackgroundBlur backgroundBlur;

        public event UIScreenCallback OnShowDone;
        public event UIScreenCallback OnHideDone;

        public RectTransform RectTransform => rectTransform;
        public bool IsInTransition => animator.enabled;

        public virtual void Initialize()
        {
            animator = GetComponentInChildren<Animator>();
            animator.enabled = false;
            animatorCallbackExecuter = animator.GetBehaviour<AnimatorCallbackExecuter>();

            rectTransform = transform as RectTransform;

            backgroundBlur = GetComponentInChildren<BackgroundBlur>();
        }

        public virtual void Show()
        {
            animator.enabled = true;
            animatorCallbackExecuter.RemoveAllListeners();
            animatorCallbackExecuter.AddListener(ShowDone);
            animator.Play(UIConstants.ShowAnimHash);

            if (applyBackgroundBlur)
            {
                if (backgroundBlur == null)
                {
                    var go = new GameObject("BackgroundBlur");

                    var rt = go.AddComponent<RectTransform>();
                    rt.SetParent(transform, false);
                    rt.SetAsFirstSibling();
                    rt.anchorMin = rectTransform.anchorMin;
                    rt.anchorMax = rectTransform.anchorMax;
                    rt.sizeDelta = rectTransform.sizeDelta;

                    backgroundBlur = go.AddComponent<BackgroundBlur>();
                }
                backgroundBlur.ApplyBlur();
            }
        }

        protected virtual void ShowDone()
        {
            animator.enabled = false;
            OnShowDone?.Invoke(this);
        }

        public virtual void Hide()
        {
            animator.enabled = true;
            animatorCallbackExecuter.RemoveAllListeners();
            animatorCallbackExecuter.AddListener(HideDone);
            animator.Play(UIConstants.HideAnimHash);
        }

        protected virtual void HideDone()
        {
            if ((backgroundBlur?.IsActive).GetValueOrDefault()) backgroundBlur.RemoveBlur();

            animator.enabled = false;
            OnHideDone?.Invoke(this);
        }

        public abstract void ScreenUpdate();

        public virtual void OnSubmit(IInput input)
        {
        }

        public virtual void OnCancel(IInput input)
        {
            UIScreenManager.Instance.CloseScreen();
        }
    }
}
