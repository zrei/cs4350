using UnityEngine;

namespace Game.UI
{
    public class PauseScreen : BaseUIScreen
    {
        [SerializeField] private NamedObjectButton m_MainMenuBtn;

        protected override void ShowDone()
        {
            base.ShowDone();

            m_MainMenuBtn.onSubmit.AddListener(B_MainMenu);
        }

        public override void Hide()
        {
            base.Hide();

            m_MainMenuBtn.onSubmit.RemoveListener(B_MainMenu);
        }

        private void OnDisable()
        {

        }
        public override void ScreenUpdate()
        {
            
        }

        private void B_MainMenu()
        {
            GlobalEvents.MainMenu.OnReturnToMainMenu?.Invoke();
            GameSceneManager.Instance.LoadMainMenuScene();
        }
    }
}
