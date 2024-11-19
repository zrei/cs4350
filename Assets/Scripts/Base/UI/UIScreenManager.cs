using Game.Input;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public delegate void UIScreenCallback(IUIScreen screen);

    public interface IUIScreen
    {
        RectTransform RectTransform { get; }
        bool  IsInTransition { get; }
        event UIScreenCallback OnShowDone;
        event UIScreenCallback OnHideDone;

        void Initialize();
        void Show(params object[] args);
        void Hide();
        void ScreenUpdate();

        void OnCancel(IInput input);
        void OnSubmit(IInput input);
    }

    public class UIScreenManager : Singleton<UIScreenManager>
    {
        [SerializeField]
        private RectTransform root;

        [SerializeField]
        private RectTransform hidden;

        private Stack<IUIScreen> activeScreens = new();
        private Dictionary<string, IUIScreen> screens = new();

        public IUIScreen CurrentScreen => activeScreens.TryPeek(out IUIScreen screen) ? screen : null;
        public bool HasActiveScreen => activeScreens.Count > 0;

        public IUIScreen PauseScreen => LoadScreen("PauseScreen");
        
        public IUIScreen CharacterManagementScreen => LoadScreen("CharacterManagementScreen");
        
        public IUIScreen BattleNodeResultScreen => LoadScreen("BattleNodeResultScreen");
        public IUIScreen RewardNodeResultScreen => LoadScreen("RewardNodeResultScreen");
        public IUIScreen LevelUpResultScreen => LoadScreen("LevelUpResultScreen");
        public IUIScreen LevelResultScreen => LoadScreen("LevelResultScreen");
        public IUIScreen PartySelectScreen => LoadScreen("PartySelectScreen");
        public IUIScreen InspectScreen => LoadScreen("InspectScreen");
        public IUIScreen PreviewScreen => LoadScreen("PreviewScreen");
        public IUIScreen ExpScreen => LoadScreen("ExpScreen");
        public IUIScreen DemoEndScreen => LoadScreen("EndDemoScreen");
        public IUIScreen SaveScreen => LoadScreen("SaveScreen");

        [SerializeField]
        private List<GameObject> screenPrefabs = new();

        private IUIScreen LoadScreen(string name)
        {
            if (!screens.ContainsKey(name))
            {
                //var prefab = Addressables.LoadAssetAsync<GameObject>($"{name}").WaitForCompletion();
                var prefab = screenPrefabs.Find(x => x.name == name);
                if (prefab == null) return null;
                var screen = Instantiate(prefab, hidden).GetComponent<IUIScreen>();
                screen.Initialize();
                screens.Add(name, screen);
                //Addressables.Release(prefab);
            }
            return screens[name];
        }

        public void OpenScreen(IUIScreen screen, bool clearStack = false, params object[] args)
        {
            if ((CurrentScreen?.IsInTransition).GetValueOrDefault()) return;

            if (IsScreenOpen(screen)) return;

            if (clearStack)
            {
                while (HasActiveScreen) CloseScreen();
            }

            activeScreens.Push(screen);
            screen.Show(args);
            screen.RectTransform.SetParent(root, false);
            screen.RectTransform.SetAsLastSibling();

            HUDRoot.Instance.Hide();
        }

        public void CloseScreen()
        {
            if ((CurrentScreen?.IsInTransition).GetValueOrDefault()) return;

            if (activeScreens.TryPop(out IUIScreen screen))
            {
                void OnHideDone(IUIScreen s)
                {
                    s.RectTransform.SetParent(hidden, false);
                    s.OnHideDone -= OnHideDone;
                }
                screen.OnHideDone += OnHideDone;
                screen.Hide();

                HUDRoot.Instance.Show();
            }
        }

        public bool IsScreenOpen(IUIScreen screen)
        {
            return activeScreens.Contains(screen);
        }
        
        public bool IsScreenActive(IUIScreen screen)
        {
            return activeScreens.TryPeek(out IUIScreen activeScreen) && activeScreen == screen;
        }

        private void Update()
        {
            CurrentScreen?.ScreenUpdate();
        }
    }
}
