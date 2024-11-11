using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class UIFader
    {
        public bool IsActive => !m_IsHidden;

        CanvasGroup m_CanvasGroup;
        bool m_IsHidden;

        public UIFader(CanvasGroup canvasGroup)
        {
            m_CanvasGroup = canvasGroup;
            m_CanvasGroup.alpha = 0;
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;
            m_IsHidden = true;
        }

        Coroutine ShowHideCo
        {
            set
            {
                if (value == m_ShowHideCo) return;

                if (m_ShowHideCo != null)
                {
                    CoroutineManager.Instance.StopCoroutine(m_ShowHideCo);
                }

                m_ShowHideCo = value;
            }
        }
        Coroutine m_ShowHideCo;

        public void Show(float duration = 0.1f, bool unscaledTime = true, VoidEvent onComplete = null, bool autoSetInteractable = false)
        {
            if (!m_IsHidden) return;

            m_IsHidden = false;
            ShowHideCo = CoroutineManager.Instance.StartCoroutine(ShowHideAnimate(true, duration, unscaledTime, onComplete, autoSetInteractable));
        }

        public void Hide(float duration = 0.1f, bool unscaledTime = true, VoidEvent onComplete = null, bool autoSetInteractable = false)
        {
            if (m_IsHidden) return;

            m_IsHidden = true;
            ShowHideCo = CoroutineManager.Instance.StartCoroutine(ShowHideAnimate(false, duration, unscaledTime, onComplete, autoSetInteractable));
        }

        IEnumerator ShowHideAnimate(bool active, float duration, bool unscaledTime, VoidEvent onComplete, bool setInteractable)
        {
            if (!active && setInteractable)
            {
                m_CanvasGroup.interactable = false;
                m_CanvasGroup.blocksRaycasts = false;
            }

            var startAlpha = m_CanvasGroup.alpha;
            var endAlpha = active ? 1f : 0f;

            var t = 0f;
            while (t < duration)
            {
                t += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                m_CanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
                yield return null;
            }

            m_CanvasGroup.alpha = endAlpha;
            if (active && setInteractable)
            {
                m_CanvasGroup.interactable = true;
                m_CanvasGroup.blocksRaycasts = true;
            }

            onComplete?.Invoke();
        }
    }
}
