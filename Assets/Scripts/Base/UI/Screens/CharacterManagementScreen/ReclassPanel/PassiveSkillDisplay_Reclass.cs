using UnityEngine;

namespace Game.UI
{
    public class PassiveSkillDisplay_Reclass : SkillDisplay_Reclass
    {
        protected override void InitialisePages(PlayerClassSO classSO)
        {
            base.InitialisePages(classSO);
            m_NumPages = Mathf.CeilToInt((float) classSO.m_PassiveEffects.Count / m_NumSkillButtons);
        }

        protected override void UpdateDisplay()
        {
            int startingIndex = (m_CurrPage - 1) * m_NumSkillButtons;
            int minSkillNumber = Mathf.Min(m_CurrClass.m_PassiveEffects.Count - startingIndex, m_NumSkillButtons);
            var hasSkill = false;
            for (int i = 0; i < minSkillNumber; ++i)
            {
                m_SkillButtons[i].icon.sprite = m_CurrClass.m_PassiveEffects[startingIndex + i].m_PassiveEffectIcon;
                ToggleCanvasGroup(m_SkillBtnCgs[i], true);
                hasSkill = true;
            }

            for (int i = minSkillNumber; i < m_NumSkillButtons; ++i)
            {
                ToggleCanvasGroup(m_SkillBtnCgs[i], false);
            }

            m_NoneText?.gameObject.SetActive(!hasSkill);
        }

        protected override void HoverSkill(int index)
        {
            PassiveEffect classPassiveEffect = m_CurrClass.m_PassiveEffects[(m_CurrPage - 1) * m_NumSkillButtons + index];

            m_SkillTitle.gameObject.SetActive(true);
            m_SkillTitle?.SetValue(classPassiveEffect.m_Name);

            m_SkillDescription?.SetValue(classPassiveEffect.m_Description);
        }
    }
}
