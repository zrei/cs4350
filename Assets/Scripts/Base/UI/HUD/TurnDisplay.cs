using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class TurnDisplay : Singleton<TurnDisplay>
    {
        [SerializeField]
        private FormattedTextDisplay unitName;

        [SerializeField]
        private FormattedTextDisplay timeToAct;

        [SerializeField]
        private float radius = 111;
        public float Radius => radius;

        private Dictionary<Unit, TurnDisplayUnit> mapping = new();
        private TurnDisplayUnit prefab;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

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
            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            isHidden = true;

            unitName.gameObject.SetActive(false);
            timeToAct.gameObject.SetActive(false);

            GlobalEvents.Scene.BattleSceneLoadedEvent += OnSceneLoad;
        }

        private void OnDestroy()
        {
            GlobalEvents.Scene.BattleSceneLoadedEvent -= OnSceneLoad;
            GlobalEvents.Battle.PlayerUnitSetupEndEvent -= OnSetupEnd;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
        }

        private void OnSceneLoad()
        {
            GlobalEvents.Battle.PlayerUnitSetupEndEvent += OnSetupEnd;
            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
            GlobalEvents.Battle.PreviewUnitEvent += OnPreviewUnit;
        }

        private void OnSetupEnd()
        {
            GlobalEvents.Battle.PlayerUnitSetupEndEvent -= OnSetupEnd;

            Show();
        }

        private void OnBattleEnd(UnitAllegiance _, int _2)
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;

            // Clear leftover mapping
            foreach (var display in mapping.Values)
            {
                Destroy(display.gameObject);
            }
            mapping.Clear();

            Hide();
        }

        private void OnPreviewUnit(Unit unit)
        {
            if (unit == null || !mapping.TryGetValue(unit, out var display))
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
            Debug.Log("Instantiating turn display unit for " + unit.name);
            if (prefab == null)
            {
                prefab = Addressables
                    .LoadAssetAsync<GameObject>("TurnDisplayUnit")
                    .WaitForCompletion()
                    .GetComponent<TurnDisplayUnit>();
            }

            TurnDisplayUnit display;
            if (!mapping.ContainsKey(unit))
            {
                display = Instantiate(prefab);
                display.transform.SetParent(transform, false);
                display.Initialize(unit);
                mapping.Add(unit, display);
            }
            else
            {
                display = mapping[unit];
            }
            
            return display;
        }

        public void RemoveTurnDisplayUnit(Unit display)
        {
            mapping.Remove(display);
        }

        private void Show()
        {
            isHidden = false;
            animator.enabled = true;
            animator.Play(UIConstants.ShowAnimHash);
        }

        private void Hide()
        {
            isHidden = true;
            animator.enabled = true;
            animator.Play(UIConstants.HideAnimHash);
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
            canvasGroup.interactable = !isHidden;
            canvasGroup.blocksRaycasts = !isHidden;
        }
    }
}
