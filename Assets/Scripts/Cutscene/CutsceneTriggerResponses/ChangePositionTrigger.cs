using System.Collections;
using UnityEngine;

[AddComponentMenu("CutsceneTriggerResponses/ChangePositionTrigger")]
public class ChangePositionTrigger : CutsceneTriggerResponse
{
    [SerializeField] private Transform m_MoveToPosition;
    [SerializeField] private bool m_Instant;
    [SerializeField] private float m_LerpTime;

    protected override void PerformTrigger()
    {
        if (m_Instant)
            transform.position = m_MoveToPosition.position;
        else
            StartCoroutine(MoveTowards(m_MoveToPosition.position));
    }

    private IEnumerator MoveTowards(Vector3 targetPosition)
    {
        float t = 0f;
        Vector3 initialPosition = transform.position;
        while (t < m_LerpTime)
        {
            yield return null;
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(initialPosition, targetPosition, t / m_LerpTime);
        }
        transform.position = targetPosition;
    }
}