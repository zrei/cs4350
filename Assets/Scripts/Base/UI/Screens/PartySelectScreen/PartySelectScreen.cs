using System.Collections;
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
        [SerializeField] private PartySelectCharacterButton m_CharacterButton;
        [SerializeField] Transform m_CharacterButtonParent;

        [Header("Tutorial")]
        [SerializeField] private List<TutorialPageUIData> m_Tutorial;

        [Space]
        [SerializeField] TextMeshProUGUI m_PartyText;
        [SerializeField] NamedObjectButton m_BeginLevelButton;

        private int m_LevelId;
        private int m_PartyLimit;
        private int m_NumUnitsSelected = 0;

        private const string PARTY_TEXT_FORMAT = "Party ({0}/{1})";
        private const string PARTY_TEXT_MAX_FORMAT = "Party <color=red>({0}/{1})</color>";

        #region Initialise
        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            ShowPartySelect((LevelSO) args[0]);
            base.Show();
        }

        protected override void ShowDone()
        {
            base.ShowDone();

            m_BeginLevelButton.onSubmit.AddListener(OnBeginLevel);

            if (!FlagManager.Instance.GetFlagValue(Flag.HAS_VISITED_PARTY_SELECT))
            {                
                StartCoroutine(ShowTutorial());
            }
        }

        private IEnumerator ShowTutorial()
        {
            yield return null;
            UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.TutorialScreen, false, m_Tutorial);
            FlagManager.Instance.SetFlagValue(Flag.HAS_VISITED_PARTY_SELECT, true, FlagType.PERSISTENT);
        }

        public override void Hide()
        {
            base.Hide();

            m_BeginLevelButton.onSubmit.RemoveListener(OnBeginLevel);
        }
        #endregion

        #region Load Level
        private void OnBeginLevel()
        {
            if (m_NumUnitsSelected <= 0)
                return;

            UIScreenManager.Instance.CloseScreen();
            IEnumerable<int> selectedIds = GetSelectedCharacterIds();
            SaveManager.Instance.Save(() => GameSceneManager.Instance.LoadLevelScene(m_LevelId, CharacterDataManager.Instance.RetrieveCharacterData(selectedIds)));
        }
        #endregion

        #region Display
        private void ShowPartySelect(LevelSO levelSO)
        {
            ResetButtons();
            m_PartyLimit = levelSO.m_UnitLimit;
            m_LevelId = levelSO.m_LevelId;

            // exclude the lord and additional locked in characters because they cannot be swapped out
            foreach (PlayerCharacterData data in CharacterDataManager.Instance.RetrieveAllCharacterData(levelSO.m_LockedInCharacters.Select(x => x.m_Id), excludeLord: true))
            {
                InstantiateCharacterButton(data);
            }

            for (int i = 0; i < m_PartySelectionSlotButtons.Count; ++i)
            {
                // also instantiate slot
                PartySelectionSlotButton partySelectionSlotButton = m_PartySelectionSlotButtons[i];
                partySelectionSlotButton.Initialise(i, () => OnRemovePartySlot(partySelectionSlotButton));

                if (i < m_PartyLimit)
                {
                    partySelectionSlotButton.SetEmpty();
                }   
                else
                    partySelectionSlotButton.SetLocked();
            }


            m_NumUnitsSelected = 0;
            List<PlayerCharacterData> lockedInCharacters = new();
            if (CharacterDataManager.Instance.TryRetrieveLordCharacterData(out PlayerCharacterData lordData))
            {
                lockedInCharacters.Add(lordData);
            }

            lockedInCharacters.AddRange(CharacterDataManager.Instance.RetrieveCharacterData(levelSO.m_LockedInCharacters.Select(x => x.m_Id), true));
            
            foreach (PlayerCharacterData playerCharacterData in lockedInCharacters)
            {
                if (!TryGetEmptySlot(out PartySelectionSlotButton partySelectionSlotButton))
                {
                    break;
                }
                
                partySelectionSlotButton.SetDisplay(new PartySelectionSlotUIData(playerCharacterData, () => DisplayPreview(playerCharacterData), true));
                ++m_NumUnitsSelected;
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
        private void OnSelectCharacterButton(PartySelectCharacterButton button, PlayerCharacterData playerCharacterData)
        {
            if (!TryGetEmptySlot(out PartySelectionSlotButton partySelectionSlotButton))
            {
                ToastNotificationManager.Instance.Show($"No empty slot", Color.red);
                return;
            }

            Destroy(button.gameObject);

            partySelectionSlotButton.SetDisplay(new PartySelectionSlotUIData(playerCharacterData, () => DisplayPreview(playerCharacterData), false));
            ++m_NumUnitsSelected;
            
            UpdateState();
        }

        private void OnRemovePartySlot(PartySelectionSlotButton partySelectionSlotButton)
        {
            InstantiateCharacterButton(CharacterDataManager.Instance.RetrieveCharacterData(partySelectionSlotButton.CharacterId));
            --m_NumUnitsSelected;
            UpdateState();
            partySelectionSlotButton.SetEmpty();
        }
        #endregion

        #region Helper
        private void InstantiateCharacterButton(PlayerCharacterData playerCharacterData)
        {
            PartySelectCharacterButton characterButton = Instantiate(m_CharacterButton, m_CharacterButtonParent);
            
            characterButton.SetCharacter(new PartySelectCharacterButtonUIData(playerCharacterData.m_BaseData.m_CharacterName, playerCharacterData.CurrClass.m_ClassName,
                () => OnSelectCharacterButton(characterButton, playerCharacterData), () => DisplayPreview(playerCharacterData)));
        }

        private void ResetButtons()
        {
            foreach (Transform child in m_CharacterButtonParent)
            {
                Destroy(child.gameObject);
            }
        }

        private bool TryGetEmptySlot(out PartySelectionSlotButton partySelectionSlotButton)
        {
            foreach (PartySelectionSlotButton slot in m_PartySelectionSlotButtons)
            {
                if (slot.IsEmpty)
                {
                    partySelectionSlotButton = slot;
                    return true;
                }
            }
            partySelectionSlotButton = default;
            return false;
        }

        private List<int> GetSelectedCharacterIds()
        {
            List<int> selectedCharacterIds = new();

            foreach (PartySelectionSlotButton partySelectionSlotButton in m_PartySelectionSlotButtons)
            {
                if (partySelectionSlotButton.IsFilled)
                    selectedCharacterIds.Add(partySelectionSlotButton.CharacterId);
            }

            return selectedCharacterIds;
        }
        #endregion

        #region Preview
        private void DisplayPreview(PlayerCharacterData playerCharacterData)
        {
            UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.PreviewScreen, false, playerCharacterData);
        }
        #endregion

        public override void ScreenUpdate()
        {
            // pass
        }
    }
}
