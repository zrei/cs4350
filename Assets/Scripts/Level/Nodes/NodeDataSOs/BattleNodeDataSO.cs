using System.Collections.Generic;
using UnityEngine;

namespace Level.Nodes
{
    public class BattleNodePreviewData : NodePreviewData
    {
        public List<EnemyUnitPlacement> EnemyUnits;
        public List<ObjectiveSO> Objectives;
    }
    
    [CreateAssetMenu(fileName = "BattleNodeDataSO", menuName = "ScriptableObject/Level/BattleNodeDataSO")]
    public class BattleNodeDataSO : NodeDataSO
    {
        public override NodeType nodeType => NodeType.BATTLE;

        [Header("Battle Details")]
        public BattleSO battleSO;
        public NodeReward BattleReward;
        public int SkipBattleExpReward;
        public NodeReward SkipBattleReward;
        
        [Header("Node Display Details")]
        [Tooltip("Enemy unit to display on the node. If left empty, the first enemy unit in the battleSO will be used")]
        public EnemyCharacterSO overrideDisplayEnemyUnit;
        
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
        
        public EnemyCharacterSO GetDisplayEnemyUnit()
        {
            return overrideDisplayEnemyUnit ? overrideDisplayEnemyUnit : battleSO.m_EnemyUnitsToSpawn[0].m_EnemyCharacterData;
        }
        
        public override NodeReward GetNodeReward()
        {
            return FlagManager.Instance.GetFlagValue(Flag.SKIP_BATTLE_FLAG) ? SkipBattleReward : BattleReward;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            
        }
#endif
    }
}
