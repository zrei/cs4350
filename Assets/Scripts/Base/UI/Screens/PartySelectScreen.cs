using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class PartySelectScreen : BaseUIScreen
    {
        [SerializeField] PartySelectionSlotButton m_PartySelectionSlotButton;
        [SerializeField] NamedObjectButton m_CharacterButton;
        [SerializeField] Transform m_PartySelectionSlotParent;
        [SerializeField] Transform m_CharacterButtonParent;
        [SerializeField] TextMeshProUGUI m_PartyText;
        [SerializeField] NamedObjectButton m_BeginLevelButton;

        private PartySelectionSlotButton m_SelectedSlot = null;
        private List<int> m_SelectedData;

        private int m_LevelId;
        private int m_PartyLimit;
        private int m_NumUnitsSelected = 0;

        private const string PARTY_TEXT_FORMAT = "Party ({0}/{1})";

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.WorldMap.OnPartySelectEvent += OnPartySelect;
            
        }

        protected override void ShowDone()
        {
            base.ShowDone();

            m_BeginLevelButton.onSubmit.AddListener(OnBeginLevel);
        }

        public override void Hide()
        {
            base.Hide();

            m_BeginLevelButton.onSubmit.RemoveListener(OnBeginLevel);
        }

        private void OnDestroy()
        {
            GlobalEvents.WorldMap.OnPartySelectEvent -= OnPartySelect;
        }

        private void OnBeginLevel()
        {
            if (m_NumUnitsSelected <= 0)
                return;

            UIScreenManager.Instance.CloseScreen();
            GlobalEvents.WorldMap.OnBeginLoadLevelEvent?.Invoke();
            IEnumerable<int> selectedIds = m_SelectedData.Where(x => x != -1);
            GameSceneManager.Instance.LoadLevelScene(m_LevelId, CharacterDataManager.Instance.RetrieveCharacterData(selectedIds));
        }

        private void OnPartySelect(LevelSO levelSO)
        {
            ResetButtons();
            m_SelectedData = new();
            m_PartyLimit = levelSO.m_UnitLimit;
            m_LevelId = levelSO.m_LevelId;
            m_NumUnitsSelected = 0;
            m_BeginLevelButton.interactable = false;
            m_PartyText.text = string.Format(PARTY_TEXT_FORMAT, m_NumUnitsSelected, m_PartyLimit);

            foreach (PlayerCharacterData data in CharacterDataManager.Instance.RetrieveAllCharacterData())
            {
                InstantiateCharacterButton(data);
            }

            for (int i = 0; i < m_PartyLimit; ++i)
            {
                // also instantiate slot
                PartySelectionSlotButton partySelectionSlotButton = Instantiate(m_PartySelectionSlotButton, m_PartySelectionSlotParent);
                partySelectionSlotButton.Initialise(i, () => OnSelectPartySlot(partySelectionSlotButton), () => OnRemovePartySlot(partySelectionSlotButton));
                partySelectionSlotButton.SetEmpty();
                m_SelectedData.Add(-1);
            }
        }

        private void InstantiateCharacterButton(PlayerCharacterData playerCharacterData)
        {
            NamedObjectButton characterButton = Instantiate(m_CharacterButton, m_CharacterButtonParent);
            characterButton.nameText.text = playerCharacterData.m_BaseData.m_CharacterName;
            characterButton.onSubmit.RemoveAllListeners();
            characterButton.onSubmit.AddListener(() => OnSelectCharacterButton(characterButton, playerCharacterData));
        }

        private void OnSelectCharacterButton(NamedObjectButton button, PlayerCharacterData playerCharacterData)
        {
            if (m_SelectedSlot == null)
                return;

            Destroy(button.gameObject);

            if (m_SelectedSlot.IsFilled)
            {
                InstantiateCharacterButton(CharacterDataManager.Instance.RetrieveCharacterData(m_SelectedSlot.CharacterId));
            }
            else
            {
                ++m_NumUnitsSelected;
                m_PartyText.text = string.Format(PARTY_TEXT_FORMAT, m_NumUnitsSelected, m_PartyLimit);
                m_BeginLevelButton.interactable = m_NumUnitsSelected > 0;
            }

            m_SelectedSlot.SetDisplay(playerCharacterData);
            m_SelectedData[m_SelectedSlot.Index] = playerCharacterData.m_BaseData.m_Id;
        }

        private void OnSelectPartySlot(PartySelectionSlotButton selectedSlot)
        {
            if (selectedSlot == m_SelectedSlot)
            {
                selectedSlot.SetSelected(false);
                m_SelectedSlot = null;
                return;
            }

            if (m_SelectedSlot != null)
            {
                m_SelectedSlot.SetSelected(false);
            }

            m_SelectedSlot = selectedSlot;
            m_SelectedSlot.SetSelected(true);
        }

        private void OnRemovePartySlot(PartySelectionSlotButton partySelectionSlotButton)
        {
            m_SelectedData[partySelectionSlotButton.Index] = -1;
            InstantiateCharacterButton(CharacterDataManager.Instance.RetrieveCharacterData(partySelectionSlotButton.CharacterId));
            --m_NumUnitsSelected;
            m_PartyText.text = string.Format(PARTY_TEXT_FORMAT, m_NumUnitsSelected, m_PartyLimit);
            m_BeginLevelButton.interactable = m_NumUnitsSelected > 0;
            partySelectionSlotButton.SetEmpty();
        }

        private void ResetButtons()
        {
            foreach (Transform child in m_PartySelectionSlotParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in m_CharacterButtonParent)
            {
                Destroy(child.gameObject);
            }

            m_SelectedSlot = null;
        }

        public override void ScreenUpdate()
        {
            // pass
        }
    }
}
