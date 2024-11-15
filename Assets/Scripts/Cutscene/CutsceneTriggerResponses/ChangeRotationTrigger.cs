using UnityEngine;

public class ChangeRotationTrigger : CutsceneTriggerResponse
{
    [SerializeField] private Transform m_LookAt;
    [SerializeField] private bool m_Instant;

    protected override void PerformTrigger()
    {
        
    }
}