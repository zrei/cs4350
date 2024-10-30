using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UI 
{
    public class PartySelectionSlotButton : MonoBehaviour
    {
        [SerializeField] NamedObjectButton m_SelectionButton;
        [SerializeField] NamedObjectButton m_RemoveButton;

        public int Index {get; private set;}

        public int CharacterId {get; private set;}

        public bool IsFilled {get; private set;}

        public void Initialise(int index, UnityAction selectionButtonCallback, UnityAction removeButtonCallback)
        {
            Index = index;

            m_SelectionButton.onSubmit.RemoveAllListeners();
            m_SelectionButton.onSubmit.AddListener(selectionButtonCallback);
            
            m_RemoveButton.onSubmit.RemoveAllListeners();
            m_RemoveButton.onSubmit.AddListener(removeButtonCallback);
        }

        public void SetEmpty()
        {
            IsFilled = false;
            CharacterId = -1;
            m_RemoveButton.gameObject.SetActive(false);
            m_SelectionButton.nameText.text = "EMPTY";
        }

        public void SetDisplay(PlayerCharacterData playerCharacterData, bool isLord)
        {
            m_RemoveButton.gameObject.SetActive(!isLord);
            m_SelectionButton.interactable = !isLord;
            m_SelectionButton.nameText.text = playerCharacterData.m_BaseData.m_CharacterName;
            IsFilled = true;
            CharacterId = playerCharacterData.m_BaseData.m_Id;

            
        }

        public void SetSelected(bool isSelected)
        {
            m_SelectionButton.SetGlowActive(isSelected);
        }
    }
}
