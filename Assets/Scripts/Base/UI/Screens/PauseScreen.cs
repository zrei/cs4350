using UnityEngine;

namespace Game.UI
{
    public class PauseScreen : BaseUIScreen
    {
        [SerializeField] private NamedObjectButton m_MainMenuBtn;
        [SerializeField] private NamedObjectButton m_QuitLevelBtn;

        public override void Show()
        {
            base.Show();

            m_QuitLevelBtn.gameObject.SetActive(GameSceneManager.Instance.CurrScene == SceneEnum.LEVEL || GameSceneManager.Instance.CurrScene == SceneEnum.BATTLE);
        }

        protected override void ShowDone()
        {
            base.ShowDone();

            m_MainMenuBtn.onSubmit.AddListener(B_MainMenu);
            m_QuitLevelBtn.onSubmit.AddListener(B_QuitLevel);
        }

        public override void Hide()
        {
            base.Hide();

            RemoveListeners();
        }

        public override void ScreenUpdate()
        {
            
        }

        private void B_MainMenu()
        {
            RemoveListeners();
            GlobalEvents.MainMenu.OnReturnToMainMenu?.Invoke();
            GameSceneManager.Instance.LoadMainMenuScene();
        }

        private void B_QuitLevel()
        {
            RemoveListeners();
            GlobalEvents.Scene.EarlyQuitEvent?.Invoke();
            UIScreenManager.Instance.CloseScreen();
            GameSceneManager.Instance.ReturnToWorldMap();
        }

        private void RemoveListeners()
        {
            m_MainMenuBtn.onSubmit.RemoveListener(B_MainMenu);
            m_QuitLevelBtn.onSubmit.RemoveListener(B_QuitLevel);
        }
    }
}
