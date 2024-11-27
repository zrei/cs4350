using Game.UI;
using UnityEngine;

namespace Game
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private Camera hudCamera;

        [SerializeField]
        private Camera uiCamera;

        public Camera MainCamera => mainCamera;
        public Camera HUDCamera => hudCamera;
        public Camera UICamera => uiCamera;
    
        
        protected override void HandleAwake()
        {
            base.HandleAwake();

            transform.SetParent(null);
            DontDestroyOnLoad(this.gameObject);

            mainCamera.enabled = false;

            GlobalEvents.Scene.OnSceneTransitionEvent += OnSceneTransition;

            HandleDependencies();
        }

        private void HandleDependencies()
        {
            if (!UIScreenManager.IsReady)
            {
                UIScreenManager.OnReady += HandleDependencies;
                return;
            }

            UIScreenManager.OnReady -= HandleDependencies;

            UIScreenManager.Instance.CreditsScreen.OnShowDone += OnShowSecondaryScreen;
            UIScreenManager.Instance.OptionScreen.OnShowDone += OnShowSecondaryScreen;
        }

        protected override void HandleDestroy()
        {
            base.HandleDestroy();

            GlobalEvents.Scene.OnSceneTransitionEvent -= OnSceneTransition;
        }

        public void SetUpLevelCamera()
        {
            Instance.MainCamera.orthographic = true;
        }

        public void SetUpBattleCamera()
        {
            Instance.MainCamera.orthographic = false;
        }

        private void OnSceneTransition(SceneEnum finalScene)
        {
            mainCamera.enabled = finalScene != SceneEnum.MAIN_MENU;
        }

        private void OnShowSecondaryScreen(IUIScreen uiScreen)
        {
            if (GameSceneManager.Instance.CurrScene == SceneEnum.MAIN_MENU)
            {
                uiScreen.OnHideDone += OnHideSecondaryScreen;
                mainCamera.enabled = true;
            }
        }

        private void OnHideSecondaryScreen(IUIScreen uiScreen)
        {
            uiScreen.OnHideDone -= OnHideSecondaryScreen;
            mainCamera.enabled = false;
        }
    }
}
