using System;
using UnityEngine;
using UnityEngine.XR;

public enum HapticHand
{
    None,
    Left,
    Right,
    Both
}

public enum HapticReason
{
    Ping,
    Button,
    Lever,
    WallTouch,
    EntityProximity
}

public readonly struct HapticRequest
{
    public HapticRequest(HapticHand hand, float amplitude, float duration, HapticReason reason)
    {
        Hand = hand;
        Amplitude = amplitude;
        Duration = duration;
        Reason = reason;
    }

    public HapticHand Hand { get; }
    public float Amplitude { get; }
    public float Duration { get; }
    public HapticReason Reason { get; }
}

/// <summary>
/// Central, allocation-free haptic output for the cached left and right XR devices.
/// Device references are refreshed only during initialization, connection changes,
/// or a request made while the corresponding cached device is invalid.
/// </summary>
public static class EchoHaptics
{
    public const float PingAmplitude = 0.3f;
    public const float PingDuration = 0.15f;
    public const float ButtonAmplitude = 0.5f;
    public const float ButtonDuration = 0.1f;
    public const float LeverAmplitude = 0.4f;
    public const float LeverDuration = 0.2f;
    public const float WallTouchAmplitude = 0.2f;
    public const float WallTouchDuration = 0.05f;

    private const float MaxDuration = 1f;

    private static InputDevice _leftDevice;
    private static InputDevice _rightDevice;
    private static bool _initialized;

    /// <summary>
    /// Diagnostic hook raised once per logical request, even when no haptic-capable
    /// hardware is connected. It allows Editor verification without simulating a device.
    /// </summary>
    public static event Action<HapticRequest> HapticRequested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        if (_initialized)
        {
            InputDevices.deviceConnected -= HandleDeviceConnected;
            InputDevices.deviceDisconnected -= HandleDeviceDisconnected;
        }

        _leftDevice = default;
        _rightDevice = default;
        _initialized = false;
        HapticRequested = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        EnsureInitialized();
    }

    public static void PlayPing(HapticHand hand = HapticHand.Right)
    {
        Request(hand, PingAmplitude, PingDuration, HapticReason.Ping);
    }

    public static void PlayButton(HapticHand hand)
    {
        Request(hand, ButtonAmplitude, ButtonDuration, HapticReason.Button);
    }

    public static void PlayLever(HapticHand hand)
    {
        Request(hand, LeverAmplitude, LeverDuration, HapticReason.Lever);
    }

    public static void PlayWallTouch(HapticHand hand)
    {
        Request(hand, WallTouchAmplitude, WallTouchDuration, HapticReason.WallTouch);
    }

    public static void Request(HapticHand hand, float amplitude, float duration, HapticReason reason)
    {
        if (hand == HapticHand.None)
            return;

        EnsureInitialized();

        float safeAmplitude = Mathf.Clamp01(amplitude);
        float safeDuration = Mathf.Clamp(duration, 0f, MaxDuration);
        if (safeAmplitude <= 0f || safeDuration <= 0f)
            return;

        HapticRequested?.Invoke(new HapticRequest(hand, safeAmplitude, safeDuration, reason));

        if (hand == HapticHand.Left || hand == HapticHand.Both)
            SendImpulse(ref _leftDevice, XRNode.LeftHand, safeAmplitude, safeDuration);

        if (hand == HapticHand.Right || hand == HapticHand.Both)
            SendImpulse(ref _rightDevice, XRNode.RightHand, safeAmplitude, safeDuration);
    }

    public static void Stop(HapticHand hand)
    {
        EnsureInitialized();

        if (hand == HapticHand.Left || hand == HapticHand.Both)
            StopDevice(ref _leftDevice, XRNode.LeftHand);

        if (hand == HapticHand.Right || hand == HapticHand.Both)
            StopDevice(ref _rightDevice, XRNode.RightHand);
    }

    private static void EnsureInitialized()
    {
        if (_initialized)
            return;

        _initialized = true;
        InputDevices.deviceConnected += HandleDeviceConnected;
        InputDevices.deviceDisconnected += HandleDeviceDisconnected;
        RefreshDevice(XRNode.LeftHand, ref _leftDevice);
        RefreshDevice(XRNode.RightHand, ref _rightDevice);
    }

    private static void HandleDeviceConnected(InputDevice device)
    {
        InputDeviceCharacteristics characteristics = device.characteristics;
        if ((characteristics & InputDeviceCharacteristics.Controller) == 0)
            return;

        if ((characteristics & InputDeviceCharacteristics.Left) != 0)
            _leftDevice = device;
        if ((characteristics & InputDeviceCharacteristics.Right) != 0)
            _rightDevice = device;
    }

    private static void HandleDeviceDisconnected(InputDevice device)
    {
        if (_leftDevice.Equals(device))
            RefreshDevice(XRNode.LeftHand, ref _leftDevice);
        if (_rightDevice.Equals(device))
            RefreshDevice(XRNode.RightHand, ref _rightDevice);
    }

    private static void SendImpulse(ref InputDevice device, XRNode node, float amplitude, float duration)
    {
        if (!device.isValid)
            RefreshDevice(node, ref device);

        if (!device.isValid ||
            !device.TryGetHapticCapabilities(out HapticCapabilities capabilities) ||
            !capabilities.supportsImpulse)
        {
            return;
        }

        device.SendHapticImpulse(0u, amplitude, duration);
    }

    private static void StopDevice(ref InputDevice device, XRNode node)
    {
        if (!device.isValid)
            RefreshDevice(node, ref device);

        if (device.isValid)
            device.StopHaptics();
    }

    private static void RefreshDevice(XRNode node, ref InputDevice device)
    {
        device = InputDevices.GetDeviceAtXRNode(node);
    }
}
