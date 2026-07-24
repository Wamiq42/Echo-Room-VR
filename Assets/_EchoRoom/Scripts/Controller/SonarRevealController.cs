using UnityEngine;

/// <summary>
/// Drives the EchoRoom/EchoSonarReveal shader. Each ping records a soft radial "reveal
/// pulse" at the player's origin; the GPU lights up the nearby area showing real texture,
/// then fades it back to black. Pulses are pushed to the shader as a single global array,
/// so there is no per-frame CPU work and surfaces spawned after start are covered too.
///
/// Subscribes directly to <see cref="PingEmitter.OnPingEmitted"/> — independent of the
/// legacy EchoPulseController. PingEmitter also calls RevealGlobal directly so reveal still
/// works if this scene component is inactive or not yet subscribed.
/// </summary>
public class SonarRevealController : MonoBehaviour
{
    // Must match ECHO_MAX_PULSES in EchoSonarReveal.shader.
    private const int MaxPulses = 16;

    private static readonly int PulsesId = Shader.PropertyToID("_SonarPulses");
    private static readonly int PulseRangesId = Shader.PropertyToID("_SonarPulseRanges");
    private static readonly int RevealLingerOverrideId = Shader.PropertyToID("_EchoRevealLingerOverride");
    private static readonly int AfterWaveCountId = Shader.PropertyToID("_EchoAfterWaveCount");
    private static readonly int AfterWaveDelayId = Shader.PropertyToID("_EchoAfterWaveDelay");
    private static readonly int AfterWaveStrengthId = Shader.PropertyToID("_EchoAfterWaveStrength");
    private static readonly int AfterWaveStrengthDecayId = Shader.PropertyToID("_EchoAfterWaveStrengthDecay");
    private static readonly int AfterWaveWidthScaleId = Shader.PropertyToID("_EchoAfterWaveWidthScale");
    private static readonly Vector4[] SharedPulses = new Vector4[MaxPulses];
    private static readonly Vector4[] SharedPulseRanges = new Vector4[MaxPulses];
    private static int sharedNextIndex;
    private static int lastRevealFrame = -1;
    private static Vector3 lastRevealOrigin;
    private static bool sharedBufferInitialized;

    [Tooltip("The PingEmitter that fires OnPingEmitted. Each ping triggers a reveal pulse.")]
    [SerializeField] private PingEmitter pingEmitter;

    [Header("Reveal Timing")]
    [Tooltip("How long revealed environment surfaces linger after the ping wave passes them. This overrides individual material linger values.")]
    [SerializeField, Min(0.05f)] private float revealLingerSeconds = 2.25f;

    [Header("Visual After-Waves")]
    [Tooltip("Number of purely visual rings that follow the primary reveal wave. These rings never reveal surfaces or trigger gameplay.")]
    [SerializeField, Range(0, 3)] private int afterWaveCount = 2;
    [Tooltip("Time between the primary ring and each visual follower.")]
    [SerializeField, Min(0.01f)] private float afterWaveDelaySeconds = 0.18f;
    [Tooltip("Brightness of the first visual follower relative to the primary ring.")]
    [SerializeField, Range(0f, 1f)] private float afterWaveStrength = 0.10f;
    [Tooltip("Brightness multiplier applied to each successive visual follower.")]
    [SerializeField, Range(0f, 1f)] private float afterWaveStrengthDecay = 1.0f;
    [Tooltip("Follower width relative to the primary ring width.")]
    [SerializeField, Range(0.05f, 1.5f)] private float afterWaveWidthScale = 0.65f;

    private void Awake()
    {
        // Clear the global buffer before the first frame renders so no phantom reveal
        // appears at world origin while every slot is still zero.
        ResetGlobalPulses();
        ApplyRevealSettings();
    }

    private void OnEnable()
    {
        ApplyRevealSettings();

        if (pingEmitter != null)
            pingEmitter.OnPingEmitted += HandlePing;
    }

    private void OnDisable()
    {
        if (pingEmitter != null)
            pingEmitter.OnPingEmitted -= HandlePing;
    }

    private void HandlePing(Vector3 origin) => Reveal(origin);

    public void ApplyRevealSettings()
    {
        Shader.SetGlobalFloat(RevealLingerOverrideId, Mathf.Max(0.05f, revealLingerSeconds));
        Shader.SetGlobalFloat(AfterWaveCountId, Mathf.Clamp(afterWaveCount, 0, 3));
        Shader.SetGlobalFloat(AfterWaveDelayId, Mathf.Max(0.01f, afterWaveDelaySeconds));
        Shader.SetGlobalFloat(AfterWaveStrengthId, Mathf.Clamp01(afterWaveStrength));
        Shader.SetGlobalFloat(AfterWaveStrengthDecayId, Mathf.Clamp01(afterWaveStrengthDecay));
        Shader.SetGlobalFloat(AfterWaveWidthScaleId, Mathf.Clamp(afterWaveWidthScale, 0.05f, 1.5f));
    }

    private void OnValidate()
    {
        revealLingerSeconds = Mathf.Max(0.05f, revealLingerSeconds);
        afterWaveCount = Mathf.Clamp(afterWaveCount, 0, 3);
        afterWaveDelaySeconds = Mathf.Max(0.01f, afterWaveDelaySeconds);
        afterWaveStrength = Mathf.Clamp01(afterWaveStrength);
        afterWaveStrengthDecay = Mathf.Clamp01(afterWaveStrengthDecay);
        afterWaveWidthScale = Mathf.Clamp(afterWaveWidthScale, 0.05f, 1.5f);
        ApplyRevealSettings();
    }

    /// <summary>
    /// Triggers a soft radial reveal centered on the given world-space origin.
    /// Overwrites the oldest pulse once all slots are in use.
    /// </summary>
    /// <param name="origin">World position of the reveal center (the player's body).</param>
    public void Reveal(Vector3 origin) => RevealGlobal(origin);

    public static void RevealGlobal(Vector3 origin) => RevealGlobal(origin, 0f);

    /// <summary>
    /// Adds one reveal pulse with its own maximum visual range. A non-positive range
    /// keeps the legacy material _RevealRadius fallback for non-profile callers.
    /// </summary>
    public static void RevealGlobal(Vector3 origin, float visualRange)
    {
        EnsureGlobalBuffer();

        if (lastRevealFrame == Time.frameCount && Vector3.SqrMagnitude(lastRevealOrigin - origin) < 0.0001f)
            return;

        // Shader _Time.y follows Time.time and does not reset when scenes change.
        // Using timeSinceLevelLoad makes pulses appear already expired after returning
        // from another scene (for example Maze -> Main Menu -> Tutorial).
        float startTime = Mathf.Max(Time.time, 0.0001f);

        SharedPulses[sharedNextIndex] = new Vector4(origin.x, origin.y, origin.z, startTime);
        SharedPulseRanges[sharedNextIndex] = new Vector4(Mathf.Max(0f, visualRange), 0f, 0f, 0f);
        sharedNextIndex = (sharedNextIndex + 1) % MaxPulses;
        lastRevealFrame = Time.frameCount;
        lastRevealOrigin = origin;

        Shader.SetGlobalVectorArray(PulsesId, SharedPulses);
        Shader.SetGlobalVectorArray(PulseRangesId, SharedPulseRanges);
    }

    private static void EnsureGlobalBuffer()
    {
        if (sharedBufferInitialized)
            return;

        ResetGlobalPulses();
    }

    private static void ResetGlobalPulses()
    {
        for (int i = 0; i < MaxPulses; i++)
        {
            SharedPulses[i] = Vector4.zero;
            SharedPulseRanges[i] = Vector4.zero;
        }

        sharedNextIndex = 0;
        lastRevealFrame = -1;
        lastRevealOrigin = Vector3.zero;
        sharedBufferInitialized = true;
        Shader.SetGlobalVectorArray(PulsesId, SharedPulses);
        Shader.SetGlobalVectorArray(PulseRangesId, SharedPulseRanges);
    }
}
