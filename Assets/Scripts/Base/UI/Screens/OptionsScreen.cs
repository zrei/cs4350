using UnityEngine;

namespace Game.UI
{
    public class OptionsScreen : BaseUIScreen
    {
        [SerializeField] NamedObjectButton m_CloseButton;

        public override void ScreenUpdate()
        {
            // pass
        }

        protected override void ShowDone()
        {
            base.ShowDone();

            m_CloseButton.onSubmit.AddListener(B_Close);
        }

        protected override void HideDone()
        {
            base.HideDone();

            m_CloseButton.onSubmit.RemoveAllListeners();
        }

        private void B_Close()
        {
            UIScreenManager.Instance.CloseScreen();
        }
    }
}
