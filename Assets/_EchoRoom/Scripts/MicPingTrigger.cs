using UnityEngine;
using System.Collections;

public class MicPingTrigger : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool isDebugging = false;

    [Header("Microphone Settings")]
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float checkInterval = 0.1f;

    private AudioClip micClip;
    private const int sampleWindow = 128;

    private bool isListening = true;

    private void Start()
    {
        InitializeMic();
    }

    /// <summary>
    /// Sets up the microphone device and starts the input check coroutine.
    /// </summary>
    private void InitializeMic()
    {
        string mic = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (mic == null)
        {
            Debug.LogWarning("No microphone device found.");
            enabled = false;
            return;
        }

        micClip = Microphone.Start(mic, true, 1, 44100);
        StartCoroutine(CheckMicInput());
        LogDebug("Microphone initialized and listening.");
    }

    private IEnumerator CheckMicInput()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (!isListening) continue;

            float volume = GetMaxVolume();
            if (volume > sensitivity)
            {
                LogDebug($"Mic volume {volume:F3} exceeded threshold {sensitivity}");
                
                float cooldown =  PingEmitter.RequestPing?.Invoke() ?? 0f;
                
                if (cooldown > 0f)
                {
                    LogDebug($"Ping accepted. Pausing mic listening for {cooldown:F2}s");
                    StartCoroutine(PauseMicListening(cooldown));
                }
            }
        }
    }

    /// <summary>
    /// Calculates the peak volume from the most recent microphone sample window.
    /// </summary>
    private float GetMaxVolume()
    {
        float max = 0f;
        float[] samples = new float[sampleWindow];
        int start = Microphone.GetPosition(null) - sampleWindow;

        if (start < 0) return 0f;

        micClip.GetData(samples, start);
        foreach (float s in samples)
            max = Mathf.Max(max, Mathf.Abs(s));

        return max;
    }

    /// <summary>
    /// Temporarily disables mic listening after a ping is triggered.
    /// </summary>
    private IEnumerator PauseMicListening(float duration)
    {
        isListening = false;
        yield return new WaitForSeconds(duration);
        isListening = true;
        LogDebug("Mic listening resumed.");
    }

    /// <summary>
    /// Logs debug messages if debugging is enabled.
    /// </summary>
    private void LogDebug(string msg)
    {
        if (!isDebugging) return;
        Debug.Log($"[MicPingTrigger] {msg}");
    }
}
