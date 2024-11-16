using Game.Input;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(UIAnimator))]
    public abstract class BaseUIScreen : MonoBehaviour, IUIScreen
    {
        [SerializeField]
        private bool applyBackgroundBlur;

        [SerializeField]
        [Tooltip("Leave empty for no sound")]
        private AudioDataSO m_OpenSound;

        [SerializeField]
        [Tooltip("Leave empty for no sound")]
        private AudioDataSO m_CloseSound;

        private UIAnimator uiAnimator;
        private RectTransform rectTransform;
        private BackgroundBlur backgroundBlur;

        public event UIScreenCallback OnShowDone;
        public event UIScreenCallback OnHideDone;

        public RectTransform RectTransform => rectTransform;
        public bool IsInTransition => uiAnimator.IsInTransition;

        public virtual void Initialize()
        {
            uiAnimator = GetComponent<UIAnimator>();
            uiAnimator.onAnimationEnd += OnAnimationFinish;

            rectTransform = transform as RectTransform;
            backgroundBlur = GetComponentInChildren<BackgroundBlur>();
        }

        public virtual void Show(params object[] args)
        {
            uiAnimator.Show();

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

            if (m_OpenSound != null)
            {
                SoundManager.Instance.Play(m_OpenSound);
            }
        }

        public virtual void Hide()
        {
            uiAnimator.Hide();

            if (m_CloseSound != null)
            {
                SoundManager.Instance.Play(m_CloseSound);
            }
        }

        private void OnAnimationFinish(bool isHidden)
        {
            if (!isHidden) ShowDone();
            else HideDone();
        }

        protected virtual void ShowDone()
        {
            OnShowDone?.Invoke(this);
        }

        protected virtual void HideDone()
        {
            if ((backgroundBlur?.IsActive).GetValueOrDefault()) backgroundBlur.RemoveBlur();

            OnHideDone?.Invoke(this);
        }

        public abstract void ScreenUpdate();

        public virtual void OnSubmit(IInput input)
        {
        }

        public virtual void OnCancel(IInput input)
        {
            Close();
        }

        public void Close()
        {
            UIScreenManager.Instance.CloseScreen();
        }
    }
}
