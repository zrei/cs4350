using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public struct CharacterManagementUIData
    {
        public List<PlayerCharacterData> PartyMembers;
        public bool InLevel;

        public CharacterManagementUIData(List<PlayerCharacterData> partyMemebers, bool inLevel)
        {
            PartyMembers = partyMemebers;
            InLevel = inLevel;
        }
    }

    public class CharacterManagementScreen : BaseUIScreen
    {
        private enum Tab 
        {
            OVERVIEW,
            RECLASS,
            WEAPON,
        }

        [Header("Panel")]
        [SerializeField] private CharacterOverviewDisplay m_CharacterOverviewDisplay;
        [SerializeField] private ReclassPanel m_ReclassPanel;
        [SerializeField] private WeaponsOverviewDisplay m_WeaponsOverviewDisplay;
        [SerializeField] private SkillsAndStatusPanel m_SkillsAndStatusPanel;

        [Header("Buttons")]
        [SerializeField]
        private NamedObjectButton m_WeaponsOverviewButton;
        [SerializeField]
        private Sprite m_WeaponsOverviewOpenSprite;
        [SerializeField]
        private Sprite m_WeaponsOverviewCloseSprite;

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
            m_WeaponsOverviewDisplay.OnOverviewEvent += () => TabSwitch(Tab.OVERVIEW);

            m_SkillsAndStatusPanel.OnReclassEvent += () => TabSwitch(Tab.RECLASS);

            m_WeaponsOverviewButton.onSubmit.AddListener(() => TabSwitch(m_CurrTab != Tab.WEAPON ? Tab.WEAPON : Tab.OVERVIEW));
        }

        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            ShowPartyOverview((CharacterManagementUIData) args[0]);

            base.Show();
        }

        protected override void HideDone()
        {
            base.HideDone();

            GlobalEvents.UI.OnClosePartyOverviewEvent?.Invoke();
            
            if (!m_IsInLevel)
            {
                GlobalEvents.UI.SavePartyChangesEvent?.Invoke();
            }
        }

        private void ShowPartyOverview(CharacterManagementUIData characterManagementUIData)
        {
            m_IsInLevel = characterManagementUIData.InLevel;

            // Clear existing party members
            foreach (var button in m_PartyMemberButtons)
            {
                Destroy(button.gameObject);
            }
            m_PartyMemberButtons.Clear();
            
            if (characterManagementUIData.PartyMembers.Count == 0)
            {
                m_CharacterOverviewDisplay.gameObject.SetActive(false);
                return;
            }
            
            foreach (var playerUnit in characterManagementUIData.PartyMembers)
            {
                var partyMemberButton = Instantiate(m_PartyMemberButtonPrefab, m_PartyMemberButtonContainer);
                partyMemberButton.SetObjectName(playerUnit.m_BaseData.m_CharacterName);
                partyMemberButton.onSubmit.AddListener(() => B_DisplayPartyMemnber(partyMemberButton, playerUnit));
                m_PartyMemberButtons.Add(partyMemberButton);
                continue;
            }

            CoroutineManager.Instance.ExecuteAfterFrames(() => B_DisplayPartyMemnber(m_PartyMemberButtons[0], characterManagementUIData.PartyMembers[0]), 1);
            
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
            m_WeaponsOverviewDisplay.DisplayUnitWeapons(playerCharacterData);
            m_SkillsAndStatusPanel.ViewUnit(playerCharacterData);
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
            m_WeaponsOverviewDisplay.gameObject.SetActive(tab == Tab.WEAPON);
            if (tab == Tab.WEAPON)
            {
                m_WeaponsOverviewDisplay.Show();
            }
            else
            {
                m_WeaponsOverviewDisplay.Hide();
            }
            m_SkillsAndStatusPanel.gameObject.SetActive(tab == Tab.OVERVIEW);

            m_CurrTab = tab;
            m_WeaponsOverviewButton.SetGlowActive(m_CurrTab == Tab.WEAPON);
            m_WeaponsOverviewButton.icon.sprite = m_CurrTab == Tab.WEAPON ? m_WeaponsOverviewCloseSprite : m_WeaponsOverviewOpenSprite;
        }
    }
}
