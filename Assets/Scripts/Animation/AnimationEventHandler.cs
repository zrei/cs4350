using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public event Action onSkillRelease;
    public event Action onSkillHit;
    public event Action onSkillComplete;

    private void OnSkillRelease()
    {
        onSkillRelease?.Invoke();
    }

    private void OnSkillHit()
    {
        onSkillHit?.Invoke();
    }

    private void OnSkillComplete()
    {
        onSkillComplete?.Invoke();
    }
}
