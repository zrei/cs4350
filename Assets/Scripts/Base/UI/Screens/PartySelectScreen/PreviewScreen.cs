using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PreviewScreen : BaseUIScreen
    {
        [SerializeField] SkillsOverviewDisplay m_SkillsDisplay;
        [SerializeField] CharacterStatDisplay m_CharacterStatDisplay;
        [SerializeField] StatusDisplay m_StatusDisplay;
        [SerializeField] Image m_CharacterImg;
        [SerializeField] FormattedTextDisplay m_CharacterName;
        [SerializeField] private NamedObjectButton m_CloseButton;

        private void Awake()
        {
            m_CloseButton.onSubmit.AddListener(Close);
        }

        private void OnDestroy()
        {
            m_CloseButton.onSubmit.RemoveAllListeners();
        }

        protected override void HideDone()
        {
            base.HideDone();

            m_SkillsDisplay.Hide();
        }

        public override void ScreenUpdate()
        {
            // pass
        }

        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            PlayerCharacterData character = args[0] as PlayerCharacterData;
            m_SkillsDisplay.DisplayUnitSkills(character);
            m_CharacterStatDisplay.SetDisplay(character);
            m_CharacterImg.sprite = character.m_BaseData.m_CharacterSprite;
            m_StatusDisplay.SetStatuses(new List<IStatus>(), character.m_BaseData.GetInflictedMoralityTokens(MoralityManager.Instance.CurrMoralityPercentage));
            m_CharacterName?.SetValue(character.m_BaseData.m_CharacterName, character.CurrClass.m_ClassName);
            m_SkillsDisplay.Show();

            base.Show();
        }
    }
}
