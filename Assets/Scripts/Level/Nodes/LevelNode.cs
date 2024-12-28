using System.Collections.Generic;
using Game.UI;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

#endif

namespace Level.Nodes
{
    public class NodePreviewData
    {
        public string NodeName;
        public string NodeDescription;
        public bool IsMoralityLocked;
        public MoralityCondition MoralityCondition;
    }
    
    public class LevelNode : MonoBehaviour
    {
        public int nodeId;
        [Expandable]
        [SerializeField]
        private NodeDataSO m_NodeData;
        public NodeDataSO NodeData => m_NodeData;
        public NodeType NodeType => m_NodeData.nodeType;
        
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
        
        
        #region Node Events

        public virtual void EnterNode()
        {
            Debug.Log("Entered Node: " + name);
            m_IsCurrent = true;
            GlobalEvents.Level.NodeEnteredEvent?.Invoke(this);
        }
    
        public virtual void ClearNode()
        {
            Debug.Log("Cleared Node: " + name);
            SetCleared();
            GlobalEvents.Level.NodeClearedEvent?.Invoke(this);
        }
    
        public virtual void ExitNode()
        {
            Debug.Log("Exited Node: " + name);
            m_IsCurrent = false;
            GlobalEvents.Level.NodeExitedEvent?.Invoke(this);
        }

        #endregion

        #region Morality

        public bool IsMoralityLocked => m_NodeData.isMoralityLocked;
        public MoralityCondition MoralityCondition => m_NodeData.moralityCondition;

        #endregion
        
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Reference: https://stackoverflow.com/questions/56155148/how-to-avoid-the-onvalidate-method-from-being-called-in-prefab-mode
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
            if (!isValidPrefabStage && prefabConnected)
            {
                //Variables you only want checked when in a Scene
                if (m_NodeData == null)
                {
                    Logger.LogEditor(this.GetType().Name, $"NodeData must be specified: {name}", LogLevel.WARNING);
                }
            }
            //variables you want checked all the time (even in prefab mode)
            
        }
#endif
    }
}