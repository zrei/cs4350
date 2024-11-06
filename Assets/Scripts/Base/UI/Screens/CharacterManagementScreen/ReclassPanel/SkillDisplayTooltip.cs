using System.Collections;
using UnityEngine;

namespace Game.UI
{
    public struct TooltipContents
    {
        public string m_Title;
        public string m_Body;

        public TooltipContents(string title, string body)
        {
            m_Title = title;
            m_Body = body;
        }
    }

    public class SkillDisplayTooltip : MonoBehaviour
    {
        [SerializeField] private FormattedTextDisplay m_TitleText;
        [SerializeField] private FormattedTextDisplay m_BodyText;

        [SerializeField] private RectTransform m_ParentRectTransform;
        [SerializeField] private RectTransform m_RectTransform;

        private void OnEnable()
        {
            UpdatePosition();
            StartCoroutine(UpdatePosition_Coroutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void SetDisplay(TooltipContents tooltipContents)
        {
            m_TitleText?.SetValue(tooltipContents.m_Title);
            m_BodyText?.SetValue(tooltipContents.m_Body);
            this.gameObject.SetActive(true);
        }

        private IEnumerator UpdatePosition_Coroutine()
        {
            while (true)
            {
                yield return null;
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            m_RectTransform.anchoredPosition = UnityEngine.Input.mousePosition;
        }
    }
}
