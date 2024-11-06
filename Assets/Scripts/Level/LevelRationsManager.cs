using System.Collections.Generic;
using UnityEngine;

public class LevelRationsManager : MonoBehaviour
{
    [Tooltip("List of thresholds that, once rations falls below, will have an effect. Only the lowest fulfilled threshold takes effect.")]
    public List<RationsThreshold> m_LessThanRationsThresholds;
    
    private float m_StartingRations;
    public float StartingRations => m_StartingRations;
    private float m_CurrRations;
    public float CurrRations => m_CurrRations;
    
    public const float MIN_RATIONS = -99;
    
    #region Initialisation
    
    public void Initialise(float startingRations)
    {
        m_StartingRations = startingRations;
        m_CurrRations = startingRations;
        
        GlobalEvents.Rations.RationsSetEvent += SetRations;
        GlobalEvents.Rations.RationsChangeEvent += ChangeRations;
    }
    
    private void OnDestroy()
    {
        GlobalEvents.Rations.RationsSetEvent -= SetRations;
        GlobalEvents.Rations.RationsChangeEvent -= ChangeRations;
    }
    
    #endregion
    
    #region Rations
    
    private void SetRations(float newRations)
    {
        m_CurrRations = newRations;
    }
    
    private void ChangeRations(float changeAmount)
    {
        m_CurrRations += changeAmount;
        m_CurrRations = Mathf.Clamp(m_CurrRations, MIN_RATIONS, m_CurrRations);
    }
    
    #endregion

    #region Tokens

    public List<InflictedToken> GetInflictedTokens()
    {
        foreach (RationsThreshold rationsThreshold in m_LessThanRationsThresholds)
        {
            if (rationsThreshold.IsThresholdMet(m_CurrRations, true))
                return rationsThreshold.m_Tokens;
        }
        return new List<InflictedToken>();
    }

    #endregion
}

[System.Serializable]
public struct RationsThreshold
{
    public float m_Threshold;

    public List<InflictedToken> m_Tokens;

    /// <summary>
    /// Checks whether the threshold has been met
    /// </summary>
    /// <param name="currRations"></param>
    /// <param name="lessThan">Whether to check for less than the threshold or greater than the threshold</param>
    /// <returns></returns>
    public bool IsThresholdMet(float currRations, bool lessThan)
    {
        if (lessThan && currRations <= m_Threshold)
            return true;
        else if (!lessThan && currRations >= m_Threshold)
            return true;
        return false;
    }
}
