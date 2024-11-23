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
            
            GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;
        }

        private void OnSceneLoad(SceneEnum fromScene, SceneEnum toScene)
        {
            if (toScene != SceneEnum.BATTLE)
                return;

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

            GlobalEvents.Scene.OnBeginSceneChange += OnSceneChange;

            Show();
        }

        private void OnDestroy()
        {
            GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;
            GlobalEvents.Battle.PlayerUnitSetupStartEvent -= OnSetupStart;
            GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;
        }

        private void EndSetup()
        {
            GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;

            BattleManager.Instance.PlayerUnitSetup.EndSetup();
            Hide();
        }

        private void OnSceneChange(SceneEnum _, SceneEnum _2)
        {
            GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;

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
