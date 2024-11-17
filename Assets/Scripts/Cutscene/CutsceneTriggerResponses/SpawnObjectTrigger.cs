using UnityEngine;

/// <summary>
/// Place this on the parent you want to spawn the component under
/// </summary>
[AddComponentMenu("CutsceneTriggerResponses/SpawnObjectTrigger")]
public class SpawnObjectTrigger : CutsceneTriggerResponse
{
    [SerializeField] private GameObject m_Component;

    protected override void PerformTrigger()
    {
        Instantiate(m_Component, this.transform);
    }
}
