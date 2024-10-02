using UnityEngine;

public class LevelTimerLogic : MonoBehaviour
{
    private float m_TimeLimit;
    public float TimeLimit => m_TimeLimit;
    private float m_TimeRemaining;
    public float TimeRemaining => m_TimeRemaining;
    
    #region Initialisation
    
    public void Initialise(float timeLimit)
    {
        m_TimeLimit = timeLimit;
        m_TimeRemaining = timeLimit;
    }
    
    #endregion
    
    #region Timer
    
    /// <summary>
    /// Subtracts deltaTime from the time remaining
    /// </summary>
    /// <param name="deltaTime"></param>
    public void AdvanceTimer(float deltaTime)
    {
        m_TimeRemaining -= deltaTime;
        m_TimeRemaining = Mathf.Max(0, m_TimeRemaining);
        
        GlobalEvents.Level.TimeRemainingUpdatedEvent(m_TimeRemaining);
        
        if (m_TimeRemaining <= 0)
        {
            GlobalEvents.Level.LevelEndEvent(LevelResultType.OUT_OF_TIME);
        }
    }
    
    #endregion
}
