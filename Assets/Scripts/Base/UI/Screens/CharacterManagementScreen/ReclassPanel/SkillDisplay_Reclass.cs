using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public abstract class SkillDisplay_Reclass : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private ActionButton m_LeftScrollButton;
        [SerializeField] private ActionButton m_RightScrollButton;
        [SerializeField] protected List<ActionButton> m_SkillButtons;   

        [Header("Visibility")]
        [SerializeField] private CanvasGroup m_LeftScrollBtnCg;
        [SerializeField] private CanvasGroup m_RightScrollBtnCg;
        [SerializeField] protected List<CanvasGroup> m_SkillBtnCgs;

        [Header("Description")]
        [SerializeField] protected FormattedTextDisplay m_SkillTitle;
        [SerializeField] protected FormattedTextDisplay m_SkillDescription;
        [SerializeField] protected TextMeshProUGUI m_NoneText;

        protected int m_NumSkillButtons;
        protected int m_NumPages;
        protected int m_CurrPage;

        protected PlayerClassSO m_CurrClass;
        protected PlayerCharacterData m_CurrCharacter;

        private void Start()
        {
            m_NumSkillButtons = m_SkillButtons.Count;

            m_LeftScrollButton.onSubmit.RemoveAllListeners();
            m_LeftScrollButton.onSubmit.AddListener(ScrollLeft);

            m_RightScrollButton.onSubmit.RemoveAllListeners();
            m_RightScrollButton.onSubmit.AddListener(ScrollRight);

            for (int i = 0; i < m_SkillButtons.Count; ++i)
            {
                int cachedIndex = i;
                m_SkillButtons[i].onPointerEnter.AddListener(() => HoverSkill(cachedIndex));
            }
        }

        private void ScrollLeft()
        {
            --m_CurrPage;

            ResetDescription();
            UpdateScrollButtons();
            UpdateDisplay();
        }

        private void ScrollRight()
        {
            ++m_CurrPage;

            ResetDescription();
            UpdateScrollButtons();
            UpdateDisplay();
        }

        private void ResetDescription()
        {
            m_SkillTitle.gameObject.SetActive(false);
        }

        private void UpdateScrollButtons()
        {
            ToggleCanvasGroup(m_LeftScrollBtnCg, m_CurrPage > 1);
            ToggleCanvasGroup(m_RightScrollBtnCg, m_CurrPage < m_NumPages);
        }

        protected abstract void UpdateDisplay();

        protected virtual void InitialisePages(PlayerClassSO classSO)
        {
            m_CurrPage = 1;
        }

        public virtual void SetDisplay(PlayerCharacterData playerCharacterData, PlayerClassSO classSO)
        {
            m_CurrClass = classSO;
            m_CurrCharacter = playerCharacterData;
            InitialisePages(classSO);
            ResetDescription();
            UpdateScrollButtons();
            UpdateDisplay();
        }

        protected abstract void HoverSkill(int index);

        protected void ToggleCanvasGroup(CanvasGroup cg, bool toShow)
        {
            cg.alpha = toShow ? 1f : 0f;
            cg.interactable = toShow;
            cg.blocksRaycasts = toShow;
        }
    }
}
