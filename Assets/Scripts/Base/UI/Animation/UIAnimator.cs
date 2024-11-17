using System.Collections;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIAnimator : MonoBehaviour
    {
        public event BoolEvent onAnimationEnd;

        [SerializeField]
        private bool m_HideOnAwake = true;

        [SerializeField]
        private bool m_IsInteractable = true;

        [SerializeField]
        private bool m_BlocksRaycast = true;

        [SerializeField]
        private bool m_HasIdleAnim;

        private Animator m_Animator;
        private CanvasGroup m_CanvasGroup;
        private bool m_IsHidden;
        private bool m_IsInTransition;

        private Coroutine m_AnimateCo;

        public CanvasGroup CanvasGroup => m_CanvasGroup;
        public bool IsInTransition => m_IsInTransition;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_Animator.enabled = false;

            m_CanvasGroup = GetComponent<CanvasGroup>();

            m_IsHidden = m_HideOnAwake;
            m_CanvasGroup.alpha = m_IsHidden ? 0f : 1f;
            m_CanvasGroup.interactable = m_IsInteractable && !m_IsHidden;
            m_CanvasGroup.blocksRaycasts = m_BlocksRaycast && !m_IsHidden;
        }

        public void Show(float crossFadeDuration = 0f)
        {
            if (!m_IsHidden) return;

            m_IsHidden = false;
            if (m_AnimateCo != null)
            {
                StopCoroutine(m_AnimateCo);
                m_AnimateCo = null;
                m_IsInTransition = false;
            }
            m_AnimateCo = StartCoroutine(Animate(m_IsHidden, UIConstants.ShowAnimHash, crossFadeDuration));
        }

        public void Hide(float crossFadeDuration = 0f)
        {
            if (m_IsHidden) return;

            m_IsHidden = true;
            if (m_AnimateCo != null)
            {
                StopCoroutine(m_AnimateCo);
                m_AnimateCo = null;
                m_IsInTransition = false;
            }
            m_AnimateCo = StartCoroutine(Animate(m_IsHidden, UIConstants.HideAnimHash, crossFadeDuration));
        }

        private IEnumerator Animate(bool isHidden, int animHash, float crossFadeDuration)
        {
            m_IsInTransition = true;

            // interactable should be disabled on Hide start
            if (isHidden)
            {
                m_CanvasGroup.interactable = false;
                m_CanvasGroup.blocksRaycasts = false;
            }
            
            m_Animator.enabled = true;
            if (crossFadeDuration > 0f)
            {
                m_Animator.CrossFade(animHash, crossFadeDuration);
            }
            else
            {
                m_Animator.Play(animHash);
            }
            while (m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != animHash)
            {
                yield return null;
            }
            while (m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == animHash)
            {
                yield return null;
            }
            m_Animator.enabled = m_HasIdleAnim && !isHidden;
            
            m_CanvasGroup.alpha = isHidden ? 0f : 1f;
            // interactable should be enabled on Show end
            if (!isHidden)
            {
                m_CanvasGroup.interactable = m_IsInteractable;
                m_CanvasGroup.blocksRaycasts = m_BlocksRaycast;
            }
            onAnimationEnd?.Invoke(isHidden);
            
            yield return new WaitForEndOfFrame();

            m_AnimateCo = null;
            m_IsInTransition = false;
        }
    }
}
