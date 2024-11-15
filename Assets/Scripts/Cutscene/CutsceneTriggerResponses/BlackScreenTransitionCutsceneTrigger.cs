using UnityEngine;

[AddComponentMenu("CutsceneTriggerResponses/BlackScreenTransitionCutsceneTrigger")]
public class BlackScreenTransitionCutsceneTrigger : CutsceneTriggerResponse
{
    protected override void PerformTrigger()
    {
        GameSceneManager.Instance.PlayTransition(null, null);
    }
}
