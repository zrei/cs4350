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
            
            for (int i = 0; i < minSkillNumber; ++i)
            {
                m_SkillButtons[i].icon.sprite = m_CurrClass.m_PassiveEffects[startingIndex + i].m_PassiveEffectIcon;
                m_SkillButtons[i].gameObject.SetActive(true);
            }

            for (int i = minSkillNumber; i < m_NumSkillButtons; ++i)
            {
                m_SkillButtons[i].gameObject.SetActive(false);
            }
        }

        protected override void HoverSkill(int index)
        {
            ClassPassiveEffect classPassiveEffect = m_CurrClass.m_PassiveEffects[(m_CurrPage - 1) * m_NumSkillButtons + index];
            GlobalEvents.CharacterManagement.OnTooltipEvent?.Invoke(new TooltipContents(classPassiveEffect.m_Name, classPassiveEffect.m_Description));
        }
    }
}
