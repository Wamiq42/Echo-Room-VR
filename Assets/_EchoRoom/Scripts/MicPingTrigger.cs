using EchoRoom.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class MicPingTrigger : MonoBehaviour
{
    private const int SampleWindow = 128;

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
    private bool cooldownWaitLogged;
    private PingEmitter subscribedPingEmitter;
    private bool pingEmittedDuringRequest;
#if UNITY_ANDROID && !UNITY_EDITOR
    private bool permissionRequestPending;
    private bool permissionRefused;
#endif

    public bool IsPushToTalkHeld { get; private set; }
    public bool IsMicrophoneRecording =>
        microphoneDevice != null && Microphone.IsRecording(microphoneDevice);

    private void OnEnable()
    {
        CreatePushToTalkAction();
        SubscribeToPingEmitter();
        EchoRoomSettings.Changed += OnSettingChanged;
    }

    private void OnSettingChanged(EchoRoomSetting setting)
    {
        if (setting != EchoRoomSetting.Handedness)
            return;

        // Push-to-talk is a held control, so a live hold must be torn down with the old binding
        // or the microphone stays open on a controller that no longer owns the shout.
        StopMicrophoneCapture();
        IsPushToTalkHeld = false;
        wasPushToTalkHeld = false;
        DestroyPushToTalkAction();
        CreatePushToTalkAction();
    }

    private void Update()
    {
        bool held = Time.timeScale > 0f && pushToTalkAction != null && pushToTalkAction.IsPressed();
        IsPushToTalkHeld = held;

        if (held && !wasPushToTalkHeld)
        {
            pingAttemptedThisHold = false;
            cooldownWaitLogged = false;
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

        TryEmitPing(volume);
    }

    /// <summary>
    /// One threshold crossing is allowed per hold, but the attempt is only spent once
    /// the emitter actually fired. RequestPing reports the shared lockout whether it
    /// accepted or rejected the request, so the returned float cannot tell the two
    /// apart; the emitter's own event can. A rejected shout therefore keeps polling
    /// and pings the instant the cooldown expires, instead of stranding a player who
    /// is still holding Y.
    /// </summary>
    private void TryEmitPing(float volume)
    {
        SubscribeToPingEmitter();

        pingEmittedDuringRequest = false;
        float waitDuration = PingEmitter.RequestPing?.Invoke() ?? 0f;

        // With no emitter to listen to, the outcome is unknowable; spend the attempt
        // rather than risk re-requesting every checkInterval for the whole hold.
        bool pingEmitted = subscribedPingEmitter == null || pingEmittedDuringRequest;
        if (pingEmitted)
        {
            pingAttemptedThisHold = true;
            LogDebug(
                $"Push-to-talk volume {volume:F3} exceeded {sensitivity:F3}; " +
                $"ping emitted and shared wait is {waitDuration:F2}s.");
            return;
        }

        if (cooldownWaitLogged)
            return;

        cooldownWaitLogged = true;
        LogDebug(
            $"Push-to-talk volume {volume:F3} exceeded {sensitivity:F3} but the shared " +
            $"cooldown owns the ping; retrying while held, {waitDuration:F2}s remaining.");
    }

    private void OnDisable()
    {
        IsPushToTalkHeld = false;
        wasPushToTalkHeld = false;
        pingAttemptedThisHold = false;
        cooldownWaitLogged = false;
        StopMicrophoneCapture();
        UnsubscribeFromPingEmitter();
        EchoRoomSettings.Changed -= OnSettingChanged;
        DestroyPushToTalkAction();
    }

    private void CreatePushToTalkAction()
    {
        if (pushToTalkAction != null)
            return;

        pushToTalkAction = new InputAction("Microphone Push To Talk", InputActionType.Button);
        pushToTalkAction.AddBinding(HandedInput.MicrophonePingPath);
#if UNITY_EDITOR
        pushToTalkAction.AddBinding("<Keyboard>/v");
#endif
        pushToTalkAction.Enable();
    }

    private void DestroyPushToTalkAction()
    {
        if (pushToTalkAction == null)
            return;

        pushToTalkAction.Disable();
        pushToTalkAction.Dispose();
        pushToTalkAction = null;
    }

    private void SubscribeToPingEmitter()
    {
        if (subscribedPingEmitter != null)
            return;

        subscribedPingEmitter = FindFirstObjectByType<PingEmitter>();
        if (subscribedPingEmitter == null)
            return;

        subscribedPingEmitter.OnPingEmitted += HandlePingEmitted;
    }

    private void UnsubscribeFromPingEmitter()
    {
        if (subscribedPingEmitter != null)
            subscribedPingEmitter.OnPingEmitted -= HandlePingEmitted;

        subscribedPingEmitter = null;
    }

    private void HandlePingEmitted(Vector3 origin)
    {
        // Raised synchronously from inside RequestPing, so this doubles as the receipt
        // for the request currently in flight.
        pingEmittedDuringRequest = true;
    }

    private void StartMicrophoneCapture()
    {
        if (IsMicrophoneRecording)
            return;

        if (!HasMicrophonePermission())
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

    /// <summary>
    /// Android gates RECORD_AUDIO behind a runtime grant, and until it is given
    /// Microphone.devices stays empty. The dialog resolves frames later through
    /// PermissionCallbacks, so this hold simply captures nothing and the next press
    /// picks it up; nothing here blocks or assumes an immediate answer.
    /// </summary>
    private bool HasMicrophonePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            return true;

        // At most one request per session, so a refusal cannot re-prompt on every press.
        if (permissionRequestPending || permissionRefused)
            return false;

        permissionRequestPending = true;

        PermissionCallbacks callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += OnMicrophonePermissionGranted;
        callbacks.PermissionDenied += OnMicrophonePermissionRefused;
        callbacks.PermissionDeniedAndDontAskAgain += OnMicrophonePermissionRefused;
        Permission.RequestUserPermission(Permission.Microphone, callbacks);

        LogDebug("Requested the Android microphone permission.");
        return false;
#else
        // Editor and standalone rely on the OS-level microphone grant directly.
        return true;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnMicrophonePermissionGranted(string permissionName)
    {
        permissionRequestPending = false;
        LogDebug($"Android permission '{permissionName}' granted; hold push-to-talk again to capture.");
    }

    private void OnMicrophonePermissionRefused(string permissionName)
    {
        permissionRequestPending = false;
        permissionRefused = true;
        Debug.LogWarning(
            $"[MicPingTrigger] Android permission '{permissionName}' was denied; microphone pings stay " +
            "unavailable until it is granted from the system app permission settings.");
    }
#endif

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
