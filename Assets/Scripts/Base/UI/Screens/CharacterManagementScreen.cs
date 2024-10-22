using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class CharacterManagementScreen : BaseUIScreen
    {
        [SerializeField]
        private CharacterOverviewDisplay m_CharacterOverviewDisplay;
        
        [SerializeField]
        private Transform m_PartyMemberButtonContainer;
        
        [SerializeField]
        private NamedObjectButton m_PartyMemberButtonPrefab;
        
        private List<NamedObjectButton> m_PartyMemberButtons = new();
        
        public NamedObjectButton SelectedPartyMemberButton
        {
            set
            {
                if (m_SelectedPartyMemberButton == value) return;
                
                if (m_SelectedPartyMemberButton != null)
                {
                    m_SelectedPartyMemberButton.SetGlowActive(false);
                }
                
                m_SelectedPartyMemberButton = value;
                if (m_SelectedPartyMemberButton != null)
                {
                    m_SelectedPartyMemberButton.SetGlowActive(true);
                }
            }
        }
        private NamedObjectButton m_SelectedPartyMemberButton;

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.UI.OpenPartyOverviewEvent += OnOpenPartyOverview;
        }
        
        private void OnDestroy()
        {
            GlobalEvents.UI.OpenPartyOverviewEvent -= OnOpenPartyOverview;
        }
        
        private void OnOpenPartyOverview(List<PlayerCharacterData> partyMembers)
        {
            // Clear existing party members
            foreach (var button in m_PartyMemberButtons)
            {
                Destroy(button.gameObject);
            }
            m_PartyMemberButtons.Clear();
            
            if (partyMembers.Count == 0)
            {
                m_CharacterOverviewDisplay.gameObject.SetActive(false);
                return;
            }
            
            foreach (var playerUnit in partyMembers)
            {
                var partyMemberButton = Instantiate(m_PartyMemberButtonPrefab, m_PartyMemberButtonContainer);
                partyMemberButton.SetObjectName(playerUnit.m_BaseData.m_CharacterName);
                partyMemberButton.onSubmit.AddListener(DisplayPartyMember);
                m_PartyMemberButtons.Add(partyMemberButton);
                continue;

                void DisplayPartyMember()
                {
                    SelectedPartyMemberButton = partyMemberButton;
                    m_CharacterOverviewDisplay.ViewUnit(playerUnit);
                }
            }
            
            SelectedPartyMemberButton = m_PartyMemberButtons[0];
            m_CharacterOverviewDisplay.ViewUnit(partyMembers[0]);
            m_CharacterOverviewDisplay.gameObject.SetActive(true);
        }
        
        public override void ScreenUpdate()
        {
        }
    }
}
