using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.UI
{

    public class DamageDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Vector3 offset = new(0, 2, 0);

        [SerializeField]
        private float randomOffsetScale = 0.3f;

        private Transform anchor;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private Vector3 randomOffset;

        public event VoidEvent onAnimationFinishEvent;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public Vector3 GetAnchorPosition()
        {
            if (anchor == null) return Vector3.zero;

            return anchor.position + offset + randomOffset;
        }

        public void Show(string damageText, Transform anchor)
        {
            animator.enabled = true;
            animator.Play(UIConstants.ShowAnimHash);

            text.text = damageText;
            this.anchor = anchor;
            randomOffset = Random.insideUnitSphere * randomOffsetScale;
            //StopAllCoroutines();
            //StartCoroutine(Follow());
        }

        private IEnumerator Follow()
        {
            while (animator.enabled)
            {
                transform.position = GetAnchorPosition();
                transform.rotation = CameraManager.Instance.MainCamera.transform.rotation;
                yield return null;
            }
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
            onAnimationFinishEvent?.Invoke();
        }
    }
}
