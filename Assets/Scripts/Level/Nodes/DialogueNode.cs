using Game.UI;
using UnityEngine;

public class DialogueNode : NodeInternal
{
    [SerializeField] private Dialogue m_Dialogue;
    
    public override void StartNodeEvent()
    {
        Debug.Log("Starting Dialogue Node");
        GlobalEvents.Dialogue.DialogueEndEvent += OnDialogueEnd;
        
        DialogueDisplay.Instance.StartDialogue(m_Dialogue);
    }

    private void OnDialogueEnd()
    {
        Debug.Log("Dialogue Ended");
        GlobalEvents.Dialogue.DialogueEndEvent -= OnDialogueEnd;
        
        GlobalEvents.Level.DialogueNodeEndEvent?.Invoke(this);
    }
}
