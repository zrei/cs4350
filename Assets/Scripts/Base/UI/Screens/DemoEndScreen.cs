using UnityEngine;

namespace Game.UI
{
    public class DemoEndScreen : BaseUIScreen
    {
        [SerializeField] private NamedObjectButton m_ReturnToMainMenuButton;

        public override void ScreenUpdate()
        {
            
        }

        private void Awake()
        {
            m_ReturnToMainMenuButton.onSubmit.AddListener(ReturnToMainMenu);
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void RemoveListeners()
        {
            m_ReturnToMainMenuButton.onSubmit.RemoveListener(ReturnToMainMenu);
        }

        private void ReturnToMainMenu()
        {
            RemoveListeners();
            GlobalEvents.MainMenu.OnReturnToMainMenu?.Invoke();
            GameSceneManager.Instance.LoadMainMenuScene();
        }
    }
}