using System.Collections;
using UnityEngine;

namespace Game.UI
{
    public class UIFader
    {
        public bool IsActive => !m_IsHidden;

        CanvasGroup m_CanvasGroup;
        bool m_IsHidden;
        bool m_IsInteractable;
        bool m_BlocksRaycast;
        Coroutine m_AnimateCo;

        public UIFader(CanvasGroup canvasGroup, bool hideOnAwake = true, bool isInteractable = true, bool blocksRaycast = true)
        {
            m_CanvasGroup = canvasGroup;
            m_IsHidden = hideOnAwake;
            m_IsInteractable = isInteractable;
            m_BlocksRaycast = blocksRaycast;
            m_CanvasGroup.alpha = m_IsHidden ? 0f : 1f;
            m_CanvasGroup.interactable = m_IsInteractable && !m_IsHidden;
            m_CanvasGroup.blocksRaycasts = m_BlocksRaycast && !m_IsHidden;
        }

        public void Show(float duration = 0.1f, bool unscaledTime = true, VoidEvent onComplete = null)
        {
            if (!m_IsHidden) return;

            m_IsHidden = false;
            if (m_AnimateCo != null)
            {
                CoroutineManager.Instance.StopCoroutine(m_AnimateCo);
                m_AnimateCo = null;
            }
            m_AnimateCo = CoroutineManager.Instance.StartCoroutine(Animate(m_IsHidden, duration, unscaledTime, onComplete));
        }

        public void Hide(float duration = 0.1f, bool unscaledTime = true, VoidEvent onComplete = null)
        {
            if (m_IsHidden) return;

            m_IsHidden = true;
            if (m_AnimateCo != null)
            {
                CoroutineManager.Instance.StopCoroutine(m_AnimateCo);
                m_AnimateCo = null;
            }
            m_AnimateCo = CoroutineManager.Instance.StartCoroutine(Animate(m_IsHidden, duration, unscaledTime, onComplete));
        }

        IEnumerator Animate(bool isHidden, float duration, bool unscaledTime, VoidEvent onComplete)
        {
            if (isHidden)
            {
                m_CanvasGroup.interactable = false;
                m_CanvasGroup.blocksRaycasts = false;
            }

            var startAlpha = m_CanvasGroup.alpha;
            var endAlpha = isHidden ? 0f : 1f;

            var t = 0f;
            while (t < duration)
            {
                t += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                m_CanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
                yield return null;
            }

            m_CanvasGroup.alpha = endAlpha;
            if (!isHidden)
            {
                m_CanvasGroup.interactable = m_IsInteractable;
                m_CanvasGroup.blocksRaycasts = m_BlocksRaycast;
            }

            onComplete?.Invoke();
        }
    }
}
