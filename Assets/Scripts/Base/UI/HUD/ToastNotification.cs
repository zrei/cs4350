using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ToastNotification : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private GraphicGroup graphicGroup;

        public event VoidEvent onHideFinish;

        private Animator animator;
        private CanvasGroup canvasGroup;
        private VoidEvent onAnimationFinish;
        private bool isHidden;

        private Coroutine hideAfterDelayCoroutine;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isHidden = true;
        }

        public void Show(string message, Color color, float duration)
        {
            if (!isHidden) return;
            isHidden = false;

            text.text = message;
            graphicGroup.color = color;

            void WaitAndHide()
            {
                if (hideAfterDelayCoroutine != null)
                {
                    StopCoroutine(hideAfterDelayCoroutine);
                    hideAfterDelayCoroutine = null;
                }

                IEnumerator HideAfterDelay(float duration)
                {
                    yield return new WaitForSecondsRealtime(duration);
                    Hide();
                    hideAfterDelayCoroutine = null;
                }

                hideAfterDelayCoroutine = StartCoroutine(HideAfterDelay(duration));
            }

            onAnimationFinish = WaitAndHide;
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
            onAnimationFinish?.Invoke();

            if (isHidden)
            {
                onHideFinish?.Invoke();
            }
        }
    }
}
