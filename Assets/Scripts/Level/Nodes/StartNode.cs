using Game.UI;
using UnityEngine;

public class StartNode : NodeInternal
{
    [SerializeField] private Dialogue m_DefaultDialogue;
    [SerializeField] private ConditionalDialogue[] m_ConditionalDialogues;
    
    public void StartNodeEvent(VoidEvent onEventFinished)
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
        
        onEventFinished?.Invoke();
        return;

        void onDialogueEnd()
        {
            GlobalEvents.Dialogue.DialogueEndEvent -= onDialogueEnd;
            onEventFinished?.Invoke();
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
