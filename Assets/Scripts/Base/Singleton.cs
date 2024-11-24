using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance => m_Instance;
    private static T m_Instance = null;

    public static bool IsReady => m_Instance != null && m_Instance.m_IsReady;
    private bool m_IsReady = false;
    public static VoidEvent OnReady;
    private static void AddOnReadyEvent(VoidEvent onReadyEvent) => OnReady += onReadyEvent;
    private static void RemoveOnReadyEvent(VoidEvent onReadyEvent) => OnReady -= onReadyEvent;
    
    private static List<SingletonDependency> m_Dependencies = new();
    protected static void AddDependency<U>() where U : Singleton<U>
    {
        m_Dependencies.Add(new SingletonDependency{Name = typeof(U).Name, IsReady = () => Singleton<U>.IsReady, AddOnReadyEvent = Singleton<U>.AddOnReadyEvent, RemoveOnReadyEvent = Singleton<U>.RemoveOnReadyEvent});
    }

    private void Awake()
    {
        if (m_Instance != null)
        {
            Debug.LogError("An instance already exists! : " + typeof(T).Name);
            Destroy(this.gameObject);
            return;
        }

        m_Instance = (T) this;
        AddDependencies();
        HandleDependencies();
    }

    /// <summary>
    /// Override this method to add dependencies to other singletons that this singleton should initialise after,
    /// using the AddDependency method.
    /// </summary>
    protected virtual void AddDependencies()
    {
        
    }

    private void HandleDependencies()
    {
        foreach (var dependency in m_Dependencies)
        {
            if (!dependency.IsReady())
            {
                Debug.Log($"{this.GetType().Name}: Dependency not ready - {dependency.Name}");
                dependency.AddOnReadyEvent(HandleDependencies);
                return;
            }
            
            Debug.Log($"{this.GetType().Name}: Dependency ready - {dependency.Name}");
            dependency.RemoveOnReadyEvent(HandleDependencies);
        }
        
        HandleAwake();
        
        m_IsReady = true;
        Logger.Log(this.GetType().Name, "Singleton is ready", LogLevel.LOG);
        OnReady?.Invoke();
    }

    protected virtual void HandleAwake()
    {
        
    }

    protected virtual void HandleDestroy()
    {

    }

    private void OnDestroy()
    {
        if (m_Instance == this)
        {
            HandleDestroy();
            m_Instance = null;
            Logger.Log(this.GetType().Name, "Singleton has been destroyed", LogLevel.LOG);
        }
    }
}

public class SingletonDependency
{
    public string Name;
    public Func<bool> IsReady;
    public Action<VoidEvent> AddOnReadyEvent;
    public Action<VoidEvent> RemoveOnReadyEvent;
}