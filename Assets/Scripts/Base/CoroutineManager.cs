/// <summary>
/// Singleton for non-monobehaviours to use coroutines.
/// </summary>
public class CoroutineManager : Singleton<CoroutineManager>
{
    protected override void HandleAwake()
    {
        base.HandleAwake();

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}
