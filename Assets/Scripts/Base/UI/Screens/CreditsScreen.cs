using UnityEngine;

namespace Game.UI
{
    public class CreditsScreen : BaseUIScreen
    {
        [SerializeField] NamedObjectButton m_CloseButton;

        public override void ScreenUpdate()
        {
            // pass
        }

        private void Awake()
        {
            m_CloseButton.onSubmit.AddListener(B_Close);
        }

        private void OnDestroy()
        {
            m_CloseButton.onSubmit.RemoveAllListeners();
        }

        private void B_Close()
        {
            Hide();
        }
    }
}