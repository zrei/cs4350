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
        public float radius = 60;

        private Dictionary<Unit, TurnDisplayUnit> mapping = new();
        private TurnDisplayUnit prefab;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

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

            GlobalEvents.Battle.PlayerUnitSetupEndEvent += OnSetupEnd;
            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        }

        private void OnSetupEnd()
        {
            GlobalEvents.Battle.PlayerUnitSetupEndEvent -= OnSetupEnd;

            Show();
        }

        private void OnBattleEnd(UnitAllegiance _, int _2)
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;

            Hide();
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.PlayerUnitSetupEndEvent -= OnSetupEnd;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        }

        public TurnDisplayUnit InstantiateTurnDisplayUnit(Unit unit)
        {
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
