using System.Linq;
using UnityEngine;

namespace Game.UI
{
    public class ActiveSkillDisplay_Reclass : SkillDisplay_Reclass
    {
        protected override void InitialisePages(PlayerClassSO classSO)
        {
            base.InitialisePages(classSO);
            m_NumPages = Mathf.CeilToInt((float) classSO.m_ActiveSkills.Count() / m_NumSkillButtons);
        }

        protected override void UpdateDisplay()
        {
            int startingIndex = (m_CurrPage - 1) * m_NumSkillButtons;
            int minSkillNumber = Mathf.Min(m_CurrClass.m_ActiveSkills.Count() - startingIndex, m_NumSkillButtons);
            for (int i = 0; i < minSkillNumber; ++i)
            {
                m_SkillButtons[i].icon.sprite = m_CurrClass.m_ActiveSkills[startingIndex + i].m_Icon;
                ToggleCanvasGroup(m_SkillBtnCgs[i], true);
            }

            for (int i = minSkillNumber; i < m_NumSkillButtons; ++i)
            {
                ToggleCanvasGroup(m_SkillBtnCgs[i], false);
            }
        }

        protected override void HoverSkill(int index)
        {
            ActiveSkillSO activeSkill = m_CurrClass.m_ActiveSkills[(m_CurrPage - 1) * m_NumSkillButtons + index];
            m_SkillTitle.gameObject.SetActive(true);
            m_SkillTitle?.SetValue(activeSkill.m_SkillName);

            m_SkillDescription?.SetValue(activeSkill.GetDescription(m_CurrCharacter, null));
        }
    }
}
