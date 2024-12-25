using UnityEngine;

namespace Level.Tokens
{
    public abstract class LevelNodeTokenAnimator : MonoBehaviour
    {
        public abstract void ShowToken();
        public abstract void HideToken();

        public virtual void PlayEntryAnimation(PlayerToken playerToken, VoidEvent onComplete)
        {
            // No animation by default
            onComplete?.Invoke();
        }

        public virtual void PlayClearAnimation(PlayerToken playerToken, VoidEvent onComplete)
        {
            // No animation by default
            onComplete?.Invoke();
        }

        public virtual void PlayFailureAnimation(PlayerToken playerToken, VoidEvent onComplete, bool resetOnComplete)
        {
            // No animation by default
            onComplete?.Invoke();
        }
    }
}