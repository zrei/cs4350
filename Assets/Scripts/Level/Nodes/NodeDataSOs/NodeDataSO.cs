using System.Linq;
using UnityEngine;

namespace Level.Nodes
{
    public enum NodeType
    {
        DIALOGUE,
        REWARD,
        BATTLE,
        EMPTY,
    }

    [CreateAssetMenu(fileName = "NodeDataSO", menuName = "ScriptableObject/Level/NodeDataSO")]
    public class NodeDataSO : ScriptableObject
    {
        public virtual NodeType nodeType => NodeType.EMPTY;
        
        [Header("Node Details")]
        public string nodeName;
        public string nodeDescription;
        
        public bool isMoralityLocked;
        public Threshold moralityThreshold;
        
        [Header("Dialogues")]
        [Tooltip("Dialogue to play upon first visiting the node - leave empty for no dialogue")]
        public Dialogue defaultPreDialogue;
        [Tooltip("Conditional pre-dialogues to play given certain conditions are met. Takes priority over default pre-dialogue")]
        public ConditionalDialogue[] conditionalPreDialogues;
        [Tooltip("Dialogue to play upon finishing the node for the first time - leave empty for no dialogue")]
        public Dialogue defaultPostDialogue;
        [Tooltip("Conditional post-dialogues to play given certain conditions are met. Takes priority over default post-dialogue")]
        public ConditionalDialogue[] conditionalPostDialogues;
        
        [Header("Tutorials")]
        [Tooltip("Tutorial to play upon first visiting the node - leave empty for no tutorial")]
        public TutorialSO preTutorial;
        [Tooltip("Tutorial to play upon finishing the node for the first time - leave empty for no tutorial")]
        public TutorialSO postTutorial;
        [Tooltip("Tutorial to play before next node selection phase after clearing this node for the first time - leave empty for no tutorial")]
        public TutorialSO preSelectionTutorial;
        
        public virtual NodePreviewData GetNodePreviewData()
        {
            return new NodePreviewData
            {
                NodeName = nodeName,
                NodeDescription = nodeDescription,
                IsMoralityLocked = isMoralityLocked,
                MoralityThreshold = moralityThreshold
            };
        }

        public Dialogue GetPreDialogueToPlay()
        {
            foreach (var conditionalDialogue in conditionalPreDialogues)
            {
                if (conditionalDialogue.IsConditionsSatisfied())
                    return conditionalDialogue.m_Dialogue;
            }

            return defaultPreDialogue;
        }

        public Dialogue GetPostDialogueToPlay()
        {
            foreach (var conditionalDialogue in conditionalPostDialogues)
            {
                if (conditionalDialogue.IsConditionsSatisfied())
                    return conditionalDialogue.m_Dialogue;
            }

            return defaultPostDialogue;
        }
        
        public virtual NodeReward GetNodeReward()
        {
            // Default reward is empty
            return new NodeReward();
        }
    }
    
    [System.Serializable]
    public struct NodeReward
    {
        public int rationReward;
        public WeaponInstanceSO[] weaponRewards;
        
        public bool IsEmpty()
        {
            return rationReward == 0 && weaponRewards.Length == 0;
        }
    }
    
    [System.Serializable]
    public struct FlagCondition
    {
        public string flagName;
        public bool flagValue;
        
        public bool IsSatisfied()
        {
            return FlagManager.Instance.GetFlagValue(flagName) == flagValue;
        }
    }

    [System.Serializable]
    public struct ConditionalDialogue
    {
        public Dialogue m_Dialogue;
        public Threshold[] m_MoralityConditions;
        public FlagCondition[] m_FlagConditions;
    
        public bool IsConditionsSatisfied()
        {
            if (m_MoralityConditions.Any(moralityCondition => !moralityCondition.IsSatisfied(MoralityManager.Instance.CurrMoralityPercentage)))
            {
                return false;
            }
            
            if (m_FlagConditions.Any(flagCondition => !flagCondition.IsSatisfied()))
            {
                return false;
            }

            return true;
        }
    }
}
