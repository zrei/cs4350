using UnityEngine;

[RequireComponent(typeof(CutsceneSpawner))]
[AddComponentMenu("CutsceneTriggerResponses/ChangeCutsceneTrigger")]
public class ChangeCutsceneTrigger : CutsceneTriggerResponse
{
    [SerializeField] Cutscene m_Cutscene;

    private CutsceneSpawner m_CutsceneSpawner;

    private void Start()
    {
        m_CutsceneSpawner = GetComponent<CutsceneSpawner>();
    }

    protected override void PerformTrigger()
    {
       m_CutsceneSpawner.SwitchToCutscene(m_Cutscene);
    }
}
