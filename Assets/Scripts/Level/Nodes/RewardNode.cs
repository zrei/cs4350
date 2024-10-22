using UnityEngine;

public class RewardNode : NodeInternal
{
    [SerializeField] private RewardType m_RewardType;
    public RewardType RewardType => m_RewardType;
    
    // Main currency (STC)
    [SerializeField] private int m_RationReward;
    public int RationReward => m_RationReward;
    
    [SerializeField] private WeaponInstanceSO m_WeaponReward;
    public WeaponInstanceSO WeaponReward => m_WeaponReward;
    
    public override void StartNodeEvent()
    {
        Debug.Log("Starting Reward Node");
        GlobalEvents.Level.RewardNodeStartEvent?.Invoke(this);
    }
}
