using TMPro;
using UnityEngine;

public class LevelTimerVisual : MonoBehaviour
{
    LevelTimerLogic m_LevelTimerLogic;
    
    [SerializeField] private TextMeshPro m_TimeRemainingText;
    
    #region Initialisation
    
    public void Initialise(LevelTimerLogic levelTimerLogic)
    {
        m_LevelTimerLogic = levelTimerLogic;
        GlobalEvents.Level.TimeRemainingUpdatedEvent += OnTimeRemainingUpdate;
        
        SetTimeRemainingText(m_LevelTimerLogic.TimeRemaining);
    }
    
    #endregion
    
    #region Callbacks
    
    private void OnTimeRemainingUpdate(float timeRemaining)
    {
        SetTimeRemainingText(m_LevelTimerLogic.TimeRemaining);
    }
    
    #endregion
    
    
    #region Graphics
    
    private void SetTimeRemainingText(float timeRemaining)
    {
        m_TimeRemainingText.text = "Time Remaining: " + timeRemaining.ToString("F2");
    }
    
    #endregion
}
