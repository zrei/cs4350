using System.Linq;
using Game.UI;
using UnityEngine;

[System.Serializable]
public struct LevelButtonData
{
    public int levelId;
    public NamedObjectButton button;
    public FlagCondition[] conditions;
    
    public bool IsConditionsSatisfied()
    {
        if (conditions == null) return true;
        return conditions.All(flagCondition => {
            if (!FlagManager.IsReady) return true;
            return FlagManager.Instance.GetFlagValue(flagCondition.flagName) == flagCondition.flagValue;
        });
    }
}

// Temp menu for accessing multiple levels from world map
public class UI_WorldMapMenu : MonoBehaviour
{
    [SerializeField] LevelButtonData[] m_LevelButtons;
    
    private CanvasGroup m_CanvasGroup;

    private void Awake()
    {
        foreach (LevelButtonData levelButtonData in m_LevelButtons)
        {
            levelButtonData.button.onSubmit.AddListener(() => LoadLevel(levelButtonData.levelId));
            levelButtonData.button.interactable = levelButtonData.IsConditionsSatisfied();
        }
        
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

        foreach (var levelButtonData in m_LevelButtons)
        {
            levelButtonData.button.interactable = levelButtonData.IsConditionsSatisfied();
        }
    }
    
    private void Hide()
    {
        m_CanvasGroup.alpha = 0;
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
    }
}