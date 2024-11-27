using Game.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class RewardNodeResultScreen : BaseUIScreen
    {
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] SelectableBase m_ReturnButton;

        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            ShowRewardNode((RewardNode) args[0]);

            base.Show();
        }

        private void ShowRewardNode(RewardNode rewardNode)
        {
            if (rewardNode.RewardType == RewardType.RATION)
            {
                var rationReward = rewardNode.RationReward;
                m_ResultText.text = $"Gained {rationReward} rations!";
            }
            else if (rewardNode.RewardType == RewardType.WEAPON)
            {
                var weaponReward = rewardNode.WeaponReward;
                m_ResultText.text = $"Gained {weaponReward.m_WeaponName}!";
            }
        
            m_ReturnButton.onSubmit.AddListener(CloseResults);
        }
    
        private void CloseResults()
        {
            UIScreenManager.Instance.CloseScreen();
            m_ReturnButton.onSubmit.RemoveListener(CloseResults);
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
