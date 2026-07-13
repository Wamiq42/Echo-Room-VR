using UnityEngine;

/// <summary>
/// Drives the EchoRoom/EchoPulseSonar shader. Each ping is an expanding spherical
/// shell centered on the player's body. Ping origins + start times are pushed to the
/// shader as a single global array; the GPU derives each shell's radius from elapsed
/// time, so no per-frame CPU work or per-renderer setup is needed and surfaces spawned
/// after start are covered automatically.
/// </summary>
public class EchoPulseController : MonoBehaviour
{
    // Must match ECHO_MAX_PINGS in EchoPulseSonar.shader.
    private const int MaxPings = 20;

    private static readonly int PingOriginsId = Shader.PropertyToID("_PingOrigins");

    // xyz = world origin, w = start time (seconds). w <= 0 marks an empty slot.
    private readonly Vector4[] _pings = new Vector4[MaxPings];
    private int _nextIndex;

    private void Awake()
    {
        // Clear the global buffer before the first frame renders so no phantom ring
        // expands from world origin while every slot is still zero.
        for (int i = 0; i < MaxPings; i++)
            _pings[i] = Vector4.zero;

        Shader.SetGlobalVectorArray(PingOriginsId, _pings);
    }

    /// <summary>
    /// Triggers a sonar shell expanding outward from the given world-space origin.
    /// Overwrites the oldest ping once all slots are in use.
    /// </summary>
    /// <param name="origin">World position of the ping center (the player's body).</param>
    public void TriggerPing(Vector3 origin)
    {
        // Shader _Time.y follows Time.time and does not reset when scenes change.
        // Keep both clocks aligned so a pulse is not immediately treated as expired.
        float startTime = Mathf.Max(Time.time, 0.0001f);

        _pings[_nextIndex] = new Vector4(origin.x, origin.y, origin.z, startTime);
        _nextIndex = (_nextIndex + 1) % MaxPings;

        Shader.SetGlobalVectorArray(PingOriginsId, _pings);

        PingEmitter.PlayPingSound?.Invoke();
    }
}
