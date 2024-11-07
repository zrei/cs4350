using UnityEngine;
using UnityEngine.Serialization;

namespace Level
{
    [System.Serializable]
    public struct FlagTrigger
    {
        public string flagName;
        public FlagType flagType;
        public bool flagValue;
    }
    
    /// <summary>
    /// Script to store results of a dialogue to be applied.
    /// Results includes ration and morality changes.
    /// ApplyResults() can then be added to a dialogue's OnEnterEvent in the editor.
    /// </summary>
    public class DialogueResult : MonoBehaviour
    {
        [FormerlySerializedAs("timeChangeAmt")] public float rationChangeAmt;
        
        public int moralityChangeAmt;
        
        public FlagTrigger[] flagResults;
        
        public void ApplyResults()
        {
            ApplyRationsChange();
            ApplyMoralityChange();
            ApplyFlagTriggers();
        }
        
        private void ApplyRationsChange()
        {
            if (rationChangeAmt == 0) return;
            
            GlobalEvents.Rations.RationsChangeEvent(rationChangeAmt);
        }
        
        private void ApplyMoralityChange()
        {
            if (moralityChangeAmt == 0) return;
            
            GlobalEvents.Morality.MoralityChangeEvent(moralityChangeAmt);
        }
        
        private void ApplyFlagTriggers()
        {
            foreach (var flag in flagResults)
            {
                FlagManager.Instance.SetFlagValue(flag.flagName, flag.flagValue, flag.flagType);
            }
        }
    }
}