using Game.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        void Show();
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
        
        public IUIScreen BattleNodeResultScreen => LoadScreen("BattleNodeResultScreen");
        public IUIScreen RewardNodeResultScreen => LoadScreen("RewardNodeResultScreen");

        private IUIScreen LoadScreen(string name)
        {
            if (!screens.ContainsKey(name))
            {
                var prefab = Addressables.LoadAssetAsync<GameObject>($"{name}").WaitForCompletion();
                var screen = Instantiate(prefab, hidden).GetComponent<IUIScreen>();
                screen.Initialize();
                screens.Add(name, screen);
                Addressables.Release(prefab);
            }
            return screens[name];
        }

        public void OpenScreen(IUIScreen screen, bool clearStack = false)
        {
            if ((CurrentScreen?.IsInTransition).GetValueOrDefault()) return;

            if (IsScreenOpen(screen)) return;

            if (clearStack)
            {
                while (HasActiveScreen) CloseScreen();
            }

            activeScreens.Push(screen);
            screen.Show();
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

        private void Update()
        {
            CurrentScreen?.ScreenUpdate();
        }
    }
}
