using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool enableDebugging = false;  // ✅ debug toggle on top

    [Header("Subsystems")]
    [SerializeField] private PingEmitter pingEmitter;
    [SerializeField] private EchoPulseController echoPulseController;
    [SerializeField] private Transform xrOrigin;  // renamed to match conventions

    private void Awake()
    {
        if (pingEmitter == null || echoPulseController == null)
            Log("Missing subsystem references.");
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

    private void HandlePing(Vector3 origin)
    {
        if (echoPulseController != null)
            echoPulseController.TriggerPing(origin);
        else
            Log("EchoPulseController not assigned.");
    }

    /// <summary>
    /// Moves the player rig (XR Origin) to the given spawn location and rotation.
    /// </summary>
    public void MovePlayerToSpawn(Vector3 position, Quaternion rotation)
    {
        if (xrOrigin != null)
        {
            xrOrigin.SetPositionAndRotation(position, rotation);
            Log("Moved player to spawn point.");
        }
        else
        {
            Log("No XR Origin found to move!");
        }
    }

    // ---------- Debugging ----------
    private void Log(string message)
    {
        if (enableDebugging)
            Debug.Log($"[PlayerController] {message}");
    }
}