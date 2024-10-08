using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimerVisual : MonoBehaviour
{
    LevelTimerLogic m_LevelTimerLogic;
    
    [SerializeField] private TextMeshProUGUI m_TimeRemainingText;
    [SerializeField] private Image m_TimeRemainingFill;
    [SerializeField] private Image m_TimeRemainingBackground;
    
    #region Initialisation
    
    public void Initialise(LevelTimerLogic levelTimerLogic)
    {
        m_LevelTimerLogic = levelTimerLogic;
        GlobalEvents.Level.TimeRemainingUpdatedEvent += OnTimeRemainingUpdate;
        
        SetTimeRemainingText(m_LevelTimerLogic.TimeRemaining);
        m_TimeRemainingFill.fillAmount = 1;
        
        GlobalEvents.Level.BattleNodeStartEvent += OnBattleNodeStart;
        GlobalEvents.Battle.ReturnFromBattleEvent += OnReturnFromBattle;
        
        Show();
    }
    
    #endregion
    
    #region Callbacks
    
    private void OnTimeRemainingUpdate(float timeRemaining)
    {
        SetTimeRemainingText(m_LevelTimerLogic.TimeRemaining);
        m_TimeRemainingFill.fillAmount = m_LevelTimerLogic.TimeRemaining / m_LevelTimerLogic.TimeLimit;
    }

    private void OnBattleNodeStart(BattleNode _)
    {
        Hide();
    }

    private void OnReturnFromBattle()
    {
        Show();
    }
    
    #endregion
    
    #region Graphics
    
    private void SetTimeRemainingText(float timeRemaining)
    {
        m_TimeRemainingText.text = timeRemaining.ToString("F0");
    }

    private void Hide()
    {
        m_TimeRemainingBackground.enabled = false;
        m_TimeRemainingFill.enabled = false;
        m_TimeRemainingText.enabled = false;
    }
    
    private void Show()
    {
        m_TimeRemainingBackground.enabled = true;
        m_TimeRemainingFill.enabled = true;
        m_TimeRemainingText.enabled = true;
    }
    
    #endregion
}
