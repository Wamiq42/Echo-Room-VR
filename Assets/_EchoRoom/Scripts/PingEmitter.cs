using UnityEngine;
using UnityEngine.InputSystem;

public class PingEmitter : MonoBehaviour
{
    [Header("Logger")]
    [SerializeField] private bool isDebugging = false;

    [Header("Ping Settings")]
    [SerializeField] private float pingRange = 10f;
    [SerializeField] private float pingRadius = 0.1f;
    [SerializeField] private LayerMask pingLayers;

    [Header("Input")]
    [SerializeField] private InputActionProperty pingAction;

    [Header("Feedback")]
    [SerializeField] private AudioSource pingSound;
    [SerializeField] private GameObject hitEffectPrefab;

    // NEW: Event to notify player controller
    public event System.Action<Vector3> OnPingEmitted;

    private void OnEnable()
    {
        pingAction.action.performed += OnPingPerformed;
    }

    private void OnDisable()
    {
        pingAction.action.performed -= OnPingPerformed;
    }

    private void OnPingPerformed(InputAction.CallbackContext ctx)
    {
        EmitPing();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LogDebug("Editor test key pressed (SPACE)");
            EmitPing();
        }
    }
#endif

    private void EmitPing()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit hit;
        bool hitSomething = Physics.SphereCast(origin, pingRadius, direction, out hit, pingRange, pingLayers);

        if (pingSound != null)
            pingSound.Play();

        if (hitSomething)
        {
            LogDebug("Ping hit: " + hit.collider.name);
            if (hitEffectPrefab)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
        }

        // NEW: Notify other systems
        OnPingEmitted?.Invoke(origin);
    }

    private void LogDebug(string msg)
    {
        if (!isDebugging) return;
        Debug.Log($"[PingEmitter] {msg}");
    }
}