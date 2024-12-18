using UnityEngine;

namespace Level.Nodes
{
    public enum NodeType
    {
        Dialogue,
        Reward,
        Battle,
        Empty,
    }

    [CreateAssetMenu(fileName = "NodeDataSO", menuName = "ScriptableObject/Level/NodeDataSO")]
    public class NodeDataSO : ScriptableObject
    {
        public virtual NodeType nodeType => NodeType.Empty;
        
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
    }
    
    [System.Serializable]
    public struct FlagCondition
    {
        public string flagName;
        public bool flagValue;
    }

    [System.Serializable]
    public struct ConditionalDialogue
    {
        public Dialogue m_Dialogue;
        public Threshold[] m_MoralityConditions;
        public FlagCondition[] m_FlagConditions;
    
        public bool IsConditionsSatisfied()
        {
            foreach (var moralityCondition in m_MoralityConditions)
            {
                if (!moralityCondition.IsSatisfied(MoralityManager.Instance.CurrMoralityPercentage))
                {
                    return false;
                }
            }

            foreach (var flagCondition in m_FlagConditions)
            {
                if (FlagManager.Instance.GetFlagValue(flagCondition.flagName) != flagCondition.flagValue)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
