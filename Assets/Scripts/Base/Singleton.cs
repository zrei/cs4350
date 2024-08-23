using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance => m_Instance;
    private static T m_Instance = null;

    public static bool IsReady => m_Instance != null && m_Instance.m_IsReady;
    private bool m_IsReady = false;
    public static VoidEvent OnReady;

    private void Awake()
    {
        if (m_Instance != null)
        {
            Debug.LogError("An instance already exists!");
            Destroy(this.gameObject);
            return;
        }

        m_Instance = (T) this;
        HandleAwake();
        m_IsReady = true;
        OnReady?.Invoke();
        Logger.Log(this.GetType().Name, "Singleton is ready", LogLevel.LOG);
    }

    protected virtual void HandleAwake()
    {

    }

    protected virtual void HandleDestroy()
    {

    }

    private void OnDestroy()
    {
        HandleDestroy();
        if (m_Instance == this)
        {
            m_Instance = null;
            Logger.Log(this.GetType().Name, "Singleton has been destroyed", LogLevel.LOG);
        }
    }
}