using UnityEngine;

/// <summary>
/// Place this on the object you want to destroy
/// </summary>
[AddComponentMenu("CutsceneTriggerResponses/DestroyObjectTrigger")]
public class DestroyObjectTrigger : CutsceneTriggerResponse
{
    protected override void PerformTrigger()
    {
        Destroy(this.gameObject);
    }
}
