using System.Collections.Generic;
using UnityEngine;

namespace Level.Nodes
{
    [CreateAssetMenu(fileName = "RewardNodeDataSO", menuName = "ScriptableObject/Level/RewardNodeDataSO")]
    public class RewardNodeDataSO : NodeDataSO
    {
        public override NodeType nodeType => NodeType.REWARD;

        [Header("Reward Details")]
        [Tooltip("The main type of reward. Determines the token displayed on the node.")]
        public RewardType rewardType;
        public NodeReward nodeReward;

        public override NodeReward GetNodeReward()
        {
            return nodeReward;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            
        }
#endif
    }
}
