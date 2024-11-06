using UnityEngine;

public class LevelRationsManager : MonoBehaviour
{
    private float m_StartingRations;
    public float StartingRations => m_StartingRations;
    private float m_CurrRations;
    public float CurrRations => m_CurrRations;
    
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
    }
    
    #endregion
}
