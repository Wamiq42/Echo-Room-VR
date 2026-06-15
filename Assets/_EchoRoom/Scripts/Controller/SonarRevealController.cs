using UnityEngine;

/// <summary>
/// Drives the EchoRoom/EchoSonarReveal shader. Each ping records a soft radial "reveal
/// pulse" at the player's origin; the GPU lights up the nearby area showing real texture,
/// then fades it back to black. Pulses are pushed to the shader as a single global array,
/// so there is no per-frame CPU work and surfaces spawned after start are covered too.
///
/// Subscribes directly to <see cref="PingEmitter.OnPingEmitted"/> — independent of the
/// legacy EchoPulseController.
/// </summary>
public class SonarRevealController : MonoBehaviour
{
    // Must match ECHO_MAX_PULSES in EchoSonarReveal.shader.
    private const int MaxPulses = 16;

    private static readonly int PulsesId = Shader.PropertyToID("_SonarPulses");

    [Tooltip("The PingEmitter that fires OnPingEmitted. Each ping triggers a reveal pulse.")]
    [SerializeField] private PingEmitter pingEmitter;

    // xyz = world origin, w = start time (seconds). w <= 0 marks an empty slot.
    private readonly Vector4[] _pulses = new Vector4[MaxPulses];
    private int _nextIndex;

    private void Awake()
    {
        // Clear the global buffer before the first frame renders so no phantom reveal
        // appears at world origin while every slot is still zero.
        for (int i = 0; i < MaxPulses; i++)
            _pulses[i] = Vector4.zero;

        Shader.SetGlobalVectorArray(PulsesId, _pulses);
    }

    private void OnEnable()
    {
        if (pingEmitter != null)
            pingEmitter.OnPingEmitted += HandlePing;
    }

    private void OnDisable()
    {
        if (pingEmitter != null)
            pingEmitter.OnPingEmitted -= HandlePing;
    }

    private void HandlePing(Vector3 origin) => Reveal(origin);

    /// <summary>
    /// Triggers a soft radial reveal centered on the given world-space origin.
    /// Overwrites the oldest pulse once all slots are in use.
    /// </summary>
    /// <param name="origin">World position of the reveal center (the player's body).</param>
    public void Reveal(Vector3 origin)
    {
        // timeSinceLevelLoad matches the shader's _Time.y; clamp above 0 so the slot
        // never reads as empty on the very first frame.
        float startTime = Mathf.Max(Time.timeSinceLevelLoad, 0.0001f);

        _pulses[_nextIndex] = new Vector4(origin.x, origin.y, origin.z, startTime);
        _nextIndex = (_nextIndex + 1) % MaxPulses;

        Shader.SetGlobalVectorArray(PulsesId, _pulses);
    }
}
