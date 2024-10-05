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

        [SerializeField]
        private Camera levelCamera;

        public Camera MainCamera => mainCamera;
        public Camera HUDCamera => hudCamera;
        public Camera UICamera => uiCamera;
        public Camera AttackAnimCamera => attackAnimCamera;
        public Camera LevelCamera => levelCamera;
    }
}
