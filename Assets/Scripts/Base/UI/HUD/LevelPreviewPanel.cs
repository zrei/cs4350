using TMPro;
using UnityEngine;

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
    // TODO: Link it back to the UI Manager
    public class LevelPreviewPanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_LevelTitleText;
        [SerializeField] TextMeshProUGUI m_LevelSynopsisText;
        [SerializeField] NamedObjectButton m_StartLevelButton;
        [SerializeField] CanvasGroup m_CanvasGroup;

        private const string LEVEL_TITLE_FORMAT = "Level {0}: {1}";

        #region Initialisation
        private void Awake()
        {
            GlobalEvents.WorldMap.OnGoToLevel += OnGoToLevel;
            ToggleShown(true);
        }

        private void OnDestroy()
        {
            GlobalEvents.WorldMap.OnGoToLevel -= OnGoToLevel;
        }
        #endregion
        
        #region Transition
        private void LoadLevel(int levelId)
        {
            CloseScreen();
            GlobalEvents.WorldMap.OnBeginLoadLevelEvent?.Invoke();
            GameSceneManager.Instance.LoadLevelScene(levelId, CharacterDataManager.Instance.RetrieveAllCharacterData());
        }
        #endregion

        #region Helper
        private void CloseScreen()
        {
            ToggleShown(false);
            m_StartLevelButton.onSubmit.RemoveAllListeners();
        }
        #endregion

        #region Display
        private void ToggleShown(bool show)
        {
            m_CanvasGroup.alpha = show ? 1f : 0f;
            m_CanvasGroup.interactable = show;
            m_CanvasGroup.blocksRaycasts = show;
        }

        private void OnGoToLevel(LevelData levelData)
        {
            m_LevelTitleText.text = string.Format(LEVEL_TITLE_FORMAT, levelData.m_LevelSO.m_LevelNum, levelData.m_LevelSO.m_LevelName);
            m_LevelSynopsisText.text = levelData.m_LevelSO.m_LevelDescription;

            m_StartLevelButton.onSubmit.RemoveAllListeners();

            if (levelData.m_IsCleared)
            {
                m_StartLevelButton.interactable = false;
                m_StartLevelButton.nameText.text = "CLEARED";
            }
            else
            {
                m_StartLevelButton.nameText.text = "START";
                m_StartLevelButton.interactable = true;
                m_StartLevelButton.onSubmit.AddListener(() => LoadLevel(levelData.m_LevelSO.m_LevelId));
            }

            ToggleShown(true);
        }
        #endregion
    }
}
