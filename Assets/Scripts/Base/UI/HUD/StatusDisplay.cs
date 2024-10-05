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

        [SerializeField]
        private LayoutGroup layout;

        private Dictionary<IStatus, IndividualStatusDisplay> activeDisplays = new();

        private ObjectPool<IndividualStatusDisplay> displayPool;

        public IStatusManager TrackedStatusManager
        {
            get => trackedStatusManager;
            set
            {
                if (trackedStatusManager == value) return;

                if (trackedStatusManager != null)
                {
                    trackedStatusManager.OnAdd -= OnAdd;
                    trackedStatusManager.OnRemove -= OnRemove;
                    trackedStatusManager.OnChange -= OnChange;

                    foreach (var status in activeDisplays.Keys)
                    {
                        OnRemove(status);
                    }
                }
                trackedStatusManager = value;
                if (trackedStatusManager != null)
                {
                    trackedStatusManager.OnAdd += OnAdd;
                    trackedStatusManager.OnRemove += OnRemove;
                    trackedStatusManager.OnChange += OnChange;

                    foreach (var status in trackedStatusManager.TokenStacks)
                    {
                        //OnAdd(status);
                    }
                    foreach (var status in trackedStatusManager.StatusEffects)
                    {
                        OnAdd(status);
                    }
                }
            }
        }
        private IStatusManager trackedStatusManager;

        private void Awake()
        {
            displayPool = new(
                createFunc: () => Instantiate(individualStatusDisplayPrefab, layout.transform),
                actionOnGet: display => display.gameObject.SetActive(true),
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

        public void OnAdd(IStatus status)
        {
            if (displayPool.CountActive >= MaxDisplays) return;

            IndividualStatusDisplay display;
            if (activeDisplays.TryAdd(status, display = displayPool.Get()))
            {
                display.TrackedStatus = status;
            }
        }

        public void OnRemove(IStatus status)
        {
            if (activeDisplays.TryGetValue(status, out var display))
            {
                display.TrackedStatus = null;
                activeDisplays.Remove(status);
                displayPool.Release(display);
            }
        }

        public void OnChange(IStatus status)
        {
            if (activeDisplays.TryGetValue(status, out var display))
            {
                display.OnChange();
            }
        }
    }
}
