using System.Collections;
using UnityEngine;

namespace Game.UI
{
    public class CondensedUnitDisplay : MonoBehaviour
    {
        [SerializeField]
        private Vector3 offset;

        [SerializeField]
        private ProgressBar hpBar;

        private Unit TrackedUnit
        {
            get => trackedUnit;
            set
            {
                if (trackedUnit == value) return;

                if (trackedUnit != null)
                {
                    trackedUnit.OnHealthChange -= OnHealthChange;
                }

                trackedUnit = value;
                if (trackedUnit != null)
                {
                    trackedUnit.OnHealthChange += OnHealthChange;

                    if (hpBar != null)
                    {
                        hpBar.SetValue(trackedUnit.CurrentHealth, trackedUnit.MaxHealth, 0);
                    }
                }
            }
        }
        private Unit trackedUnit;

        private void Start()
        {
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
    }
}
