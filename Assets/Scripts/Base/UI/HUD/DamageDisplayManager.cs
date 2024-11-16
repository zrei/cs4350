using UnityEngine;
using UnityEngine.Pool;

namespace Game.UI
{
    public class DamageDisplayManager : Singleton<DamageDisplayManager>
    {
        private const int MaxDisplays = 20;

        [SerializeField]
        private DamageDisplay damageDisplayPrefab;

        private ObjectPool<DamageDisplay> displayPool;

        protected override void HandleAwake()
        {
            displayPool = new(
                createFunc: () =>
                {
                    var display = Instantiate(damageDisplayPrefab);
                    display.onAnimationFinishEvent += () => displayPool.Release(display);
                    WorldSpaceHUDManager.Instance.AddHUD(display.transform, () => display.GetAnchorPosition());
                    return display;
                },
                actionOnGet: display => { display.gameObject.SetActive(true); display.transform.SetAsFirstSibling(); },
                actionOnRelease: display => display.gameObject.SetActive(false),
                actionOnDestroy: display => Destroy(display.gameObject),
                collectionCheck: true,
                defaultCapacity: 3,
                maxSize: MaxDisplays
            );
        }

        public void ShowDamage(string text, Transform anchor)
        {
            ShowDamage(text, Color.white, anchor);
        }

        public void ShowDamage(string text, Color color, Transform anchor)
        {
            var display = displayPool.Get();
            display.Show(text, color, anchor);
        }
    }
}
