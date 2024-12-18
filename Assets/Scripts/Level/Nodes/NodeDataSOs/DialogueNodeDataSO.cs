using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Level.Nodes
{
    [CreateAssetMenu(fileName = "DialogueNodeDataSO", menuName = "ScriptableObject/Level/DialogueNodeDataSO")]
    public class DialogueNodeDataSO : NodeDataSO
    {
        public override NodeType nodeType => NodeType.Dialogue;

        [Header("Dialogue Details")]
        [SerializeField] private Dialogue m_DefaultDialogue;
        [SerializeField] private ConditionalDialogue[] m_ConditionalDialogues;
        
        public Dialogue GetMainDialogueToPlay()
        {
            foreach (var conditionalDialogue in conditionalPreDialogues)
            {
                if (conditionalDialogue.IsConditionsSatisfied())
                    return conditionalDialogue.m_Dialogue;
            }

            return m_DefaultDialogue;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            
        }
#endif
    }
}
