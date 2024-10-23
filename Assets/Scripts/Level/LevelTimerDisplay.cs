using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CanvasGroup))]
public class LevelTimerDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TimeRemainingText;
    [SerializeField] private Image m_TimeRemainingFill;
    
    private LevelTimerLogic m_LevelTimerLogic;
    
    private Animator m_Animator;
    private CanvasGroup m_CanvasGroup;

    private bool isHidden;
    
    #region Initialisation

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Animator.enabled = false;
        
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
        m_CanvasGroup.alpha = 0;
        
        isHidden = true;
        
        GlobalEvents.Scene.LevelSceneLoadedEvent += OnSceneLoad;
    }

    public void Initialise(LevelTimerLogic levelTimerLogic)
    {
        m_LevelTimerLogic = levelTimerLogic;
        GlobalEvents.Level.TimeRemainingUpdatedEvent += OnTimeRemainingUpdate;
        
        SetTimeRemainingText(m_LevelTimerLogic.TimeRemaining);
        m_TimeRemainingFill.fillAmount = 1;
    }
    
    #endregion
    
    #region Callbacks

    private void OnSceneLoad()
    {
        m_LevelTimerLogic = FindObjectOfType<LevelTimerLogic>();
        
        if (m_LevelTimerLogic == null)
        {
            Debug.LogError("LevelTimerVisual: LevelTimerLogic not found in scene");
            return;
        }
        
        Initialise(m_LevelTimerLogic);
        
        GlobalEvents.Level.BattleNodeStartEvent += OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent += OnBattleNodeEnd;
        GlobalEvents.Level.ReturnFromLevelEvent += Hide;
        
        Show();
    }

    private void OnDestroy()
    {
        GlobalEvents.Scene.LevelSceneLoadedEvent -= OnSceneLoad;
        GlobalEvents.Level.TimeRemainingUpdatedEvent -= OnTimeRemainingUpdate;
        GlobalEvents.Level.BattleNodeStartEvent -= OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent -= OnBattleNodeEnd;
        GlobalEvents.Level.ReturnFromLevelEvent -= Hide;
    }

    private void OnTimeRemainingUpdate(float timeRemaining)
    {
        SetTimeRemainingText(m_LevelTimerLogic.TimeRemaining);
        m_TimeRemainingFill.fillAmount = m_LevelTimerLogic.TimeRemaining / m_LevelTimerLogic.TimeLimit;
    }

    private void OnBattleNodeStart(BattleNode _)
    {
        Hide();
    }

    private void OnBattleNodeEnd(BattleNode node, UnitAllegiance _1, int _2)
    {
        Show();
    }
    
    #endregion
    
    #region Graphics
    
    private void SetTimeRemainingText(float timeRemaining)
    {
        m_TimeRemainingText.text = $"{timeRemaining:F0}<sprite name=\"Rations\" tint>";
    }

    private void Hide()
    {
        isHidden = true;
        m_Animator.enabled = true;
        m_Animator.Play(UIConstants.HideAnimHash);
    }
    
    private void Show()
    {
        isHidden = false;
        m_Animator.enabled = true;
        m_Animator.Play(UIConstants.ShowAnimHash);
    }
    
    private void OnAnimationFinish()
    {
        m_Animator.enabled = false;
        m_CanvasGroup.interactable = !isHidden;
        m_CanvasGroup.blocksRaycasts = !isHidden;
    }
    
    #endregion
}
