using Game.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour
{
    public UnityEvent onEnterState;
    public UnityEvent onExitState;

    [TextArea]
    public string text;

    public bool DebugTrigger
    {
        get => debugTrigger;
        set
        {
            debugTrigger = value;
            DialogueDisplay.Instance.StartDialogue(this);
        }
    }
    [SerializeField]
    [SerializeProperty("DebugTrigger")]
    private bool debugTrigger;

    [Serializable]
    public struct DialogueOption
    {
        [TextArea]
        public string text;
        public Dialogue nextState;
        [Tooltip("Whether this dialogue option will result in a change in morality")]
        public bool changesMorality;
        [Tooltip("How much the morality should change by")]
        public int moralityChange;
    }

    public List<DialogueOption> options;
    public Dialogue defaultNextState;
}
