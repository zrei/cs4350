using UnityEngine;

public class RewardNode : LevelNodeInternal
{
    // Main currency (STC)
    [SerializeField] private int m_RationReward;
    public int RationReward => m_RationReward;
    
    public override void StartNodeEvent()
    {
        Debug.Log("Starting Reward Node");
        GlobalEvents.Level.RewardNodeStartEvent?.Invoke(this);
    }
}
