using UnityEngine;
using UnityEngine.Pool;

namespace Game.UI
{
    public class TooltipDisplayManager : Singleton<TooltipDisplayManager>
    {
        private const int MaxDisplays = 20;

        [SerializeField]
        private TooltipDisplay tooltipDisplayPrefab;

        [SerializeField]
        private RectTransform root;

        private ObjectPool<TooltipDisplay> displayPool;

        protected override void HandleAwake()
        {
            displayPool = new(
                createFunc: () =>
                {
                    var display = Instantiate(tooltipDisplayPrefab, root);
                    display.onHide += () => displayPool.Release(display);
                    return display;
                },
                actionOnGet: display => { display.gameObject.SetActive(true); display.transform.SetAsLastSibling(); },
                actionOnRelease: display => display.gameObject.SetActive(false),
                actionOnDestroy: display => Destroy(display.gameObject),
                collectionCheck: true,
                defaultCapacity: 3,
                maxSize: MaxDisplays
            );
        }

        /// <summary>
        /// Creates and returns a tooltip with the given parameters.
        /// Whoever calls this is responsible for hiding the tooltip once done.
        /// </summary>
        /// <returns></returns>
        public TooltipDisplay ShowTooltip(Vector3 pos, string header, string text, Color? color = null, Vector2? pivot = null)
        {
            var display = displayPool.Get();

            Vector2 localPos = transform.InverseTransformPoint(pos);
            var width = root.rect.width;
            var height = root.rect.height;
            Vector2 localPosNorm = new(localPos.x / width + 0.5f, localPos.y / height + 0.5f);
            Debug.Log(localPosNorm);
            Debug.Log(GetClosestCorner(localPosNorm));

            display.Show(
                localPos,
                header,
                text,
                color.GetValueOrDefault(Color.white),
                pivot.GetValueOrDefault(GetClosestCorner(localPosNorm)));
            return display;
        }

        private static readonly Vector2[] corners = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            //new Vector2(0.5f, 0),
            //new Vector2(0, 0.5f),
            //new Vector2(1, 0.5f),
            //new Vector2(0.5f, 1),
        };

        public static Vector2 GetClosestCorner(Vector2 normalizedPosition)
        {
            float closestDistance = float.MaxValue;
            Vector2 closestCorner = Vector2.zero;

            foreach (Vector2 corner in corners)
            {
                float distance = Vector2.SqrMagnitude(normalizedPosition - corner);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCorner = corner;
                }
            }

            return closestCorner;
        }
    }
}
