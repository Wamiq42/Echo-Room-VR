using System.Collections;
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
    
    [SerializeField] private GameObject echoSoundPrefab;
    
    [SerializeField] private ParticleSystem pingRippleFX;
    [SerializeField] private Transform playerOrigin; // Assign this in the inspector
    [SerializeField] private float sphereCastRadius = 0.2f;
    [SerializeField] private float maxEchoDistance = 20f;
    
    private float nextPingTime = 0f;
    
#if UNITY_EDITOR
    private Vector3 debugHitPoint;
    private Vector3 debugDirection;
    private float debugDistance;
#endif
    
    // NEW: Event to notify player controller
    public event System.Action<Vector3> OnPingEmitted;
    public static System.Action PlayPingSound;

    private void OnEnable()
    {
        pingAction.action.performed += OnPingPerformed;
        PlayPingSound += PingSound;
    }

    private void OnDisable()
    {
        pingAction.action.performed -= OnPingPerformed;
        PlayPingSound -= PingSound;
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
    
    /// <summary>
    /// Emits a short-range ping using OverlapSphere to detect nearby objects,
    /// triggers visual hit effects, and notifies listeners.
    /// Also calls EmitDirectionalEcho() for forward-facing echo feedback.
    /// </summary>
    private void EmitPing()
    {
        if (Time.time < nextPingTime)
            return;
        
        Vector3 origin = transform.position;

        // OverlapSphere detects ALL colliders within the radius
        Collider[] hits = Physics.OverlapSphere(origin, pingRadius, pingLayers);
    
        foreach (var hit in hits)
        {
            LogDebug("Ping detected: " + hit.name);
            OnPingEmitted?.Invoke(origin);
        }

       

#if UNITY_EDITOR
        debugHitPoint = origin;
        debugDistance = pingRadius;
#endif
        EmitDirectionalEcho();
        
        if (pingSound != null && pingSound.clip != null)
        {
            nextPingTime = Time.time + pingSound.clip.length;
        }
    }
    
    /// <summary>
    /// Performs a directional SphereCast in the forward direction.
    /// If an object is hit, calculates the echo delay based on distance
    /// and plays a delayed echo sound at the hit point.
    /// </summary>
    private void EmitDirectionalEcho()
    {
        // Ray starts from player origin, but goes in controller's forward direction
        Vector3 origin = playerOrigin.position;
        Vector3 direction = transform.forward;

        Ray ray = new Ray(origin, direction);
        PlayDetachedParticle(ray);
        if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, maxEchoDistance, pingLayers))
        {
            float distance = hit.distance;
            float delay = distance / 343f; // speed of sound

            LogDebug($"Echo hit: {hit.collider.name} at {distance:F2}m (delay: {delay:F2}s)");
            StartCoroutine(PlayEchoAfterDelay(hit.point, delay));

#if UNITY_EDITOR
            debugHitPoint = hit.point;
            debugDirection = transform.forward;
            debugDistance = distance;
#endif
        }
        PlayDetachedParticle(ray);
        
        // DEBUG: Draw the ray in the Scene view
        Debug.DrawRay(ray.origin, ray.direction * maxEchoDistance, Color.cyan, 1.0f);
    }
    
    /// <summary>
    /// Waits for a time delay based on hit distance, then instantiates
    /// and plays an echo sound at the specified world position.
    /// </summary>
    private IEnumerator PlayEchoAfterDelay(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (echoSoundPrefab != null)
        {
            GameObject echo = Instantiate(echoSoundPrefab, position, Quaternion.identity);
            AudioSource src = echo.GetComponent<AudioSource>();
            if (src != null)
                src.Play();

            Destroy(echo, 5f); // cleanup
        }
    }
    private void PingSound()
    {
        if (pingSound != null)
            pingSound.Play();
    }
  
    private void PlayDetachedParticle(Ray ray)
    {
        if (pingRippleFX == null) return;

        Transform particleTransform = pingRippleFX.transform;

        // Store parent
        Transform originalParent = particleTransform.parent;

        // Detach it so it doesn’t move with the controller
        particleTransform.SetParent(null);

        // Set position only — we already rotated the prefab manually in Unity
        particleTransform.position = ray.origin + ray.direction * 0.2f;

        // Rotation with -90° Y offset to match prefab orientation
        Quaternion forwardRotation = Quaternion.LookRotation(ray.direction);
        Quaternion offset = Quaternion.Euler(0, -90f, 0);
        particleTransform.rotation = forwardRotation * offset;
        
        // Clear and play
        pingRippleFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        pingRippleFX.Play();

        // Reattach after it's finished
        StartCoroutine(ReattachParticleAfterDelay(pingRippleFX.main.duration));
    }



    private IEnumerator ReattachParticleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pingRippleFX != null)
        {
            pingRippleFX.transform.SetParent(transform);
        }
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