using Game.Input;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
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

        private Animator animator;
        private AnimatorCallbackExecuter animatorCallbackExecuter;
        private CanvasGroup canvasGroup;
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
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = transform as RectTransform;
            backgroundBlur = GetComponentInChildren<BackgroundBlur>();

            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public virtual void Show(params object[] args)
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

            if (m_OpenSound != null)
            {
                SoundManager.Instance.Play(m_OpenSound);
            }
        }

        protected virtual void ShowDone()
        {
            animator.enabled = false;
            OnShowDone?.Invoke(this);

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            animator.enabled = true;
            animatorCallbackExecuter.RemoveAllListeners();
            animatorCallbackExecuter.AddListener(HideDone);
            animator.Play(UIConstants.HideAnimHash);

            if (m_CloseSound != null)
            {
                SoundManager.Instance.Play(m_CloseSound);
            }
        }

        protected virtual void HideDone()
        {
            if ((backgroundBlur?.IsActive).GetValueOrDefault()) backgroundBlur.RemoveBlur();

            animator.enabled = false;
            OnHideDone?.Invoke(this);

            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
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
