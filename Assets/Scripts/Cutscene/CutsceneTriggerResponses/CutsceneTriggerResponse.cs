using UnityEngine;

public abstract class CutsceneTriggerResponse : MonoBehaviour
{
    [SerializeField] CutsceneTriggerEnum m_CutsceneTrigger;

    private void Awake()
    {
        GlobalEvents.CutsceneEvents.CutsceneTriggerEvent += OnTrigger;
    }

    private void OnDestroy()
    {
        GlobalEvents.CutsceneEvents.CutsceneTriggerEvent -= OnTrigger;
    }

    private void OnTrigger(string trigger)
    {
        if (!m_CutsceneTrigger.ToString().Equals(trigger))
            return;

        PerformTrigger();
    }

    protected abstract void PerformTrigger();
}