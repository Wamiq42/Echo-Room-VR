using System.Collections;
using EchoRoom.XR;
using UnityEngine;

/// <summary>
/// Applies Quest runtime foveation and reports frame timing.
///
/// Display refresh rate is owned exclusively by <see cref="EchoRoomDisplayRefreshRateFeature"/>.
/// This component intentionally never changes the panel rate, Unity VSync, or
/// <see cref="Application.targetFrameRate"/>, so it cannot overwrite an applied 90 Hz session.
/// Horizon OS is also left to manage CPU and GPU performance levels adaptively.
/// </summary>
[DisallowMultipleComponent]
public sealed class QuestPerformanceTuner : MonoBehaviour
{
    /// <summary>
    /// Installs one persistent tuner when no scene provides an explicitly configured instance.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindFirstObjectByType<QuestPerformanceTuner>() != null)
            return;

        var host = new GameObject("Quest Performance Tuner");
        host.AddComponent<QuestPerformanceTuner>();
        DontDestroyOnLoad(host);
    }

    [Header("Fixed Foveated Rendering")]
    [SerializeField]
    [Tooltip("Reduces shading rate at the edges of the lenses.")]
    private bool enableFoveatedRendering = true;

    [SerializeField]
    [Range(0, 3)]
    [Tooltip("0 Off, 1 Low, 2 Medium, 3 High.")]
    private int foveationLevel = 2;

    [SerializeField]
    [Tooltip("Allows the runtime to raise foveation when the GPU is under pressure.")]
    private bool dynamicFoveation = true;

    [Header("Diagnostics")]
    [SerializeField]
    [Tooltip("Logs measured frame time periodically. Read with: adb logcat -s Unity")]
    private bool logFrameStats = true;

    [SerializeField]
    [Min(1f)]
    [Tooltip("Startup time ignored before collecting frame statistics.")]
    private float settleSeconds = 4f;

    [SerializeField]
    [Min(1f)]
    [Tooltip("Length of each frame-statistics window.")]
    private float statsIntervalSeconds = 5f;

    /// <summary>
    /// The applied OpenXR refresh rate, or the current Unity fallback target before a rate is applied.
    /// </summary>
    public float AppliedHz
    {
        get
        {
            if (EchoRoomDisplayRefreshRateFeature.HasAppliedRefreshRate)
                return EchoRoomDisplayRefreshRateFeature.AppliedRefreshRate;

            return Application.targetFrameRate > 0
                ? Application.targetFrameRate
                : EchoRoomDisplayRefreshRateFeature.FallbackFrameRate;
        }
    }

    private void Start()
    {
        ApplyFoveatedRendering();

        if (logFrameStats)
            StartCoroutine(LogFrameStats());
    }

    private void ApplyFoveatedRendering()
    {
        if (!enableFoveatedRendering)
            return;

        try
        {
            OVRPlugin.foveatedRenderingLevel =
                (OVRPlugin.FoveatedRenderingLevel)Mathf.Clamp(foveationLevel, 0, 3);
            OVRPlugin.useDynamicFoveatedRendering = dynamicFoveation;

            Debug.Log(
                $"[QuestPerf] Foveated rendering level {foveationLevel}, dynamic {dynamicFoveation}. " +
                "CPU/GPU levels remain managed by Horizon OS.");
        }
        catch (System.Exception error)
        {
            Debug.LogWarning("[QuestPerf] Foveated rendering unavailable: " + error.Message);
        }
    }

    private IEnumerator LogFrameStats()
    {
        yield return new WaitForSecondsRealtime(settleSeconds);

        while (true)
        {
            int frameCount = 0;
            float totalSeconds = 0f;
            float worstSeconds = 0f;
            float windowEnd = Time.realtimeSinceStartup + statsIntervalSeconds;

            while (Time.realtimeSinceStartup < windowEnd)
            {
                float delta = Time.unscaledDeltaTime;
                frameCount++;
                totalSeconds += delta;
                worstSeconds = Mathf.Max(worstSeconds, delta);
                yield return null;
            }

            if (frameCount == 0)
                continue;

            float averageSeconds = totalSeconds / frameCount;
            float targetHz = AppliedHz;
            Debug.Log(
                $"[QuestPerf] Window: {1f / Mathf.Max(averageSeconds, 0.0001f):F0} fps average, " +
                $"{averageSeconds * 1000f:F2} ms average, {worstSeconds * 1000f:F2} ms worst, " +
                $"target {targetHz:F0} Hz.");
        }
    }
}