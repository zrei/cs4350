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
            var currentScreen = UIScreenManager.Instance?.CurrentScreen;
            if (currentScreen != null)
            {
                currentScreen.OnCancel(input);
                return;
            }

            UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.PauseScreen);
        }

        private void OnDestroy()
        {
            InputManager.Instance.CancelInput.OnPressEvent -= OnCancel;
        }
    }
}
