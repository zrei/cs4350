using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ToggleableUIRoot : MonoBehaviour,
        IToggleableUI
    {
        public VisibilityTags VisibilityTags => m_VisibilityTags;

        [SerializeField]
        private VisibilityTags m_VisibilityTags;

        private CanvasGroup m_CanvasGroup;
        private UIFader m_UIFader;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_UIFader = new(m_CanvasGroup);

            if (!UIManager.IsReady)
            {
                UIManager.OnReady += Initialize;
                return;
            }
            Initialize();
        }

        private void Initialize()
        {
            UIManager.OnReady -= Initialize;

            UIManager.Instance.Add(this);
        }

        private void OnDestroy()
        {
            if (UIManager.IsReady)
                UIManager.Instance.Remove(this);
        }

        public void Show()
        {
            m_UIFader.Show();
        }

        public void Hide()
        {
            m_UIFader.Hide();
        }
    }
}
