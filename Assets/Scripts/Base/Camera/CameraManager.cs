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

        [SerializeField]
        private Camera attackAnimCamera;

        public Camera MainCamera => mainCamera;
        public Camera HUDCamera => hudCamera;
        public Camera UICamera => uiCamera;
        public Camera AttackAnimCamera => attackAnimCamera;
        
        
        [SerializeField] private Transform m_LevelCameraTransform;
        [SerializeField] private Transform m_BattleCameraTransform;
        
        public void SetUpLevelCamera()
        {
            var mainCameraTransform = CameraManager.Instance.MainCamera.transform;
            CameraManager.Instance.MainCamera.orthographic = true;
            mainCameraTransform.position = m_LevelCameraTransform.position;
            mainCameraTransform.rotation = m_LevelCameraTransform.rotation;
        }

        public void SetUpBattleCamera()
        {
            var mainCameraTransform = CameraManager.Instance.MainCamera.transform;
            CameraManager.Instance.MainCamera.orthographic = false;
            mainCameraTransform.position = m_BattleCameraTransform.position;
        }
    }
}
