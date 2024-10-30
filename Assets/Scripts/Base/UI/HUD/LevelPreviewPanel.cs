using Game.Input;
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

        private LevelSO m_CurrentLevelSO;

        #region Initialisation
        private void Awake()
        {
            GlobalEvents.WorldMap.OnGoToLevel += OnGoToLevel;
            GlobalEvents.WorldMap.OnBeginLoadLevelEvent += OnBeginLoadLevel;
            ToggleShown(true);
        }

        private void OnDestroy()
        {
            GlobalEvents.WorldMap.OnGoToLevel -= OnGoToLevel;
        }
        #endregion
        
        #region Transition
        private void OpenPartySelect(LevelSO levelSO)
        {
            CloseScreen();
            InputManager.Instance.CancelInput.OnPressEvent += OnPartySelectCancel;
            UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.PartySelectScreen);
            GlobalEvents.WorldMap.OnPartySelectEvent?.Invoke(levelSO);
            //GlobalEvents.WorldMap.OnBeginLoadLevelEvent?.Invoke();
            //GameSceneManager.Instance.LoadLevelScene(levelId, CharacterDataManager.Instance.RetrieveAllCharacterData());
        }

        private void OnBeginLoadLevel()
        {
            InputManager.Instance.CancelInput.OnPressEvent -= OnPartySelectCancel;
        }
        #endregion

        #region Helper

        private void OnPartySelectCancel(IInput input)
        {
            ToggleShown(true);
            m_StartLevelButton.onSubmit.AddListener(() => OpenPartySelect(m_CurrentLevelSO));
        }

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

            ToggleShown(true);
        }
        #endregion
    }
}
