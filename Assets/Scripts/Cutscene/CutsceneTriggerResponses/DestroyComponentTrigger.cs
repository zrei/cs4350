public class DestroyComponentTrigger : CutsceneTriggerResponse
{
    protected override void PerformTrigger()
    {
        Destroy(this.gameObject);
    }
}
