using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI
{
    public class WeaponsOverviewDisplay : MonoBehaviour
    {
        
        public UnityEvent OnWeaponChanged = new UnityEvent();

        #region Component References
        private CanvasGroup canvasGroup;
        
        [SerializeField]
        private List<NamedObjectButton> weaponButtons = new();

        [SerializeField] 
        private FormattedTextDisplay weaponHeaderText;

        [SerializeField] 
        private FormattedTextDisplay weaponDescriptionText;
        
        [SerializeField]
        private Button equipButton;
        
        [SerializeField]
        private TextMeshProUGUI equipButtonText;
        
        #endregion

        private List<PlayerCharacterData> m_PartyMembers = new();
        private PlayerCharacterData m_PlayerUnit;
        private List<WeaponInstance> m_AvailableWeapons;

        public WeaponInstance SelectedWeapon
        {
            set
            {
                if (m_SelectedWeapon == value) return;

                m_SelectedWeapon = value;
                UpdateWeaponDisplay(m_SelectedWeapon);
            }
        }
        private WeaponInstance m_SelectedWeapon;

        public NamedObjectButton SelectedWeaponButton
        {
            set
            {
                if (m_SelectedWeaponButton == value) return;
                
                if (m_SelectedWeaponButton != null)
                {
                    m_SelectedWeaponButton.SetGlowActive(false);
                }
                
                m_SelectedWeaponButton = value;
                if (m_SelectedWeaponButton != null)
                {
                    m_SelectedWeaponButton.SetGlowActive(true);
                }
            }
        }
        private NamedObjectButton m_SelectedWeaponButton;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Hide();
            
            for (int i = 0; i < weaponButtons.Count; i++)
            {
                var button = weaponButtons[i];
                var index = i;
                button.onSubmit.AddListener(() => OnSelectWeapon(index));
            }
            
            equipButton.onClick.AddListener(ToggleWeaponEquip);
        }

        public void DisplayUnitWeapons(PlayerCharacterData playerUnit)
        {
            // Reset Display
            SelectedWeapon = null;
            SelectedWeaponButton = null;
            UpdateWeaponDisplay(null);
            
            m_PlayerUnit = playerUnit;
            var unitWeaponType = playerUnit.CurrClass.m_WeaponType.m_WeaponType;
            m_AvailableWeapons = InventoryManager.Instance.RetrieveWeaponsOfType(unitWeaponType);
            
            for (int i = 0; i < weaponButtons.Count; i++)
            {
                if (i < m_AvailableWeapons.Count)
                {
                    var weapon = m_AvailableWeapons[i];
                        
                    weaponButtons[i].gameObject.SetActive(true);
                    weaponButtons[i].SetGlowActive(false);
                    weaponButtons[i].SetObjectName(weapon.m_WeaponInstanceSO.m_WeaponName);
                }
                else
                {
                    weaponButtons[i].gameObject.SetActive(false);
                }
            }
            
            if (m_AvailableWeapons.Count == 0)
            {
                weaponDescriptionText.SetValue("No weapons available");
            }
            
            UpdateEquippedWeaponButtonText();
        }

        private void OnSelectWeapon(int index)
        {
            SelectedWeapon = m_AvailableWeapons[index];
            SelectedWeaponButton = weaponButtons[index];
        }

        private void ToggleWeaponEquip()
        {
            if (m_SelectedWeapon == null) return;
            
            if (m_PlayerUnit.m_CurrEquippedWeaponId == m_SelectedWeapon.m_InstanceId)
            {
                // Unequipping weapon
                m_PlayerUnit.m_CurrEquippedWeaponId = null;
                InventoryManager.Instance.ChangeWeaponEquipStatus(m_SelectedWeapon.m_InstanceId, false);
            }
            else 
            {
                // Unequip selected weapon from previous unit
                if (m_SelectedWeapon.m_IsEquipped)
                {
                    foreach (var partyMember in m_PartyMembers)
                    {
                        if (partyMember.m_CurrEquippedWeaponId == m_SelectedWeapon.m_InstanceId)
                        {
                            partyMember.m_CurrEquippedWeaponId = null;
                        }
                    }
                }
                
                // Unequip current weapon
                if (m_PlayerUnit.m_CurrEquippedWeaponId != null)
                {
                    InventoryManager.Instance.ChangeWeaponEquipStatus(m_PlayerUnit.m_CurrEquippedWeaponId.Value, false);
                }
                
                // Equip new weapon
                m_PlayerUnit.m_CurrEquippedWeaponId = m_SelectedWeapon.m_InstanceId;
                InventoryManager.Instance.ChangeWeaponEquipStatus(m_SelectedWeapon.m_InstanceId, true);
            }
            
            UpdateWeaponDisplay(m_SelectedWeapon);
            UpdateEquippedWeaponButtonText();
            OnWeaponChanged.Invoke();
        }

        private void UpdateWeaponDisplay(WeaponInstance weapon)
        {
            if (weapon == null)
            {
                weaponHeaderText.SetValue(string.Empty);
                weaponDescriptionText.SetValue(string.Empty);
                equipButtonText.text = "Equip";
                equipButton.interactable = false;
                return;
            }

            weaponHeaderText.SetValue(weapon.m_WeaponInstanceSO.m_WeaponName);
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"Weapon Type: {weapon.m_WeaponInstanceSO.m_WeaponType.ToString()}");
            builder.AppendLine($"Base Attack Modifier: {weapon.m_WeaponInstanceSO.m_BaseAttackModifier}");
            builder.AppendLine($"Base Heal Modifier: {weapon.m_WeaponInstanceSO.m_BaseHealModifier}");
            weaponDescriptionText.SetValue(builder.ToString());
            equipButton.interactable = true;
            
            equipButtonText.text = m_PlayerUnit.m_CurrEquippedWeaponId == weapon.m_InstanceId ? "Unequip" : "Equip";
        }

        // Temp method to add equipped tag to weapon buttons
        private void UpdateEquippedWeaponButtonText()
        {
            for (int i = 0; i < m_AvailableWeapons.Count; i++)
            {
                if (m_PlayerUnit.m_CurrEquippedWeaponId == m_AvailableWeapons[i].m_InstanceId)
                {
                    weaponButtons[i]
                        .SetObjectName(m_AvailableWeapons[i].m_WeaponInstanceSO.m_WeaponName + " <Equipped>");
                }
                else
                {
                    weaponButtons[i].SetObjectName(m_AvailableWeapons[i].m_WeaponInstanceSO.m_WeaponName);
                }
            }
        }

        public void Hide()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
        }
        
        public void Show()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1;
        }
        
        // Temporary method to get party members for unequipping
        public void SetPartyMembers(List<PlayerCharacterData> partyMembers)
        {
            m_PartyMembers = partyMembers;
        }
    }
}
