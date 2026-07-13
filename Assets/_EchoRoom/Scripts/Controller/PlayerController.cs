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
    /// <summary>
    /// Moves the player rig so the tracked headset/camera lands on the spawn point.
    /// </summary>
    public void MovePlayerToSpawn(Vector3 position, Quaternion rotation)
    {
        if (xrOrigin != null)
        {
            CharacterController[] controllers = xrOrigin.GetComponentsInChildren<CharacterController>();
            foreach (CharacterController controller in controllers)
                controller.enabled = false;

            Transform headTransform = Camera.main != null && Camera.main.transform.IsChildOf(xrOrigin)
                ? Camera.main.transform
                : xrOrigin;

            Quaternion targetRotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
            float headYaw = headTransform != null ? headTransform.eulerAngles.y : xrOrigin.eulerAngles.y;
            float yawDelta = Mathf.DeltaAngle(headYaw, targetRotation.eulerAngles.y);
            xrOrigin.RotateAround(headTransform.position, Vector3.up, yawDelta);
            Physics.SyncTransforms();

            Vector3 trackedPoint = headTransform != null ? headTransform.position : xrOrigin.position;
            Vector3 offset = trackedPoint - xrOrigin.position;
            offset.y = 0f;

            Vector3 rigPosition = position - offset;
            rigPosition.y = position.y;
            xrOrigin.position = rigPosition;
            Physics.SyncTransforms();

            foreach (CharacterController controller in controllers)
                controller.enabled = true;

            Log("Moved player headset to spawn point.");
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
