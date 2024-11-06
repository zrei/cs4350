using System.Collections;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UnitSetupMenu : MonoBehaviour
    {
        [SerializeField]
        private SelectableBase button;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            isHidden = true;

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

            if (!isHidden)
            {
                button.Select();
            }
        }
    }
}
