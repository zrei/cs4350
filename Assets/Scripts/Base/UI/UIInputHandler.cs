using Game.Input;
using UnityEngine;

namespace Game.UI
{
    public class UIInputHandler : MonoBehaviour
    {
        private bool m_PauseAllowed = true;

        private void Start()
        {
            InputManager.Instance.CancelInput.OnPressEvent += OnCancel;
            GlobalEvents.Dialogue.DialogueStartEvent += DisablePause;
            GlobalEvents.Dialogue.DialogueEndEvent += EnablePause;
            GlobalEvents.Save.OnBeginSaveEvent += DisablePause;
            GlobalEvents.Save.OnCompleteSaveEvent += EnablePause;
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
            
            if (!m_PauseAllowed)
                return;

            var pauseScreen = UIScreenManager.Instance.PauseScreen;
            UIScreenManager.Instance.OpenScreen(pauseScreen);
        }

        private void OnDestroy()
        {
            if (InputManager.IsReady)
            {
                InputManager.Instance.CancelInput.OnPressEvent -= OnCancel;
            }

            GlobalEvents.Dialogue.DialogueStartEvent -= DisablePause;
            GlobalEvents.Dialogue.DialogueEndEvent -= EnablePause;
            GlobalEvents.Save.OnBeginSaveEvent -= DisablePause;
            GlobalEvents.Save.OnCompleteSaveEvent -= EnablePause;
        }

        private void DisablePause()
        {
            m_PauseAllowed = false;
        }

        private void EnablePause()
        {
            m_PauseAllowed = true;
        }
    }
}
