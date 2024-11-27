using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CreditsScreen : BaseUIScreen
    {
        [SerializeField] NamedObjectButton m_CloseButton;
        [SerializeField] ScrollRect m_CreditsScroll;

        public override void ScreenUpdate()
        {
            // pass
        }

        public override void Show(params object[] args)
        {
            base.Show(args);

            m_CreditsScroll.verticalNormalizedPosition = 1f;
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
