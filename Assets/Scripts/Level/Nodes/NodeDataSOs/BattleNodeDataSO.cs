using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Level.Nodes
{
    [CreateAssetMenu(fileName = "BattleNodeDataSO", menuName = "ScriptableObject/Level/BattleNodeDataSO")]
    public class BattleNodeDataSO : NodeDataSO
    {
        public override NodeType nodeType => NodeType.Battle;

        [Header("Battle Details")]
        public BattleSO battleSO;
        public int rationReward;
        public List<WeaponInstanceSO> weaponRewards;
        
        public override NodePreviewData GetNodePreviewData()
        {
            return new BattleNodePreviewData
            {
                NodeName = nodeName,
                NodeDescription = nodeDescription,
                IsMoralityLocked = isMoralityLocked,
                MoralityThreshold = moralityThreshold,
                EnemyUnits = battleSO.m_EnemyUnitsToSpawn,
                Objectives = battleSO.m_Objectives
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            
        }
#endif
    }
}
