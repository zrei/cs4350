using System.Collections;
using System.Collections.Generic;
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
    }
}
