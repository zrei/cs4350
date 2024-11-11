using System.Collections;
using UnityEngine;


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

    public Coroutine ExecuteAfterDelay(VoidEvent action, float delay)
    {
        IEnumerator Co()
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        return StartCoroutine(Co());
    }

    public Coroutine ExecuteAfterFrames(VoidEvent action, float frames)
    {
        IEnumerator Co()
        {
            for (int i = 0; i < frames; i++) yield return null;
            action?.Invoke();
        }
        return StartCoroutine(Co());
    }
}
