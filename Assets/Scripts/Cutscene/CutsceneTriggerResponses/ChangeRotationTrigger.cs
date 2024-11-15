using System.Collections;
using UnityEngine;

[AddComponentMenu("CutsceneTriggerResponses/ChangeRotationTrigger")]
public class ChangeRotationTrigger : CutsceneTriggerResponse
{
    [SerializeField] private Transform m_LookAt;
    [SerializeField] private bool m_Instant;
    [SerializeField] private float m_LerpTime;

    protected override void PerformTrigger()
    {
        if (m_Instant)
            transform.LookAt(m_LookAt);
        else
            StartCoroutine(Rotation(Quaternion.LookRotation(transform.position - m_LookAt.transform.position)));

    }

    private IEnumerator Rotation(Quaternion targetRotation)
    {
        float t = 0f;
        Quaternion initialRotation = transform.rotation;
        while (t < m_LerpTime)
        {
            yield return null;
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t / m_LerpTime);
        }
        transform.rotation = targetRotation;
    }
}
