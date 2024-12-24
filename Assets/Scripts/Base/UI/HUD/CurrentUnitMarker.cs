using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(UIAnimator))]
    public class CurrentUnitMarker : MonoBehaviour
    {
        [SerializeField]
        private Vector3 worldOffset;

        private Unit trackedUnit;

        private UIAnimator uiAnimator;

        private Coroutine followCoroutine;

        private void Awake()
        {
            uiAnimator = GetComponent<UIAnimator>();
            uiAnimator.onAnimationEnd += OnAnimationFinish;

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

        private void OnAttackAnimation()
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
            uiAnimator.Show();
        }

        public void Hide()
        {
            uiAnimator.Hide(0.1f);
        }

        private void OnAnimationFinish(bool isHidden)
        {
            if (trackedUnit != null)
            {
                BeginFollow();
                Show();
            }
        }
    }
}
