using UnityEngine;

[RequireComponent(typeof(BGMManager))]
public class BGMCutsceneTrigger : CutsceneTriggerResponse
{
    private BGMManager m_BGMManager;

    private void Start()
    {
        m_BGMManager = GetComponent<BGMManager>();
    }
    protected override void PerformTrigger()
    {
        throw new System.NotImplementedException();
    }
}
