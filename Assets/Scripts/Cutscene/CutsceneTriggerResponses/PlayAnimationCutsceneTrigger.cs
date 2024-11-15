using UnityEngine;

[RequireComponent(typeof(ArmorVisual))]
[AddComponentMenu("CutsceneTriggerResponses/PlayAnimationCutsceneTrigger")]
public class PlayAnimationCutsceneTrigger : CutsceneTriggerResponse
{
    [SerializeField] private int m_AnimationTriggerId;

    private ArmorVisual m_ArmorVisual;

    private void Start()
    {
        m_ArmorVisual = GetComponent<ArmorVisual>();
    }

    protected override void PerformTrigger()
    {
        m_ArmorVisual.PlayAnimations(m_AnimationTriggerId);
    }
}
