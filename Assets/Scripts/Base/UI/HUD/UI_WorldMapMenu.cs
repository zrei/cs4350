using Game.UI;
using UnityEngine;

// Temp menu for accessing multiple levels from world map
public class UI_WorldMapMenu : MonoBehaviour
{
    [SerializeField] NamedObjectButton m_Level1Button;
    [SerializeField] NamedObjectButton m_Level2Button;
    
    private CanvasGroup m_CanvasGroup;

    private void Awake()
    {
        m_Level1Button.onSubmit.AddListener(() => LoadLevel(0));
        m_Level2Button.onSubmit.AddListener(() => LoadLevel(1));
        m_Level2Button.interactable = false;
        
        m_CanvasGroup = GetComponent<CanvasGroup>();
        Show();
        
        GlobalEvents.Scene.LevelSceneLoadedEvent += OnLevelSceneLoaded;
    }
    
    private void LoadLevel(int levelId)
    {
        GameSceneManager.Instance.LoadLevelScene(levelId, CharacterDataManager.Instance.RetrieveAllCharacterData());
        GlobalEvents.Level.ReturnFromLevelEvent += OnReturnFromLevel;
        
        void OnReturnFromLevel()
        {
            GlobalEvents.Level.ReturnFromLevelEvent -= OnReturnFromLevel;
            
            Show();
        }
    }
    
    private void OnLevelSceneLoaded()
    {
        Hide();
    }

    private void Show()
    {
        m_CanvasGroup.alpha = 1;
        m_CanvasGroup.interactable = true;
        m_CanvasGroup.blocksRaycasts = true;
        
        m_Level2Button.interactable = FlagManager.Instance.GetFlagValue("Level1Complete");
    }
    
    private void Hide()
    {
        m_CanvasGroup.alpha = 0;
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
    }
}