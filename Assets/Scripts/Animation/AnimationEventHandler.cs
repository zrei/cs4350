using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public event Action onSkillWindUp; // charge up particles
    public event Action onSkillRelease; // trails/projectiles
    public event Action onSkillHit; // buff/debuff auras/explosions
    public event Action onSkillReleaseEnd; // end trails
    public event Action onSkillComplete; // end skill animation

    private void OnSkillWindUp()
    {
        onSkillWindUp?.Invoke();
    }

    private void OnSkillRelease()
    {
        onSkillRelease?.Invoke();
    }

    private void OnSkillHit()
    {
        onSkillHit?.Invoke();
    }
    
    private void OnSkillReleaseEnd()
    {
        onSkillReleaseEnd?.Invoke();
    }

    private void OnSkillComplete()
    {
        onSkillComplete?.Invoke();
    }
}
