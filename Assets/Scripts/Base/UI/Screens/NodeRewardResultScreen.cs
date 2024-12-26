using System.Text;
using Game.Input;
using Level.Nodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class NodeRewardResultScreen : BaseUIScreen
    {
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] SelectableBase m_ReturnButton;

        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            ShowRewardNode((NodeReward) args[0]);

            base.Show();
        }

        private void ShowRewardNode(NodeReward nodeReward)
        {
            StringBuilder resultText = new StringBuilder();
            
            if (nodeReward.rationReward > 0)
            {
                var rationReward = nodeReward.rationReward;
                resultText.AppendLine($"Gained {rationReward} rations!");
                resultText.AppendLine();
            }
            if (nodeReward.weaponRewards.Length > 0)
            {
                foreach (var weaponReward in nodeReward.weaponRewards)
                {
                    resultText.AppendLine($"Gained {weaponReward.m_WeaponName}!");
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
