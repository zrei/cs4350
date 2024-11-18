using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI 
{
    public enum PartySelectSlotState
    {
        EMPTY,
        LOCKED,
        FILLED
    }

    public struct PartySelectionSlotUIData
    {
        public PlayerCharacterData PlayerCharacterData;
        public UnityAction InfoCallback;
        public bool IsRequiredCharacter;

        public PartySelectionSlotUIData(PlayerCharacterData playerCharacterData, UnityAction infoCallback, bool isRequiredCharacter)
        {
            PlayerCharacterData = playerCharacterData;
            InfoCallback = infoCallback;
            IsRequiredCharacter = isRequiredCharacter;
        }
    }

    public class PartySelectionSlotButton : MonoBehaviour
    {
        [SerializeField] Image m_Glow;
        [SerializeField] GraphicGroup m_GraphicGroup;
        [SerializeField] TextMeshProUGUI m_NameText;
        [SerializeField] NamedObjectButton m_RemoveButton;
        [SerializeField] NamedObjectButton m_InfoButton;

        [SerializeField] Color m_ActiveColor;
        [SerializeField] Color m_DisabledColor;

        public int Index {get; private set;}

        public int CharacterId {get; private set;}

        private PartySelectSlotState m_SlotState;

        public bool IsFilled => m_SlotState == PartySelectSlotState.FILLED;
        public bool IsEmpty => m_SlotState == PartySelectSlotState.EMPTY;

        public void Initialise(int index, UnityAction removeButtonCallback)
        {
            Index = index;
            
            m_RemoveButton.onSubmit.RemoveAllListeners();
            m_RemoveButton.onSubmit.AddListener(removeButtonCallback);
        }

        public void SetLocked()
        {
            CharacterId = -1;
            m_RemoveButton.gameObject.SetActive(false);
            m_NameText.text = "LOCKED";
            UpdateDisplay(PartySelectSlotState.LOCKED);
        }

        public void SetEmpty()
        {
            CharacterId = -1;
            m_RemoveButton.gameObject.SetActive(false);
            m_NameText.text = "EMPTY";
            UpdateDisplay(PartySelectSlotState.EMPTY);
        }

        public void SetDisplay(PartySelectionSlotUIData partySelectionSlotUIData)
        {
            m_RemoveButton.gameObject.SetActive(!partySelectionSlotUIData.IsRequiredCharacter);
            m_NameText.text = $"{partySelectionSlotUIData.PlayerCharacterData.m_BaseData.m_CharacterName} / {partySelectionSlotUIData.PlayerCharacterData.CurrClass.m_ClassName}";
            CharacterId = partySelectionSlotUIData.PlayerCharacterData.m_BaseData.m_Id;
            UpdateDisplay(PartySelectSlotState.FILLED);

            m_InfoButton.onSubmit.RemoveAllListeners();
            m_InfoButton.onSubmit.AddListener(partySelectionSlotUIData.InfoCallback);
        }

        private void SetGlow(bool isSelected)
        {
            m_Glow.CrossFadeAlpha(isSelected ? 1 : 0, 0.2f, false);
        }

        private void UpdateDisplay(PartySelectSlotState slotState)
        {
            m_SlotState = slotState;
            SetGlow(slotState == PartySelectSlotState.FILLED);
            m_GraphicGroup.CrossFadeColor(slotState == PartySelectSlotState.LOCKED ? m_DisabledColor : m_ActiveColor, 0f, true, true);
            m_InfoButton.gameObject.SetActive(slotState == PartySelectSlotState.FILLED);
        }
    }
}
