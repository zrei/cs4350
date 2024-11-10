using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public interface IStatusManager
{
    IEnumerable<TokenStack> TokenStacks { get; }
    IEnumerable<StatusEffect> StatusEffects { get; }
    event StatusEvent OnAdd;
    event StatusEvent OnRemove;
    event StatusEvent OnChange;
}

namespace Game.UI
{
    public class StatusDisplay : MonoBehaviour
    {
        private const int MaxDisplays = 20;

        [SerializeField]
        private IndividualStatusDisplay individualStatusDisplayPrefab;

        public Unit TrackedUnit
        {
            set
            {
                if (value == trackedUnit) return;

                if (activeDisplays.Count > 0)
                {
                    var displays = new List<IndividualStatusDisplay>(activeDisplays.Values);
                    activeDisplays.Clear();
                    displays.ForEach(x => { x.TrackedStatus = null; displayPool.Release(x); });
                }

                trackedUnit = value;

                permanentStatusCount = 0;
                regularStatusCount = 0;
                TrackedStatusManager = trackedUnit.StatusManager;

                foreach (var token in trackedUnit.PermanentTokens)
                {
                    var display = Get(true);
                    display.TrackedStatus = token;
                    activeDisplays.Add(token, display);
                }

                UpdateActiveLayoutGroups();
            }
        }
        private Unit trackedUnit;

        [SerializeField]
        private LayoutGroup regularStatusLayout;
        private Dictionary<IStatus, IndividualStatusDisplay> activeDisplays = new();
        private ObjectPool<IndividualStatusDisplay> displayPool;

        private IStatusManager TrackedStatusManager
        {
            set
            {
                if (trackedStatusManager == value) return;

                if (trackedStatusManager != null)
                {
                    trackedStatusManager.OnAdd -= OnAdd;
                    trackedStatusManager.OnRemove -= OnRemove;
                    trackedStatusManager.OnChange -= OnChange;
                }
                trackedStatusManager = value;
                if (trackedStatusManager != null)
                {
                    trackedStatusManager.OnAdd += OnAdd;
                    trackedStatusManager.OnRemove += OnRemove;
                    trackedStatusManager.OnChange += OnChange;

                    foreach (var status in trackedStatusManager.TokenStacks)
                    {
                        OnAdd(status);
                    }
                    foreach (var status in trackedStatusManager.StatusEffects)
                    {
                        OnAdd(status);
                    }
                }
            }
        }
        private IStatusManager trackedStatusManager;

        [SerializeField]
        private GameObject divider;

        [SerializeField]
        private LayoutGroup permanentStatusLayout;

        private int permanentStatusCount;
        private int regularStatusCount;

        private void Awake()
        {
            displayPool = new(
                createFunc: () => Instantiate(individualStatusDisplayPrefab, regularStatusLayout.transform),
                actionOnGet: display => { },
                actionOnRelease: display => display.gameObject.SetActive(false),
                actionOnDestroy: display => Destroy(display.gameObject),
                collectionCheck: true,
                defaultCapacity: 3,
                maxSize: MaxDisplays
            );
        }

        private void OnDestroy()
        {
            displayPool.Clear();
        }

        public IndividualStatusDisplay Get(bool isPermanentToken)
        {
            var display = displayPool.Get();
            var layout = isPermanentToken ? permanentStatusLayout : regularStatusLayout;
            if (isPermanentToken)
            {
                permanentStatusCount++;
            }
            else
            {
                regularStatusCount++;
            }

            display.SetIsPermanentToken(isPermanentToken);
            display.transform.SetParent(layout.transform, false);
            display.transform.SetAsFirstSibling();
            display.gameObject.SetActive(true);

            return display;
        }

        public void OnAdd(IStatus status)
        {
            if (displayPool.CountActive >= MaxDisplays) return;
            if (activeDisplays.ContainsKey(status)) return;

            var display = Get(false);
            display.TrackedStatus = status;
            activeDisplays.Add(status, display);
        }

        public void OnRemove(IStatus status)
        {
            if (activeDisplays.TryGetValue(status, out var display))
            {
                display.TrackedStatus = null;
                activeDisplays.Remove(status);
                displayPool.Release(display);
                regularStatusCount--;
            }
        }

        public void OnChange(IStatus status)
        {
            if (activeDisplays.TryGetValue(status, out var display))
            {
                display.OnChange();
            }
        }

        private void UpdateActiveLayoutGroups()
        {
            var regularStatusLayoutActive = regularStatusCount > 0;
            var permanentStatusLayoutActive = permanentStatusCount > 0;

            regularStatusLayout.gameObject.SetActive(regularStatusLayoutActive);
            permanentStatusLayout.gameObject.SetActive(permanentStatusLayoutActive);
            divider.gameObject.SetActive(regularStatusLayoutActive && permanentStatusLayoutActive);
        }
    }
}
