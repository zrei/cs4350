using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CharacterOverviewDisplay : MonoBehaviour
    {
        #region Component References
        [SerializeField] private FormattedTextDisplay m_AttributeDisplay;

        [SerializeField] private FormattedTextDisplay m_NameDisplay;

        [SerializeField] private FormattedTextDisplay m_LevelDisplay;
        
        [SerializeField] private ProgressBar m_ExpBar;
        
        [SerializeField] private FormattedTextDisplay m_ClassDisplay;

        [SerializeField] private Image m_CharacterArt;

        [SerializeField] private FormattedTextDisplay m_EquippedWeaponDisplay;

        [SerializeField] private CharacterStatDisplay m_CharacterStatDisplay;
        #endregion
        
        private PlayerCharacterData m_PlayerUnit;

        private void Awake()
        {
            GlobalEvents.CharacterManagement.OnWeaponChangedEvent += OnWeaponChanged;
            GlobalEvents.CharacterManagement.OnPreviewReclass += OnPreviewReclass;
            GlobalEvents.CharacterManagement.OnReclass += OnReclass;
        }

        private void OnDestroy()
        {
            GlobalEvents.CharacterManagement.OnWeaponChangedEvent -= OnWeaponChanged;
            GlobalEvents.CharacterManagement.OnPreviewReclass -= OnPreviewReclass;
            GlobalEvents.CharacterManagement.OnReclass -= OnReclass;
        }

        public void ViewUnit(PlayerCharacterData playerUnit)
        {
            m_PlayerUnit = playerUnit;
            
            m_AttributeDisplay?.SetValue($"{playerUnit.m_BaseData.m_CharacterMoralityTrait.m_TraitName}");
            m_NameDisplay?.SetValue($"{playerUnit.m_BaseData.m_CharacterName}");
            m_LevelDisplay?.SetValue($"{playerUnit.m_CurrLevel}");

            if (m_ExpBar != null)
                m_ExpBar.SetValue(LevellingManager.Instance.GetProgressToNextLevel(playerUnit), 1f, 0f);

            m_CharacterArt.sprite = playerUnit.m_BaseData.m_CharacterSprite;
            var color = m_CharacterArt.color;
            color.a = playerUnit.m_BaseData.m_CharacterSprite != null ? 1 : 0;
            m_CharacterArt.color = color;

            SetWeapon(m_PlayerUnit.GetWeaponInstanceSO());

            m_CharacterStatDisplay.SetDisplay(playerUnit);

            m_ClassDisplay?.SetValue(playerUnit.m_BaseData.m_PathGroup.m_PathName, playerUnit.CurrClass.m_ClassName);
        }

        private void OnWeaponChanged()
        {
            SetWeapon(m_PlayerUnit.GetWeaponInstanceSO());
        }

        private void SetWeapon(WeaponInstanceSO weaponInstanceSO)
        {
            m_EquippedWeaponDisplay?.SetValue($"{weaponInstanceSO.m_WeaponName}");
        }

        private void OnPreviewReclass(PlayerClassSO playerClassSO)
        {
            m_CharacterStatDisplay.SetComparisonDisplay(playerClassSO);
        }

        private void OnReclass()
        {
            m_CharacterStatDisplay.SetDisplay(m_PlayerUnit);
            m_ClassDisplay?.SetValue(m_PlayerUnit.m_BaseData.m_PathGroup.m_PathName, m_PlayerUnit.CurrClass.m_ClassName);
        }
    }
    
}
