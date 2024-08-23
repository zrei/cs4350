using UnityEngine;

public class GlobalSettings : Singleton<GlobalSettings>
{
    [Header("Debug")]
    [SerializeField] private bool m_DoDebug = false;
    public static bool DoDebug => Instance.m_DoDebug;
}