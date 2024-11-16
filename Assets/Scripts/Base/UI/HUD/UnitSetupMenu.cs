using System.Collections;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(UIAnimator))]
    public class UnitSetupMenu : MonoBehaviour
    {
        [SerializeField]
        private SelectableBase button;

        private UIAnimator uiAnimator;

        private void Awake()
        {
            uiAnimator = GetComponent<UIAnimator>();
            uiAnimator.onAnimationEnd += OnAnimationFinish;

            button.onSubmit.RemoveAllListeners();
            button.onSubmit.AddListener(EndSetup);
            
            GlobalEvents.Scene.BattleSceneLoadedEvent += OnSceneLoad;
        }

        private void OnSceneLoad()
        {
            var playerUnitSetup = BattleManager.Instance.PlayerUnitSetup;
            if (playerUnitSetup == null || !playerUnitSetup.IsSetupStarted)
            {
                GlobalEvents.Battle.PlayerUnitSetupStartEvent += OnSetupStart;
            }
            else
            {
                OnSetupStart();
            }
        }

        private void OnSetupStart()
        {
            GlobalEvents.Battle.PlayerUnitSetupStartEvent -= OnSetupStart;

            GlobalEvents.Scene.EarlyQuitEvent += OnEarlyQuit;

            Show();
        }

        private void OnDestroy()
        {
            GlobalEvents.Scene.BattleSceneLoadedEvent -= OnSceneLoad;
            GlobalEvents.Battle.PlayerUnitSetupStartEvent -= OnSetupStart;
            GlobalEvents.Scene.EarlyQuitEvent -= OnEarlyQuit;
        }

        private void EndSetup()
        {
            GlobalEvents.Scene.EarlyQuitEvent -= OnEarlyQuit;

            BattleManager.Instance.PlayerUnitSetup.EndSetup();
            Hide();
        }

        private void OnEarlyQuit()
        {
            GlobalEvents.Scene.EarlyQuitEvent -= OnEarlyQuit;

            Hide();
        }

        private void HandleEnd()
        {
            BattleManager.Instance.PlayerUnitSetup.EndSetup();
            Hide();
        }

        private void Show()
        {
            uiAnimator.Show();
        }

        private void Hide()
        {
            uiAnimator.Hide();
        }

        private void OnAnimationFinish(bool isHidden)
        {
            if (!isHidden)
            {
                button.Select();
            }
        }
    }
}
