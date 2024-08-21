using UnityEngine;

public enum LogLevel
{
    LOG,
    WARNING,
    ERROR
}

/// <summary>
/// Helper class to log messages to the Unity console.
/// </summary>
public static class Logger
{
    #region Colors
    private const string HeaderColor = "#d199de";
    private const string ClassColor = "#a2e6f5";
    private const string ObjectColor = "#a1f0ae";
    #endregion

    #region Formatting
    private const string LogObjectFormat = "<color=" + HeaderColor + ">[Class]</color> <color=" + ClassColor + ">[{0}]</color> <color=" + ObjectColor + ">[{1}]</color> {2}"; // class name, object name, message
    private const string LogSingletonFormat = "<color=" + HeaderColor + ">[Singleton]</color> <color=" + ClassColor + ">[{0}]</color> {1}"; // class name, message
    private const string LogEditorFormat = "<color=" + HeaderColor + ">[Editor]</color> <color=" + ClassColor + ">[{0}]</color> {1}"; // class name, message
    #endregion

    public static void Log(string singletonName, string message, LogLevel level)
    {
        Log(level, string.Format(LogSingletonFormat, singletonName, message));
    }

    public static void Log(string className, string objectName, string message, Object context, LogLevel level)
    {
        Log(level, string.Format(LogObjectFormat, className, objectName, message), context);
    }

    public static void LogEditor(string className, string message, LogLevel level)
    {
        Log(level, string.Format(LogEditorFormat, className, message));
    }

    private static void Log(LogLevel log, string message)
    {
        switch (log)
        {
            case LogLevel.LOG:
                Debug.Log(message);
                break;
            case LogLevel.WARNING:
                Debug.LogWarning(message);
                break;
            case LogLevel.ERROR:
                Debug.LogError(message);
                break;
        }
    }

    private static void Log(LogLevel log, string message, Object context)
    {
        switch (log)
        {
            case LogLevel.LOG:
                Debug.Log(message, context);
                break;
            case LogLevel.WARNING:
                Debug.LogWarning(message, context);
                break;
            case LogLevel.ERROR:
                Debug.LogError(message, context);
                break;
        }
    }
}