using UnityEngine;
using UnityEngine.InputSystem;

public class PingEmitter : MonoBehaviour
{
    [Header("Logger")]
    [SerializeField] private bool isDebugging = false;

    [Header("Ping Settings")]
    [SerializeField] private float pingRange = 10f;               // Max distance the ping can travel
    [SerializeField] private float pingRadius = 0.1f;             // Radius of the spherecast
    [SerializeField] private LayerMask pingLayers;                // What layers the ping can interact with

    [Header("Input")]
    [SerializeField] private InputActionProperty pingAction;      // Input binding (e.g., trigger press)

    [Header("Feedback")]
    [SerializeField] private AudioSource pingSound;               // Sound played when ping is emitted
    [SerializeField] private GameObject hitEffectPrefab;          // Optional VFX when ping hits something

    private void Update()
    {
        // Runtime input (VR controller)
        if (pingAction.action.WasPressedThisFrame())
        {
            EmitPing();
        }

#if UNITY_EDITOR
        // Editor-only testing with keyboard
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LogDebug("Editor test key pressed (SPACE)");
            EmitPing();
        }
#endif
    }


    private void EmitPing()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        // Spherecast forward from the controller to detect hits
        RaycastHit hit;
        bool hitSomething = Physics.SphereCast(origin, pingRadius, direction, out hit, pingRange, pingLayers);

        // Play ping sound
        if (pingSound != null)
            pingSound.Play();

        // If something was hit, log it and show feedback
        if (hitSomething)
        {
            LogDebug("Ping hit: " + hit.collider.name);

            if (hitEffectPrefab)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
        }
    }

    private void LogDebug(string msg)
    {
        if (!isDebugging) return;

        Debug.Log($"[PingEmitter] {msg}");
    }
}