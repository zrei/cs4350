using Game.UI;
using Level.Nodes;
using UnityEngine;

public class StartNode : NodeInternal
{
    [SerializeField] private Dialogue m_DefaultDialogue;
    [SerializeField] private ConditionalDialogue[] m_ConditionalDialogues;  

    protected override void PerformNode(VoidEvent postEvent = null)
    {
        foreach (var conditionalDialogue in m_ConditionalDialogues)
        {
            if (conditionalDialogue.IsConditionsSatisfied())
            {
                DialogueDisplay.Instance.StartDialogue(conditionalDialogue.m_Dialogue);
                GlobalEvents.Dialogue.DialogueEndEvent += onDialogueEnd;
                return;
            }
        }

        if (m_DefaultDialogue != null)
        {
            DialogueDisplay.Instance.StartDialogue(m_DefaultDialogue);
            GlobalEvents.Dialogue.DialogueEndEvent += onDialogueEnd;
            return;
        }
        
        PostTutorial();
        return;

        void onDialogueEnd()
        {
            GlobalEvents.Dialogue.DialogueEndEvent -= onDialogueEnd;
            PostTutorial();
        }

        void PostTutorial()
        {
            if (!m_HasPlayedPostTutorial)
            {
                m_HasPlayedPostTutorial = true;
                PlayTutorial(m_PostTutorial, postEvent);
            }
            else
            {
                postEvent?.Invoke();
            }
        }
    }

    public override void Initialise()
    {
        // Cleared by default
        SetCleared();
    }
    
    public override void ClearNode()
    {
    }
}
