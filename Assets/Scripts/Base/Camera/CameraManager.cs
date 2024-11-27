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
        }
    }
}
