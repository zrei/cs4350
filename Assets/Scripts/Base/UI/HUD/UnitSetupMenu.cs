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

        private void Start()
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

            Show();
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.PlayerUnitSetupStartEvent -= OnSetupStart;
        }

        private void EndSetup()
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
        }
    }
}
