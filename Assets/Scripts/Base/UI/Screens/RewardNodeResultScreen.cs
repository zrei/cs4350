using System.Text;
using Game.Input;
using Level.Nodes;
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

            ShowRewardNode((RewardNodeDataSO) args[0]);

            base.Show();
        }

        private void ShowRewardNode(RewardNodeDataSO rewardNodeData)
        {
            StringBuilder resultText = new StringBuilder();
            
            if (rewardNodeData.rationReward > 0)
            {
                var rationReward = rewardNodeData.rationReward;
                resultText.Append($"Gained {rationReward} rations!");
                resultText.AppendLine();
            }
            if (rewardNodeData.weaponRewards.Count > 0)
            {
                foreach (var weaponReward in rewardNodeData.weaponRewards)
                {
                    resultText.Append($"Gained {weaponReward.m_WeaponName}!");
                }
            }
            
            m_ResultText.text = resultText.ToString();
        
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
