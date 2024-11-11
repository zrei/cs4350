using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour
{
    public UnityEvent onEnterState;
    public UnityEvent onExitState;

    public Sprite characterSprite;
    public string characterName;
    public Color characterColor = Color.white;

    public AudioDataSO audioCue;

    [TextArea(6, 12)]
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
        [TextArea(3, 6)]
        public string text;
        public Dialogue nextState;

        public List<Condition> conditions;
        public bool hideIfConditionsUnmet;
        [TextArea(3, 6)]
        public string lockedText;

        [Tooltip("Whether this dialogue option will result in a change in morality")]
        public bool changesMorality;
        [Tooltip("How much the morality should change by")]
        public int moralityChange;

        public bool IsUnlocked => conditions.All(x => x.Evaluate());
    }

    public List<DialogueOption> options;
    public Dialogue defaultNextState;
}
