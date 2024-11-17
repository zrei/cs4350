using UnityEngine;

[RequireComponent(typeof(BGMManager))]
[AddComponentMenu("CutsceneTriggerResponses/BGMCutsceneTrigger")]
public class BGMCutsceneTrigger : CutsceneTriggerResponse
{
    [SerializeField] private AudioDataSO m_BGM;
    private BGMManager m_BGMManager;

    private void Start()
    {
        m_BGMManager = GetComponent<BGMManager>();
    }

    protected override void PerformTrigger()
    {
        m_BGMManager.FadeOutCurrBgm(() => m_BGMManager.PlayOtherBgm(m_BGM));
    }
}
