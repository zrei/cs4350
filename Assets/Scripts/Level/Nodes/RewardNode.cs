using Game.UI;
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
    
    protected override void PerformNode(VoidEvent postEvent = null)
    {
        Debug.Log("Starting Reward Node");

        IUIScreen rewardNodeResultScreen = UIScreenManager.Instance.RewardNodeResultScreen;
        rewardNodeResultScreen.OnHideDone += PostResultScreen;

        GlobalEvents.Level.RewardNodeStartEvent?.Invoke(this);

        void PostResultScreen(IUIScreen screen)
        {
            screen.OnHideDone -= PostResultScreen;
            if (!m_HasPlayedPostTutorial)
            {
                m_HasPlayedPostTutorial = true;
                PlayTutorial(m_PostTutorial, null);
            }
        }
    }
}
