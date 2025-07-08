using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Subsystems")]
    [SerializeField] private PingEmitter pingEmitter;
    [SerializeField] private EchoPulseController echoPulseController;

    private void Awake()
    {
        // Optional safety check
        if (pingEmitter == null || echoPulseController == null)
            Debug.LogWarning("[PlayerController] Missing subsystem references.");
    }

    private void OnEnable()
    {
        pingEmitter.OnPingEmitted += HandlePing;
    }

    private void OnDisable()
    {
        pingEmitter.OnPingEmitted -= HandlePing;
    }

    private void HandlePing(Vector3 origin)
    {
        echoPulseController.TriggerPing(origin);
    }
}