using UnityEngine;

public class RewardNode : NodeInternal
{
    // Main currency (STC)
    [SerializeField] private int m_goldReward;
    
    // Secondary currency (STC)
    [SerializeField] private int m_manaStoneReward;
    
    public override void StartNodeEvent()
    {
        Debug.Log("Starting Reward Node");
        GlobalEvents.Level.RewardNodeStartEvent?.Invoke(this);
    }
}
