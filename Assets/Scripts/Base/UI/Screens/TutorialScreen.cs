using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [System.Serializable]
    public struct TutorialPageUIData
    {
        public Sprite TutorialSprite;
        public string TutorialText;
    }

    public class TutorialScreen : BaseUIScreen
    {
        [Header("UI Reference")]
        [SerializeField] private Image m_TutorialImage;
        [SerializeField] private TextMeshProUGUI m_TutorialText;

        [Header("Buttons")]
        [SerializeField] private ActionButton m_LeftScrollButton;
        [SerializeField] private ActionButton m_RightScrollButton;
        [SerializeField] private NamedObjectButton m_CloseButton;

        private List<TutorialPageUIData> m_PageData;
        private int m_PageIndex = 0;
    
        private void Awake()
        {
            m_LeftScrollButton.onSubmit.AddListener(LastPage);
            m_RightScrollButton.onSubmit.AddListener(NextPage);
            m_CloseButton.onSubmit.AddListener(B_Close);
        }
    
        private void OnDestroy()
        {
            m_LeftScrollButton.onSubmit.RemoveListener(LastPage);
            m_RightScrollButton.onSubmit.RemoveListener(NextPage);
            m_CloseButton.onSubmit.RemoveListener(B_Close);
        }

        public override void ScreenUpdate()
        {
            // pass
        }

        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            m_PageData = (List<TutorialPageUIData>) args[0];
            m_PageIndex = 0;

            ShowPage();

            base.Show();
        }

        private void ShowPage()
        {
            m_TutorialImage.sprite = m_PageData[m_PageIndex].TutorialSprite;
            m_TutorialText.text = m_PageData[m_PageIndex].TutorialText;

            m_LeftScrollButton.interactable = m_PageIndex > 0;
            m_RightScrollButton.interactable = m_PageIndex < m_PageData.Count - 1;

            m_CloseButton.interactable = m_PageIndex == m_PageData.Count - 1;
        }

        private void LastPage()
        {
            --m_PageIndex;
            
            ShowPage();
        }

        private void NextPage()
        {
            ++m_PageIndex;
            ShowPage();
        }

        private void B_Close()
        {
            UIScreenManager.Instance.CloseScreen();
        }
    }
}