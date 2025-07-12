using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PingEmitter : MonoBehaviour
{
    [Header("Logger")]
    [SerializeField] private bool isDebugging = false;

    [Header("Ping Settings")]
    [SerializeField] private float pingRadius = 0.1f;
    [SerializeField] private LayerMask pingLayers;

    [Header("Input")]
    [SerializeField] private InputActionProperty pingAction;

    [Header("Feedback")]
    [SerializeField] private AudioSource pingSound;
    [SerializeField] private GameObject echoSoundPrefab;
    [SerializeField] private ParticleSystem pingRippleFX;
    [SerializeField] private Transform playerOrigin;
    [SerializeField] private float sphereCastRadius = 0.2f;
    [SerializeField] private float maxEchoDistance = 20f;

    private float nextPingTime = 0f;
    private float echoClipLength = 0f;
    public static Func<float> RequestPing;
    public static Action PlayPingSound;
    public event Action<Vector3> OnPingEmitted;

#if UNITY_EDITOR
    private Vector3 debugHitPoint;
    private Vector3 debugDirection;
    private float debugDistance;
#endif

    private void Awake()
    {
        echoClipLength = echoSoundPrefab.GetComponent<AudioSource>().clip.length;
    }

    private void OnEnable()
    {
        pingAction.action.performed += OnPingPerformed;
        PlayPingSound += PingSound;
        RequestPing += TryEmitFromExternal;
    }

    private void OnDisable()
    {
        pingAction.action.performed -= OnPingPerformed;
        PlayPingSound -= PingSound;
        RequestPing -= TryEmitFromExternal;
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
    /// Called externally by other systems (e.g. mic trigger) to attempt a ping.
    /// Respects the internal cooldown timer.
    /// </summary>
    private float TryEmitFromExternal()
    {
        if (Time.time < nextPingTime)
            return 0f;

        EmitPing();
        return Mathf.Max(0f, nextPingTime - Time.time); // return remaining duration
    }

    /// <summary>
    /// Emits a ping using overlap detection and triggers directional echo feedback.
    /// Applies internal cooldown to prevent rapid reuse.
    /// </summary>
    private void EmitPing()
    {
        if (Time.time < nextPingTime)
            return;

        Vector3 origin = transform.position;

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

        float echoDelay = EmitDirectionalEcho();

        if (pingSound != null && pingSound.clip != null)
            pingSound.Play();

        float echoDuration = echoClipLength;

        nextPingTime = Time.time + echoDelay + echoDuration;
    }

    /// <summary>
    /// Sends a forward-facing echo pulse using a spherecast.
    /// If it hits, plays echo audio after a delay based on distance.
    /// </summary>
    private float EmitDirectionalEcho()
    {
        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = transform.forward;

        Ray ray = new Ray(origin, direction);
        PlayDetachedParticle(ray);

        if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, maxEchoDistance, pingLayers))
        {
            float distance = hit.distance;
            float delay = distance / 343f;

            LogDebug($"Echo hit: {hit.collider.name} at {distance:F2}m (delay: {delay:F2}s)");
            StartCoroutine(PlayEchoAfterDelay(hit.point, delay));


            PlayDetachedParticle(ray);
            Debug.DrawRay(ray.origin, ray.direction * maxEchoDistance, Color.cyan, 1.0f);
            
#if UNITY_EDITOR
            debugHitPoint = hit.point;
            debugDirection = transform.forward;
            debugDistance = distance;
#endif
            return delay;
        }
        return 0f;
    }


    /// <summary>
    /// Plays the echo sound prefab at a location after a timed delay.
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

            Destroy(echo, 5f);
        }
    }

    /// <summary>
    /// Plays the ping audio immediately if assigned.
    /// </summary>
    private void PingSound()
    {
        if (pingSound != null)
            pingSound.Play();
    }

    /// <summary>
    /// Detaches the ripple effect particle system, positions it along the ray, and plays it.
    /// </summary>
    private void PlayDetachedParticle(Ray ray)
    {
        if (pingRippleFX == null) return;

        Transform particleTransform = pingRippleFX.transform;
        Transform originalParent = particleTransform.parent;

        particleTransform.SetParent(null);
        particleTransform.position = ray.origin + ray.direction * 0.2f;
        particleTransform.rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(0, -90f, 0);

        pingRippleFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        pingRippleFX.Play();

        StartCoroutine(ReattachParticleAfterDelay(pingRippleFX.main.duration));
    }

    /// <summary>
    /// Reattaches the particle system to this object after playing.
    /// </summary>
    private IEnumerator ReattachParticleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pingRippleFX != null)
        {
            pingRippleFX.transform.SetParent(transform);
        }
    }

    /// <summary>
    /// Logs debug messages if enabled.
    /// </summary>
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
