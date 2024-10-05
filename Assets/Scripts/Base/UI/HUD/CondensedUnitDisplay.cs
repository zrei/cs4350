using System;
using System.Collections;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class CondensedUnitDisplay : MonoBehaviour
    {
        [SerializeField]
        private Vector3 offset;

        [SerializeField]
        private ProgressBar hpBar;

        [SerializeField]
        private StatusDisplay tokenDisplay;

        private Unit TrackedUnit
        {
            get => trackedUnit;
            set
            {
                if (trackedUnit == value) return;

                if (trackedUnit != null)
                {
                    trackedUnit.OnHealthChange -= OnHealthChange;
                    trackedUnit.OnDeath -= OnDeath;
                }

                trackedUnit = value;
                if (trackedUnit != null)
                {
                    trackedUnit.OnHealthChange += OnHealthChange;
                    trackedUnit.OnDeath += OnDeath;

                    if (hpBar != null)
                    {
                        hpBar.SetValue(trackedUnit.CurrentHealth, trackedUnit.MaxHealth, 0);
                    }

                    //tokenDisplay.TrackedStatusManager = trackedUnit.StatusManager;
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }
        private Unit trackedUnit;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;
        private event Action OnAnimationFinishEvent;

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
        }

        private void OnBattleEnd(UnitAllegiance unitAllegiance)
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;

            TrackedUnit = null;
            Hide();
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        }

        private void Start()
        {
            // if the Unit prefab is not instantiated in the battle scene, don't show this UI
            if (!WorldHUDManager.IsReady)
            {
                Destroy(gameObject);
                return;
            }

            var parentUnit = transform.parent.GetComponent<Unit>();
            if (parentUnit == null)
            {
                Destroy(gameObject);
                return;
            }

            TrackedUnit = parentUnit;
            WorldHUDManager.Instance.AddHUD(transform, GetUnitPosition);
        }

        private Vector3 GetUnitPosition()
        {
            if (TrackedUnit == null || !TrackedUnit)
            {
                WorldHUDManager.Instance.RemoveHUD(transform);
                Destroy(gameObject);
                return Vector3.zero;
            }

            return TrackedUnit.transform.position + offset;
        }

        private void OnHealthChange(float change, float value, float max)
        {
            IEnumerator ExecuteWithDelay()
            {
                yield return new WaitForSeconds(2.5f);
                hpBar?.SetValue(value, max);
            }
            StopAllCoroutines();
            StartCoroutine(ExecuteWithDelay());
        }

        private void OnDeath()
        {
            void Dispose()
            {
                OnAnimationFinishEvent -= Dispose;
                WorldHUDManager.Instance.RemoveHUD(transform);
                Destroy(gameObject);
            }
            OnAnimationFinishEvent += Dispose;
            TrackedUnit = null;
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
            animator.Play(UIConstants.HideAnimHash);
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
            canvasGroup.interactable = !isHidden;
            canvasGroup.blocksRaycasts = !isHidden;
            OnAnimationFinishEvent?.Invoke();
        }
    }
}
