using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
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
                if (trackedUnit == value) return;

                Clear();

                trackedUnit = value;

                if (trackedUnit != null)
                {
                    TrackedStatusManager = trackedUnit.StatusManager;

                    foreach (var token in trackedUnit.PermanentTokens)
                    {
                        var display = Get(true);
                        display.TrackedStatus = token;
                        activeDisplays.Add(token, display);
                    }
                }
                else
                {
                    TrackedStatusManager = null;
                }

                UpdateActiveLayoutGroups();
            }
        }
        private Unit trackedUnit;

        [SerializeField]
        private LayoutGroup regularStatusLayout;
        private Dictionary<IStatus, IndividualStatusDisplay> activeDisplays = new();
        private HashSet<IndividualStatusDisplay> untrackedActiveDisplays = new();
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

        [SerializeField]
        private GameObject emptyIndicator;

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

            if (regularStatusLayout != null) regularStatusLayout.gameObject.SetActive(regularStatusLayoutActive);
            if (permanentStatusLayout != null) permanentStatusLayout.gameObject.SetActive(permanentStatusLayoutActive);
            if (divider != null) divider.gameObject.SetActive(regularStatusLayoutActive && permanentStatusLayoutActive);
            if (emptyIndicator != null) emptyIndicator.gameObject.SetActive(!regularStatusLayoutActive && !permanentStatusLayoutActive);
        }

        public void Clear()
        {
            if (activeDisplays.Count > 0)
            {
                var displays = new List<IndividualStatusDisplay>(activeDisplays.Values);
                activeDisplays.Clear();
                displays.ForEach(x => { x.TrackedStatus = null; displayPool.Release(x); });
            }

            if (untrackedActiveDisplays.Count > 0)
            {
                var displays = untrackedActiveDisplays.ToList();
                untrackedActiveDisplays.Clear();
                displays.ForEach(x => { x.TrackedStatus = null; displayPool.Release(x); });
            }

            permanentStatusCount = 0;
            regularStatusCount = 0;
        }

        /// <summary>
        /// Set the statuses to display directly, in case Unit or Status Manager is unavailable.
        /// </summary>
        public void SetStatuses(IEnumerable<IStatus> regularStatuses, IEnumerable<IStatus> permanentStatuses)
        {
            Clear();

            foreach (var regularStatus in regularStatuses)
            {
                var display = Get(false);
                display.TrackedStatus = regularStatus;
                untrackedActiveDisplays.Add(display);
            }

            foreach (var permanentStatus in permanentStatuses)
            {
                var display = Get(true);
                display.TrackedStatus = permanentStatus;
                untrackedActiveDisplays.Add(display);
            }

            UpdateActiveLayoutGroups();
        }
    }
}
