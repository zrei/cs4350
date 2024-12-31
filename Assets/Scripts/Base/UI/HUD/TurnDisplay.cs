using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.UI
{
    [RequireComponent(typeof(UIAnimator))]
    public class TurnDisplay : Singleton<TurnDisplay>
    {
        [SerializeField]
        private TurnDisplayUnit turnDisplayUnitPrefab;
        private ObjectPool<TurnDisplayUnit> displayPool;
        private Dictionary<Unit, TurnDisplayUnit> activeDisplays = new();

        [SerializeField]
        private FormattedTextDisplay unitName;

        [SerializeField]
        private FormattedTextDisplay timeToAct;

        [SerializeField]
        private float radius = 111;
        public float Radius => radius;

        private UIAnimator uiAnimator;

        private TurnDisplayUnit HighlightedDisplay
        {
            get => highlightedDisplay;
            set
            {
                if (highlightedDisplay != null)
                {
                    highlightedDisplay.OnPreviewUnit(null);
                }
                highlightedDisplay = value;
            }
        }
        private TurnDisplayUnit highlightedDisplay;

        protected override void HandleAwake()
        {
            base.HandleAwake();
            uiAnimator = GetComponent<UIAnimator>();

            unitName.gameObject.SetActive(false);
            timeToAct.gameObject.SetActive(false);

            GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;

            displayPool = new(
                createFunc: () =>
                {
                    var display = Instantiate(turnDisplayUnitPrefab, transform);
                    display.gameObject.SetActive(false);
                    return display;
                },
                actionOnGet: display =>
                {
                    display.gameObject.SetActive(true);
                },
                actionOnRelease: display => { display.gameObject.SetActive(false); },
                actionOnDestroy: display => { },
                defaultCapacity: 20,
                maxSize: 10000
            );
        }

        protected override void HandleDestroy()
        {
            base.HandleDestroy();

            GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;
            GlobalEvents.Battle.PlayerUnitSetupEndEvent -= OnSetupEnd;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;
        }

        private void OnSceneLoad(SceneEnum fromScene, SceneEnum toScene)
        {
            if (toScene != SceneEnum.BATTLE)
                return;

            GlobalEvents.Battle.PlayerUnitSetupEndEvent += OnSetupEnd;
            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
            GlobalEvents.Battle.PreviewUnitEvent += OnPreviewUnit;
            GlobalEvents.Scene.OnBeginSceneChange += OnSceneChange;
        }

        private void OnSetupEnd()
        {
            GlobalEvents.Battle.PlayerUnitSetupEndEvent -= OnSetupEnd;

            Show();
        }

        private void OnBattleEnd(UnitAllegiance _, int _2)
        {
            HandleQuit();
        }

        private void OnSceneChange(SceneEnum _, SceneEnum _2)
        {
            HandleQuit();
        }

        private void HandleQuit()
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;

            // Clear leftover mapping
            foreach (var display in activeDisplays.Values)
            {
                Destroy(display.gameObject);
            }
            activeDisplays.Clear();

            Hide();
        }

        public void OnPreviewUnit(Unit unit)
        {
            if (unit == null || !activeDisplays.TryGetValue(unit, out var display))
            {
                HighlightedDisplay = null;
                unitName.gameObject.SetActive(false);
                timeToAct.gameObject.SetActive(false);
                return;
            }

            display.OnPreviewUnit(unit);
            HighlightedDisplay = display;
            unitName.gameObject.SetActive(true);
            timeToAct.gameObject.SetActive(true);
            unitName.SetValue(unit.DisplayName);
            timeToAct.SetValue(display.TimeToAct);
        }

        public TurnDisplayUnit InstantiateTurnDisplayUnit(Unit unit)
        {
            TurnDisplayUnit display;
            if (!activeDisplays.ContainsKey(unit))
            {
                display = displayPool.Get();
                display.Initialize(unit);
                activeDisplays.Add(unit, display);
            }
            else
            {
                display = activeDisplays[unit];
            }

            return display;
        }

        public void RemoveTurnDisplayUnit(Unit unit)
        {
            if (!activeDisplays.ContainsKey(unit)) return;

            var display = activeDisplays[unit];
            displayPool.Release(display);
            activeDisplays.Remove(unit);
        }

        private void Show()
        {
            uiAnimator.Show();
        }

        private void Hide()
        {
            uiAnimator.Hide();
        }
    }
}
