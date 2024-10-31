using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class PartySelectScreen : BaseUIScreen
    {
        [Header("Party Slots")]
        [SerializeField] List<PartySelectionSlotButton> m_PartySelectionSlotButtons;

        [Header("Character Buttons")]
        [SerializeField] NamedObjectButton m_CharacterButton;
        [SerializeField] Transform m_CharacterButtonParent;

        [Space]
        [SerializeField] TextMeshProUGUI m_PartyText;
        [SerializeField] NamedObjectButton m_BeginLevelButton;

        private PartySelectionSlotButton m_SelectedSlot = null;
        private List<int> m_SelectedData;

        private int m_LevelId;
        private int m_PartyLimit;
        private int m_NumUnitsSelected = 0;

        private const string PARTY_TEXT_FORMAT = "Party ({0}/{1})";
        private const string PARTY_TEXT_MAX_FORMAT = "Party <color=red>({0}/{1})</color>";

        #region Initialise
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
        #endregion

        #region Load Level
        private void OnBeginLevel()
        {
            if (m_NumUnitsSelected <= 0)
                return;

            UIScreenManager.Instance.CloseScreen();
            GlobalEvents.WorldMap.OnBeginLoadLevelEvent?.Invoke();
            IEnumerable<int> selectedIds = m_SelectedData.Where(x => x != -1);
            GameSceneManager.Instance.LoadLevelScene(m_LevelId, CharacterDataManager.Instance.RetrieveCharacterData(selectedIds));
        }
        #endregion

        #region Display
        private void OnPartySelect(LevelSO levelSO)
        {
            ResetButtons();
            m_SelectedData = new();
            m_PartyLimit = levelSO.m_UnitLimit;
            m_LevelId = levelSO.m_LevelId;

            // exclude the lord because they cannot be swapped out
            foreach (PlayerCharacterData data in CharacterDataManager.Instance.RetrieveAllCharacterData(excludeLord: true))
            {
                InstantiateCharacterButton(data);
            }

            PartySelectionSlotButton firstButton = null;
            for (int i = 0; i < m_PartySelectionSlotButtons.Count; ++i)
            {
                // also instantiate slot
                PartySelectionSlotButton partySelectionSlotButton = m_PartySelectionSlotButtons[i];
                partySelectionSlotButton.Initialise(i, () => OnSelectPartySlot(partySelectionSlotButton), () => OnRemovePartySlot(partySelectionSlotButton));

                if (i < m_PartyLimit)
                {
                    partySelectionSlotButton.SetEmpty();
                    m_SelectedData.Add(-1);
                }   
                else
                    partySelectionSlotButton.SetLocked();
                

                if (i == 0)
                    firstButton = partySelectionSlotButton;
            }

            // if there is a lord, always fill the first slot with the lord
            // and lock interaction as it cannot be removed from the party
            if (CharacterDataManager.Instance.TryRetrieveLordCharacterData(out PlayerCharacterData lordData))
            {
                m_SelectedData[0] = lordData.Id;
                firstButton.SetDisplay(lordData, true);
                m_NumUnitsSelected = 1;
            }
            else
            {
                m_NumUnitsSelected = 0;
            }

            UpdateState();
        }

        private void UpdateState()
        {
            if (m_NumUnitsSelected == m_PartyLimit)
                m_PartyText.text = string.Format(PARTY_TEXT_MAX_FORMAT, m_NumUnitsSelected, m_PartyLimit);
            else
                m_PartyText.text = string.Format(PARTY_TEXT_FORMAT, m_NumUnitsSelected, m_PartyLimit);
        }
        #endregion

        #region Callback
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
                UpdateState();
                m_BeginLevelButton.interactable = m_NumUnitsSelected > 0;
            }

            m_SelectedSlot.SetDisplay(playerCharacterData, false);
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
            UpdateState();
            m_BeginLevelButton.interactable = m_NumUnitsSelected > 0;
            partySelectionSlotButton.SetEmpty();
        }
        #endregion

        #region Helper
        private void InstantiateCharacterButton(PlayerCharacterData playerCharacterData)
        {
            NamedObjectButton characterButton = Instantiate(m_CharacterButton, m_CharacterButtonParent);
            characterButton.nameText.text = playerCharacterData.m_BaseData.m_CharacterName;
            characterButton.onSubmit.RemoveAllListeners();
            characterButton.onSubmit.AddListener(() => OnSelectCharacterButton(characterButton, playerCharacterData));
        }

        private void ResetButtons()
        {
            foreach (Transform child in m_CharacterButtonParent)
            {
                Destroy(child.gameObject);
            }

            m_SelectedSlot = null;
        }
        #endregion

        public override void ScreenUpdate()
        {
            // pass
        }
    }
}
