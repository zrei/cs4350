using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class CharacterManagementScreen : BaseUIScreen
    {
        private enum Tab 
        {
            OVERVIEW,
            RECLASS
        }

        [Header("Panel")]
        [SerializeField] private CharacterOverviewDisplay m_CharacterOverviewDisplay;
        [SerializeField] private ReclassPanel m_ReclassPanel;
        [SerializeField] private SkillsAndWeaponPanel m_SkillsAndWeaponPanel;
        
        [Header("Party List")]
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

        private bool m_IsInLevel = false;

        private Tab m_CurrTab;

        private void Awake()
        {
            m_ReclassPanel.OnOverviewEvent += () => TabSwitch(Tab.OVERVIEW);
            m_SkillsAndWeaponPanel.OnReclassEvent += () => TabSwitch(Tab.RECLASS);
        }

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.UI.OpenPartyOverviewEvent += OnOpenPartyOverview;
        }
        
        private void OnDestroy()
        {
            GlobalEvents.UI.OpenPartyOverviewEvent -= OnOpenPartyOverview;
        }

        protected override void HideDone()
        {
            base.HideDone();

            if (!m_IsInLevel)
            {
                GlobalEvents.UI.OnClosePartyOverviewEvent?.Invoke();
                SaveManager.Instance.Save();
            }
        }

        private void OnOpenPartyOverview(List<PlayerCharacterData> partyMembers, bool inLevel)
        {
            m_IsInLevel = inLevel;

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
                partyMemberButton.onSubmit.AddListener(() => B_DisplayPartyMemnber(partyMemberButton, playerUnit));
                m_PartyMemberButtons.Add(partyMemberButton);
                continue;
            }
            
            B_DisplayPartyMemnber(m_PartyMemberButtons[0], partyMembers[0]);
            
            // Temporary way to pass party members
            //m_CharacterOverviewDisplay.SetPartyMembers(partyMembers);
        }
        
        public override void ScreenUpdate()
        {
        }

        void B_DisplayPartyMemnber(NamedObjectButton partyMemberButton, PlayerCharacterData playerCharacterData)
        {
            SelectedPartyMemberButton = partyMemberButton;
            m_CharacterOverviewDisplay.ViewUnit(playerCharacterData);
            m_ReclassPanel.SetDisplay(playerCharacterData);
            m_SkillsAndWeaponPanel.ViewUnit(playerCharacterData);
            UpdateDisplay(Tab.OVERVIEW);
        }

        private void TabSwitch(Tab tab)
        {
            if (m_CurrTab == tab)
                return;

            UpdateDisplay(tab);
        }

        private void UpdateDisplay(Tab tab)
        {
            m_ReclassPanel.gameObject.SetActive(tab == Tab.RECLASS);
            m_SkillsAndWeaponPanel.gameObject.SetActive(tab == Tab.OVERVIEW);

            m_CurrTab = tab;
        }
    }
}
