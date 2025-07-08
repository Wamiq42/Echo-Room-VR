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

#if UNITY_EDITOR
    private Vector3 debugHitPoint;
    private Vector3 debugDirection;
    private float debugDistance;
#endif
    
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

        if (pingSound != null)
            pingSound.Play();

        // OverlapSphere detects ALL colliders within the radius
        Collider[] hits = Physics.OverlapSphere(origin, pingRadius, pingLayers);
    
        foreach (var hit in hits)
        {
            LogDebug("Ping detected: " + hit.name);
            if (hitEffectPrefab)
                Instantiate(hitEffectPrefab, hit.transform.position, Quaternion.identity);
            OnPingEmitted?.Invoke(origin);
        }

       

#if UNITY_EDITOR
        debugHitPoint = origin;
        debugDistance = pingRadius;
#endif
    }

    private void LogDebug(string msg)
    {
        if (!isDebugging) return;
        Debug.Log($"[PingEmitter] {msg}");
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!isDebugging) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(debugHitPoint, debugDistance);
    }
#endif


}