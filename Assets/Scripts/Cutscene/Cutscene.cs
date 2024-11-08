using UnityEngine;

public class Cutscene : MonoBehaviour
{
    /// <summary>
    /// For use in spawning this cutscene when a certain dialogue plays
    /// </summary>
    public void SpawnCutscene()
    {
        GlobalEvents.CutsceneEvents.StartCutsceneEvent?.Invoke(this);
    }
}
