using Game.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class RewardNodeResultScreen : BaseUIScreen
    {
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] Button m_ReturnButton;

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.Level.RewardNodeStartEvent += OnRewardNodeStart;
        }

        private void OnDestroy()
        {
            GlobalEvents.Level.RewardNodeStartEvent -= OnRewardNodeStart;
        }

        
        private void OnRewardNodeStart(RewardNode rewardNode)
        {
            if (rewardNode.RewardType == RewardType.TIME)
            {
                var rationReward = rewardNode.RationReward;
                m_ResultText.text = $"Gained {rationReward} rations!";
            }
            else if (rewardNode.RewardType == RewardType.WEAPON)
            {
                var weaponReward = rewardNode.WeaponReward;
                m_ResultText.text = $"Gained {weaponReward.m_WeaponName}!";
            }
        
            m_ReturnButton.onClick.AddListener(CloseResults);
        }
    
        private void CloseResults()
        {
            GlobalEvents.Level.CloseRewardScreenEvent?.Invoke();
            UIScreenManager.Instance.CloseScreen();
            m_ReturnButton.onClick.RemoveListener(CloseResults);
        }
        
        public override void ScreenUpdate()
        {
        }
        
        public override void OnCancel(IInput input)
        {
            CloseResults();
        }
    }
}
