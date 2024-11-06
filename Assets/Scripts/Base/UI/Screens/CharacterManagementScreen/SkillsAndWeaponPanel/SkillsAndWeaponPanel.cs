using UnityEngine;
using UnityEngine.Events;

namespace Game.UI
{
    public class SkillsAndWeaponPanel : MonoBehaviour
    {
        private enum Tab
        {
            SKILL,
            WEAPON
        }

        [SerializeField] private NamedObjectButton m_SkillsTabButton;
        [SerializeField] private NamedObjectButton m_WeaponsTabButton;

        [SerializeField] private SkillsOverviewDisplay m_SkillsOverviewDisplay;
        [SerializeField] private WeaponsOverviewDisplay m_WeaponsOverviewDisplay;

        [Header("Tab")]
        [SerializeField] private NamedObjectButton m_ReclassButton;

        private Tab m_CurrTab;

        [HideInInspector]
        public event UnityAction OnReclassEvent;

        private void Awake()
        {
            m_SkillsTabButton.onSubmit.AddListener(() => TabSwitch(Tab.SKILL));
            m_WeaponsTabButton.onSubmit.AddListener(() => TabSwitch(Tab.WEAPON));
            m_ReclassButton.onSubmit.AddListener(OnReclassEvent);
        }

        public void ViewUnit(PlayerCharacterData playerCharacterData)
        {
            m_SkillsOverviewDisplay.DisplayUnitSkills(playerCharacterData);
            m_WeaponsOverviewDisplay.DisplayUnitWeapons(playerCharacterData);

            UpdateTabDisplay(Tab.SKILL);
        }

        private void TabSwitch(Tab tab)
        {
            if (m_CurrTab == tab)
                return;
            
            UpdateTabDisplay(tab);
        }

        private void UpdateTabDisplay(Tab tab)
        {
            if (tab == Tab.SKILL)
            {
                m_SkillsOverviewDisplay.Show();
                m_WeaponsOverviewDisplay.Hide();
            }
            else
            {
                m_SkillsOverviewDisplay.Hide();
                m_WeaponsOverviewDisplay.Show();
            }
            
            m_SkillsTabButton.SetGlowActive(tab == Tab.SKILL);
            m_WeaponsTabButton.SetGlowActive(tab == Tab.WEAPON);

            m_CurrTab = tab;
        }

        /*
        // Temporary method to pass party members information to weapons overview display
        public void SetPartyMembers(List<PlayerCharacterData> partyMembers)
        {
            m_WeaponsOverviewDisplay.SetPartyMembers(partyMembers);
        }
        */
    }
}
