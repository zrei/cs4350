using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class CurrentUnitMarker : MonoBehaviour
    {
        [SerializeField]
        private Vector3 worldOffset;

        private Unit trackedUnit;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

        private Coroutine followCoroutine;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            isHidden = true;

            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
            GlobalEvents.Battle.AttackAnimationEvent += OnAttackAnimation;
            GlobalEvents.Battle.PreviewCurrentUnitEvent += OnPreviewCurrentUnit;
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;
        }

        private void OnBattleEnd(UnitAllegiance _, int _2)
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewCurrentUnit;

            trackedUnit = null;
            Hide();
        }

        private void OnAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> target)
        {
            trackedUnit = null;
            Hide();
        }

        private void OnPreviewCurrentUnit(Unit unit)
        {
            if (trackedUnit == unit) return;
            var prevUnit = trackedUnit;
            trackedUnit = unit;

            if (unit == null)
            {
                Hide();
                return;
            }

            if (prevUnit != null)
            {
                Hide();
            }
            else
            {
                Show();
                BeginFollow();
            }
        }

        private void BeginFollow()
        {
            if (trackedUnit == null || !trackedUnit) return;

            if (followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
                followCoroutine = null;
            }
            followCoroutine = StartCoroutine(FollowPosition(trackedUnit.transform));
        }

        private IEnumerator FollowPosition(Transform t)
        {
            while (t && t != null)
            {
                //transform.localPosition = WorldHUDManager.Instance.WorldToHUDSpace(t.position + worldOffset);
                transform.position = t.position + worldOffset;
                var rot = CameraManager.Instance.MainCamera.transform.rotation;
                rot.x = 0;
                rot.z = 0;
                transform.rotation = rot;
                yield return null;
            }

            // if this is reached, the tracked transform was destroyed
            trackedUnit = null;
            Hide();
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
            animator.CrossFade(UIConstants.HideAnimHash, 0.1f);
        }

        private void OnAnimationFinish()
        {
            animator.enabled = !isHidden;
            
            if (trackedUnit != null)
            {
                BeginFollow();
                Show();
            }
        }
    }
}
