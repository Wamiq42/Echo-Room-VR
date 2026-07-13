using System;
using System.IO;
using UnityEngine;

public static class TutorialRuntimeLogger
{
    static string logPath;
    static float sessionStart;
    static bool started;

    public static string LogPath => logPath;

    public static void Begin(string context)
    {
#if UNITY_EDITOR
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
#else
        string directory = Path.Combine(Application.persistentDataPath, "Diagnostics");
#endif
        Directory.CreateDirectory(directory);
        logPath = Path.Combine(directory, "TutorialRuntime.log");
        sessionStart = Time.realtimeSinceStartup;
        started = true;

        string header =
            "ECHO ROOM - TUTORIAL RUNTIME TRACE" + Environment.NewLine +
            "Started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Environment.NewLine +
            "Unity: " + Application.unityVersion + Environment.NewLine +
            "Platform: " + Application.platform + Environment.NewLine +
            "Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + Environment.NewLine +
            "Context: " + context + Environment.NewLine +
            "Log path: " + logPath + Environment.NewLine +
            new string('=', 90) + Environment.NewLine;

        SafeWrite(header, false);
        Debug.Log("[TutorialTrace] Logging to: " + logPath);
    }

    public static void Event(string category, string message, UnityEngine.Object context = null)
    {
        if (!started) Begin("Late logger initialization");

        string line = string.Format(
            "[{0,8:F3}s] [{1}] {2}",
            Time.realtimeSinceStartup - sessionStart,
            category,
            message);

        SafeWrite(line + Environment.NewLine, true);
        Debug.Log("[TutorialTrace] " + line, context);
    }

    public static void Error(string category, string message, UnityEngine.Object context = null)
    {
        if (!started) Begin("Late logger initialization");

        string line = string.Format(
            "[{0,8:F3}s] [ERROR:{1}] {2}",
            Time.realtimeSinceStartup - sessionStart,
            category,
            message);

        SafeWrite(line + Environment.NewLine, true);
        Debug.LogError("[TutorialTrace] " + line, context);
    }

    static void SafeWrite(string value, bool append)
    {
        try
        {
            if (append) File.AppendAllText(logPath, value);
            else File.WriteAllText(logPath, value);
        }
        catch (Exception exception)
        {
            Debug.LogError("[TutorialTrace] Could not write log: " + exception.Message);
        }
    }
}
