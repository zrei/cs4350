using Game.UI;
using UnityEngine;
using UnityEngine.Serialization;

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

public class DialogueNode : NodeInternal
{
    [SerializeField] private Dialogue m_DefaultDialogue;
    [SerializeField] private ConditionalDialogue[] m_ConditionalDialogues;
    
    public override void StartNodeEvent()
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
        
        GlobalEvents.Level.DialogueNodeEndEvent?.Invoke(this);
    }
}
