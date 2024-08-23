public class Persisting : Singleton<Persisting>
{
    protected override void HandleAwake()
    {
        base.HandleAwake();
        DontDestroyOnLoad(this.gameObject);
    }
}