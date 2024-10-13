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
        
        
        [SerializeField] private Transform m_LevelCameraTransform;
        [SerializeField] private Transform m_BattleCameraTransform;
        
        public void SetUpLevelCamera()
        {
            Instance.MainCamera.orthographic = true;
        }

        public void SetUpBattleCamera()
        {
            Instance.MainCamera.orthographic = false;
        }
    }
}
