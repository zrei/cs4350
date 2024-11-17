public abstract class CutsceneTriggerResponse : TriggerBase
{
    private void Awake()
    {
        GlobalEvents.CutsceneEvents.CutsceneTriggerEvent += OnTrigger;
    }

    private void OnDestroy()
    {
        GlobalEvents.CutsceneEvents.CutsceneTriggerEvent -= OnTrigger;
    }

    private void OnTrigger(CutsceneTriggerEnum trigger)
    {
        if (m_CutsceneTrigger != trigger)
            return;

        PerformTrigger();
    }

    protected abstract void PerformTrigger();
}
