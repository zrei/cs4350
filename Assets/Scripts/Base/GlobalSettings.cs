using UnityEngine;

public class GlobalSettings : Singleton<GlobalSettings>
{
    [Header("Debug")]
    [SerializeField] private bool m_DoDebug = false;
    public static bool DoDebug => Instance.m_DoDebug;

    [Header("Movement Config")]
    [SerializeField] private bool m_AllowCrossingOverOccupiedSquares = false;
    public static bool AllowCrossingOverOccupiedSquares => Instance.m_AllowCrossingOverOccupiedSquares;
}