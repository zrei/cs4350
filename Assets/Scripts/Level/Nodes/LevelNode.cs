using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Level.Nodes
{
    public class LevelNode : MonoBehaviour
    {
        public int nodeId;
        [Expandable] public NodeDataSO nodeData;
        public NodeType NodeType => nodeData.nodeType;

        #region Node State Information

        // Whether is goal node
        private bool m_IsGoalNode = false;
        public bool IsGoalNode => m_IsGoalNode;
    
        // Whether the node has been cleared
        private bool m_IsCleared = false;
        public bool IsCleared => m_IsCleared;

        // Whether the node is the current node
        private bool m_IsCurrent = false;
        public bool IsCurrent => m_IsCurrent;
        
        public void SetGoalNode()
        {
            m_IsGoalNode = true;
        }
    
        public void SetCleared()
        {
            m_IsCleared = true;
        }
    
        public void SetCurrent(bool isCurrent)
        {
            m_IsCurrent = isCurrent;
        }

        #endregion
        
        #region Node Graph
        
        // Adjacent nodes and their costs
        private Dictionary<LevelNode, float> m_AdjacentNodes = new();
        public Dictionary<LevelNode, float> AdjacentNodes => m_AdjacentNodes;

        public void AddAdjacentNode(LevelNode levelNode, float cost)
        {
            m_AdjacentNodes.TryAdd(levelNode, cost);
        }
    
        public void RemoveAdjacentNode(LevelNode levelNode)
        {
            m_AdjacentNodes.Remove(levelNode);
        }
        
        #endregion
        
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (nodeData == null)
            {
                Logger.LogEditor(this.GetType().Name, $"NodeData must be specified: {name}", LogLevel.WARNING);
            }
        }
#endif
    }
    
    
}