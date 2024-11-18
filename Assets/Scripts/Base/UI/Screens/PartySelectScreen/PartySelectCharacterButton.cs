using UnityEngine;
using UnityEngine.Events;

namespace Game.UI
{
    public struct PartySelectCharacterButtonUIData
    {
        public string CharacterName;
        public string CharacterClass;
        public UnityAction SelectionCallback;
        public UnityAction InfoCallback;

        public PartySelectCharacterButtonUIData(string characterName, string characterClass,
            UnityAction selectionCallback, UnityAction infoCallback)
        {
            CharacterName = characterName;
            CharacterClass = characterClass;
            SelectionCallback = selectionCallback;
            InfoCallback = infoCallback;
        }
    }

    public class PartySelectCharacterButton : MonoBehaviour
    {
        [SerializeField] private NamedObjectButton m_SelectionButton;
        [SerializeField] private NamedObjectButton m_InfoButton;

        public void SetCharacter(PartySelectCharacterButtonUIData partySelectCharacterButtonUIData)
        {
            m_SelectionButton.onSubmit.RemoveAllListeners();
            m_SelectionButton.onSubmit.AddListener(partySelectCharacterButtonUIData.SelectionCallback);
            m_SelectionButton.nameText.text = $"{partySelectCharacterButtonUIData.CharacterName} / {partySelectCharacterButtonUIData.CharacterClass}";
            m_InfoButton.onSubmit.RemoveAllListeners();
            m_InfoButton.onSubmit.AddListener(partySelectCharacterButtonUIData.InfoCallback);
        }
    }
}
