using UnityEngine;

namespace Level
{
    /// <summary>
    /// Script to store results of a dialogue to be applied.
    /// Results includes ration and morality changes.
    /// ApplyResults() can then be added to a dialogue's OnEnterEvent in the editor.
    /// </summary>
    public class DialogueResult : MonoBehaviour
    {
        // To change to ration in the future
        public float timeChangeAmt;
        
        public int moralityChangeAmt;
        
        public void ApplyResults()
        {
            ApplyTimeChange();
            ApplyMoralityChange();
        }
        
        private void ApplyTimeChange()
        {
            if (timeChangeAmt == 0) return;
            
            var levelTimerLogic = FindObjectOfType<LevelTimerLogic>();
            levelTimerLogic.AddTime(timeChangeAmt);
        }
        
        private void ApplyMoralityChange()
        {
            if (moralityChangeAmt == 0) return;
            
            GlobalEvents.Morality.MoralityChangeEvent(moralityChangeAmt);
        }
    }
}