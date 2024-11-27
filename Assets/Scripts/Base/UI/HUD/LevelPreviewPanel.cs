using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct LevelData 
{
    public LevelSO m_LevelSO;
    public bool m_IsCleared;

    public LevelData(LevelSO levelSO, bool isCleared)
    {
        m_LevelSO = levelSO;
        m_IsCleared = isCleared;
    }
}

namespace Game.UI
{
    [DefaultExecutionOrder(-1)]
    // TODO: Link it back to the UI Manager
    public class LevelPreviewPanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_LevelTitleText;
        [SerializeField] TextMeshProUGUI m_LevelSynopsisText;
        [SerializeField] NamedObjectButton m_StartLevelButton;
        [SerializeField] CanvasGroup m_CanvasGroup;

        private const string LEVEL_TITLE_FORMAT = "Level {0}:\n{1}";

        private LevelSO m_CurrentLevelSO;

        #region Initialisation
        private void Awake()
        {
            GlobalEvents.WorldMap.OnGoToLevel += OnGoToLevel;
        }

        private void OnDestroy()
        {
            GlobalEvents.WorldMap.OnGoToLevel -= OnGoToLevel;
        }
        #endregion
        
        #region Transition
        private void OpenPartySelect(LevelSO levelSO)
        {
            if (levelSO.m_ShowCharacterSelectScreen)
                UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.PartySelectScreen, false, levelSO);
            else
            {
                List<PlayerCharacterData> characterData = new();
                if (CharacterDataManager.Instance.TryRetrieveLordCharacterData(out PlayerCharacterData lordData))
                    characterData.Add(lordData);
                characterData.AddRange(CharacterDataManager.Instance.RetrieveCharacterData(levelSO.m_LockedInCharacters.Select(x => x.m_Id), true));
                SaveManager.Instance.Save(() => GameSceneManager.Instance.LoadLevelScene(levelSO.m_LevelId, characterData));
            }
        }
        #endregion

        #region Display
        private void OnGoToLevel(LevelData levelData)
        {
            m_LevelTitleText.text = string.Format(LEVEL_TITLE_FORMAT, levelData.m_LevelSO.m_LevelNum, levelData.m_LevelSO.m_LevelName);
            m_LevelSynopsisText.text = levelData.m_LevelSO.m_LevelDescription;

            m_StartLevelButton.onSubmit.RemoveAllListeners();

            m_CurrentLevelSO = levelData.m_LevelSO;

            if (levelData.m_IsCleared)
            {
                m_StartLevelButton.interactable = false;
                m_StartLevelButton.nameText.text = "CLEARED";
            }
            else
            {
                m_StartLevelButton.nameText.text = "START";
                m_StartLevelButton.interactable = true;
                m_StartLevelButton.onSubmit.AddListener(() => OpenPartySelect(m_CurrentLevelSO));
            }
        }
        #endregion
    }
}
