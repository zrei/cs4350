using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimerVisual : MonoBehaviour
{
    LevelTimerLogic m_LevelTimerLogic;
    
    [SerializeField] private TextMeshProUGUI m_TimeRemainingText;
    [SerializeField] private Image m_TimeRemainingFill;
    
    #region Initialisation
    
    public void Initialise(LevelTimerLogic levelTimerLogic)
    {
        m_LevelTimerLogic = levelTimerLogic;
        GlobalEvents.Level.TimeRemainingUpdatedEvent += OnTimeRemainingUpdate;
        
        SetTimeRemainingText(m_LevelTimerLogic.TimeRemaining);
        m_TimeRemainingFill.fillAmount = 1;
    }
    
    #endregion
    
    #region Callbacks
    
    private void OnTimeRemainingUpdate(float timeRemaining)
    {
        SetTimeRemainingText(m_LevelTimerLogic.TimeRemaining);
        m_TimeRemainingFill.fillAmount = m_LevelTimerLogic.TimeRemaining / m_LevelTimerLogic.TimeLimit;
    }
    
    #endregion
    
    
    #region Graphics
    
    private void SetTimeRemainingText(float timeRemaining)
    {
        m_TimeRemainingText.text = timeRemaining.ToString("F0");
    }
    
    #endregion
}
