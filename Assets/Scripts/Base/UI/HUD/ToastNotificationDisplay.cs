using UnityEngine;
using UnityEngine.Pool;

namespace Game.UI
{
    public class ToastNotificationDisplay : Singleton<ToastNotificationDisplay>
    {
        [SerializeField]
        private ToastNotification toastNotificationPrefab;

        [SerializeField]
        private RectTransform root;

        private ObjectPool<ToastNotification> displayPool;

        private ToastNotification activeDisplay;

        public bool DebugNotification
        {
            get => debugNotification;
            set
            {
                debugNotification = value;
                Show($"{Time.time}");
            }
        }
        [SerializeField]
        [SerializeProperty("DebugNotification")]
        private bool debugNotification;

        protected override void HandleAwake()
        {
            base.HandleAwake();
            displayPool = new(
                createFunc: () =>
                {
                    var display = Instantiate(toastNotificationPrefab, root.transform);
                    display.onHideFinish += () =>
                    {
                        displayPool.Release(display);
                        if (activeDisplay == display)
                        {
                            activeDisplay = null;
                        }
                    };
                    return display;
                },
                actionOnGet: display =>
                {
                    display.gameObject.SetActive(true);
                    display.transform.SetAsLastSibling();
                },
                actionOnRelease: display =>
                {
                    display.gameObject.SetActive(false);
                },
                actionOnDestroy: display => Destroy(display.gameObject),
                collectionCheck: true,
                defaultCapacity: 3,
                maxSize: 3
            );
        }

        public void Show(string message, float duration = 2f)
        {
            Show(message, Color.white, duration);
        }

        public void Show(string message, Color color, float duration = 2f)
        {
            if (activeDisplay != null)
            {
                activeDisplay.Hide();
            }

            activeDisplay = displayPool.Get();
            activeDisplay.Show(message, color, duration);
        }
    }
}
