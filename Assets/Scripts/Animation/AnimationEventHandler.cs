using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public event Action onSkillHit;
    public event Action onSkillComplete;

    private void OnSkillHit()
    {
        onSkillHit?.Invoke();
    }

    private void OnSkillComplete()
    {
        onSkillComplete?.Invoke();
    }
}
