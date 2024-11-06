using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public abstract class SkillDisplay_Reclass : MonoBehaviour
    {
        [SerializeField] protected ActionButton m_LeftScrollButton;
        [SerializeField] protected ActionButton m_RightScrollButton;
        [SerializeField] protected List<SkillDisplayButton> m_SkillButtons;   

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
                m_SkillButtons[i].onPointerExit.AddListener(UnhoverSkill);
            }
        }

        private void ScrollLeft()
        {
            --m_CurrPage;

            UpdateScrollButtons();
            UpdateDisplay();
        }

        private void ScrollRight()
        {
            ++m_CurrPage;

            UpdateScrollButtons();
            UpdateDisplay();
        }

        private void UpdateScrollButtons()
        {
            m_LeftScrollButton.SetActive(m_CurrPage > 1);
            m_RightScrollButton.SetActive(m_CurrPage < m_NumPages);
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
            UpdateScrollButtons();
            UpdateDisplay();
        }

        protected abstract void HoverSkill(int index);

        protected void UnhoverSkill()
        {
            Debug.Log("Stop showing tooltip");
            GlobalEvents.CharacterManagement.OnHideTooltipEvent?.Invoke();
        }
    }
}
