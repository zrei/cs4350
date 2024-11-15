using UnityEngine;

public enum CutsceneTriggerEnum
{
    PRE_ONE_EXAMPLE
}

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] CutsceneTriggerEnum m_CutsceneTrigger;

    public void Trigger()
    {
        GlobalEvents.CutsceneEvents.CutsceneTriggerEvent?.Invoke(m_CutsceneTrigger.ToString());
    }
}
