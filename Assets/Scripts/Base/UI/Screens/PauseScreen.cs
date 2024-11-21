using UnityEngine;

namespace Game.UI
{
    public class PauseScreen : BaseUIScreen
    {
        [SerializeField] private NamedObjectButton m_MainMenuBtn;
        [SerializeField] private NamedObjectButton m_QuitLevelBtn;

        private const string SAVE_AND_QUIT_TEXT = "Save and Return to Main Menu";
        private const string QUIT_TEXT = "Return to Main Menu";

        public override void Show(params object[] args)
        {
            base.Show();

            m_QuitLevelBtn.gameObject.SetActive(GameSceneManager.Instance.CurrScene == SceneEnum.LEVEL || GameSceneManager.Instance.CurrScene == SceneEnum.BATTLE);
            m_MainMenuBtn.nameText.text = GameSceneManager.Instance.CurrScene == SceneEnum.WORLD_MAP ? SAVE_AND_QUIT_TEXT : QUIT_TEXT;
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
            UIScreenManager.Instance.CloseScreen();
            if (GameSceneManager.Instance.CurrScene == SceneEnum.WORLD_MAP)
            {
                GlobalEvents.Scene.SaveAndQuitEvent?.Invoke();
                SaveManager.Instance.Save(GameSceneManager.Instance.LoadMainMenuScene);
            }
            else
            {
                GameSceneManager.Instance.LoadMainMenuScene();
            }
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
