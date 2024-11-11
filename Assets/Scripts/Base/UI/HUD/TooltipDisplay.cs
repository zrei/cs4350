using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TooltipDisplay : MonoBehaviour
    {
        public event VoidEvent onHide;

        [SerializeField]
        private RectTransform m_Root;

        [SerializeField]
        private GraphicGroup m_GraphicGroup;

        [SerializeField]
        private TextMeshProUGUI m_Header;

        [SerializeField]
        private TextMeshProUGUI m_Text;

        CanvasGroup m_CanvasGroup;
        UIFader m_UIFader;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_UIFader = new(m_CanvasGroup);
        }

        public void Show(Vector2 pos, string header, string text, Color color, Vector2 pivot)
        {
            transform.localPosition = pos;
            m_Root.anchorMin = pivot;
            m_Root.anchorMax = pivot;
            m_Root.pivot = pivot;
            m_Root.anchoredPosition = Vector2.zero;

            m_GraphicGroup.color = color;
            m_Header.gameObject.SetActive(!string.IsNullOrEmpty(header));
            m_Header.text = header;
            m_Text.gameObject.SetActive(!string.IsNullOrEmpty(text));
            m_Text.text = text;

            m_UIFader.Show();
        }

        public void Hide()
        {
            m_UIFader.Hide(onComplete: onHide);
        }
    }
}
