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
    private static readonly int RevealLingerOverrideId = Shader.PropertyToID("_EchoRevealLingerOverride");
    private static readonly Vector4[] SharedPulses = new Vector4[MaxPulses];
    private static int sharedNextIndex;
    private static int lastRevealFrame = -1;
    private static Vector3 lastRevealOrigin;
    private static bool sharedBufferInitialized;

    [Tooltip("The PingEmitter that fires OnPingEmitted. Each ping triggers a reveal pulse.")]
    [SerializeField] private PingEmitter pingEmitter;

    [Header("Reveal Timing")]
    [Tooltip("How long revealed environment surfaces linger after the ping wave passes them. This overrides individual material linger values.")]
    [SerializeField, Min(0.05f)] private float revealLingerSeconds = 2.25f;

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
    }

    private void OnValidate()
    {
        revealLingerSeconds = Mathf.Max(0.05f, revealLingerSeconds);
        ApplyRevealSettings();
    }

    /// <summary>
    /// Triggers a soft radial reveal centered on the given world-space origin.
    /// Overwrites the oldest pulse once all slots are in use.
    /// </summary>
    /// <param name="origin">World position of the reveal center (the player's body).</param>
    public void Reveal(Vector3 origin) => RevealGlobal(origin);

    public static void RevealGlobal(Vector3 origin)
    {
        EnsureGlobalBuffer();

        if (lastRevealFrame == Time.frameCount && Vector3.SqrMagnitude(lastRevealOrigin - origin) < 0.0001f)
            return;

        // Shader _Time.y follows Time.time and does not reset when scenes change.
        // Using timeSinceLevelLoad makes pulses appear already expired after returning
        // from another scene (for example Maze -> Main Menu -> Tutorial).
        float startTime = Mathf.Max(Time.time, 0.0001f);

        SharedPulses[sharedNextIndex] = new Vector4(origin.x, origin.y, origin.z, startTime);
        sharedNextIndex = (sharedNextIndex + 1) % MaxPulses;
        lastRevealFrame = Time.frameCount;
        lastRevealOrigin = origin;

        Shader.SetGlobalVectorArray(PulsesId, SharedPulses);
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
            SharedPulses[i] = Vector4.zero;

        sharedNextIndex = 0;
        lastRevealFrame = -1;
        lastRevealOrigin = Vector3.zero;
        sharedBufferInitialized = true;
        Shader.SetGlobalVectorArray(PulsesId, SharedPulses);
    }
}
