using UnityEngine;
using UnityEngine.InputSystem;

public class MicPingTrigger : MonoBehaviour
{
    private const int SampleWindow = 128;
    private const string ControllerPushToTalkBinding = "<XRController>{LeftHand}/secondaryButton";

    [Header("Debug")]
    [SerializeField] private bool isDebugging = false;

    [Header("Microphone Settings")]
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float checkInterval = 0.1f;

    private AudioClip micClip;
    private readonly float[] sampleBuffer = new float[SampleWindow];
    private InputAction pushToTalkAction;
    private string microphoneDevice;
    private float nextCheckTime;
    private bool wasPushToTalkHeld;
    private bool pingAttemptedThisHold;

    public bool IsPushToTalkHeld { get; private set; }
    public bool IsMicrophoneRecording =>
        microphoneDevice != null && Microphone.IsRecording(microphoneDevice);

    private void OnEnable()
    {
        CreatePushToTalkAction();
    }

    private void Update()
    {
        bool held = Time.timeScale > 0f && pushToTalkAction != null && pushToTalkAction.IsPressed();
        IsPushToTalkHeld = held;

        if (held && !wasPushToTalkHeld)
        {
            pingAttemptedThisHold = false;
            StartMicrophoneCapture();
        }
        else if (!held && wasPushToTalkHeld)
        {
            StopMicrophoneCapture();
        }

        wasPushToTalkHeld = held;

        if (!held || pingAttemptedThisHold || micClip == null || Time.unscaledTime < nextCheckTime)
            return;

        nextCheckTime = Time.unscaledTime + Mathf.Max(0.01f, checkInterval);
        float volume = GetMaxVolume();
        if (volume <= sensitivity)
            return;

        // One threshold crossing is allowed per hold. If another ping currently owns
        // the shared cooldown, the player releases and holds Y again to retry.
        pingAttemptedThisHold = true;
        float waitDuration = PingEmitter.RequestPing?.Invoke() ?? 0f;
        LogDebug(
            $"Push-to-talk volume {volume:F3} exceeded {sensitivity:F3}; " +
            $"shared wait is {waitDuration:F2}s.");
    }

    private void OnDisable()
    {
        IsPushToTalkHeld = false;
        wasPushToTalkHeld = false;
        pingAttemptedThisHold = false;
        StopMicrophoneCapture();

        if (pushToTalkAction != null)
        {
            pushToTalkAction.Disable();
            pushToTalkAction.Dispose();
            pushToTalkAction = null;
        }
    }

    private void CreatePushToTalkAction()
    {
        if (pushToTalkAction != null)
            return;

        pushToTalkAction = new InputAction("Microphone Push To Talk", InputActionType.Button);
        pushToTalkAction.AddBinding(ControllerPushToTalkBinding);
#if UNITY_EDITOR
        pushToTalkAction.AddBinding("<Keyboard>/v");
#endif
        pushToTalkAction.Enable();
    }

    private void StartMicrophoneCapture()
    {
        if (IsMicrophoneRecording)
            return;

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[MicPingTrigger] No microphone device found; push-to-talk is unavailable.");
            return;
        }

        microphoneDevice = Microphone.devices[0];
        micClip = Microphone.Start(microphoneDevice, true, 1, 44100);
        nextCheckTime = Time.unscaledTime;
        LogDebug($"Push-to-talk capture started on '{microphoneDevice}'.");
    }

    private void StopMicrophoneCapture()
    {
        if (microphoneDevice != null && Microphone.IsRecording(microphoneDevice))
            Microphone.End(microphoneDevice);

        micClip = null;
        microphoneDevice = null;
        LogDebug("Push-to-talk capture stopped.");
    }

    private float GetMaxVolume()
    {
        if (micClip == null || microphoneDevice == null)
            return 0f;

        int start = Microphone.GetPosition(microphoneDevice) - SampleWindow;
        if (start < 0)
            return 0f;

        float max = 0f;
        micClip.GetData(sampleBuffer, start);
        foreach (float sample in sampleBuffer)
            max = Mathf.Max(max, Mathf.Abs(sample));

        return max;
    }

    private void LogDebug(string message)
    {
        if (isDebugging)
            Debug.Log($"[MicPingTrigger] {message}");
    }
}
