using System.Collections;
using UnityEngine;

[AddComponentMenu("CutsceneTriggerResponses/ChangeScaleCutsceneTrigger")]
public class ChangeScaleCutsceneTrigger : CutsceneTriggerResponse
{
    [SerializeField] private Vector3 m_TargetScale = Vector3.one;
    [SerializeField] private bool m_Instant;
    [SerializeField] private float m_LerpTime;

    protected override void PerformTrigger()
    {
        if (m_Instant)
        {
            transform.localScale = m_TargetScale;
        }
        else
        {
            StartCoroutine(ChangeScale(m_TargetScale));
        }
    }

    private IEnumerator ChangeScale(Vector3 targetScale)
    {
        float t = 0f;
        Vector3 initialScale = transform.localScale;
        while (t < m_LerpTime)
        {
            yield return null;
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t / m_LerpTime);
        }
        transform.localScale = targetScale;
    }
}
