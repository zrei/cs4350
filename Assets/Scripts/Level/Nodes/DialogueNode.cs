using Game.UI;
using Level.Nodes;
using UnityEngine;

public class DialogueNode : NodeInternal
{
    [SerializeField] private Dialogue m_DefaultDialogue;
    [SerializeField] private ConditionalDialogue[] m_ConditionalDialogues;
    
    protected override void PerformNode(VoidEvent postEvent = null)
    {
        Debug.Log("Starting Dialogue Node");
        GlobalEvents.Dialogue.DialogueEndEvent += OnDialogueEnd;
        
        foreach (var conditionalDialogue in m_ConditionalDialogues)
        {
            if (conditionalDialogue.IsConditionsSatisfied())
            {
                DialogueDisplay.Instance.StartDialogue(conditionalDialogue.m_Dialogue);
                return;
            }
        }
        DialogueDisplay.Instance.StartDialogue(m_DefaultDialogue);
    }

    private void OnDialogueEnd()
    {
        Debug.Log("Dialogue Ended");
        GlobalEvents.Dialogue.DialogueEndEvent -= OnDialogueEnd;
        
        if (!m_HasPlayedPostTutorial)
        {
            m_HasPlayedPostTutorial = true;
            PlayTutorial(m_PostTutorial, () => GlobalEvents.Level.DialogueNodeEndEvent?.Invoke(this));
        }
        else
        {
            GlobalEvents.Level.DialogueNodeEndEvent?.Invoke(this);
        }
    }
}
