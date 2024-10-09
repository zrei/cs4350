using Game.Input;
using UnityEngine;

namespace Game.UI
{
    public class UIInputHandler : MonoBehaviour
    {
        private void Start()
        {
            InputManager.Instance.CancelInput.OnPressEvent += OnCancel;
        }

        private void OnCancel(IInput input)
        {
            if (!UIScreenManager.Instance)
                return;
            
            var currentScreen = UIScreenManager.Instance.CurrentScreen;
            if (currentScreen != null)
            {
                currentScreen.OnCancel(input);
                return;
            }
            
            var pauseScreen = UIScreenManager.Instance.PauseScreen;
            UIScreenManager.Instance.OpenScreen(pauseScreen);
        }

        private void OnDestroy()
        {
            if (InputManager.IsReady)
            {
                InputManager.Instance.CancelInput.OnPressEvent -= OnCancel;
            }
        }
    }
}
