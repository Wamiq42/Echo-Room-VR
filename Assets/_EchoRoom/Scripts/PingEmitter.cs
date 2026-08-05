using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PingEmitter : MonoBehaviour
{
    [Serializable]
    private struct PingProfile
    {
        [Min(0.01f)] public float visualRevealRange;
        [Min(0.01f)] public float echoCastDistance;
        [Min(0f)] public float cooldownSeconds;

        public PingProfile(float visualRevealRange, float echoCastDistance, float cooldownSeconds)
        {
            this.visualRevealRange = visualRevealRange;
            this.echoCastDistance = echoCastDistance;
            this.cooldownSeconds = cooldownSeconds;
        }
    }

    public enum PingAudioOption
    {
        CurrentSonar,
        Pulse
    }

    public enum PingInputSource
    {
        SonarControl,
        Microphone
    }

    public PingInputSource LastPingInputSource { get; private set; } = PingInputSource.SonarControl;
    [Header("Debug")]
    [SerializeField] private bool isDebugging = false;

    [Header("Ping Detection")]
    [SerializeField] private float pingRadius = 0.1f;
    [SerializeField] private LayerMask pingLayers;

    [Header("Ping Profiles")]
    [Tooltip("Sonar control / editor Space ping: visible range, directional echo cast distance, and shared lockout.")]
    [SerializeField] private PingProfile basicPingProfile = new PingProfile(10f, 10f, 2.5f);
    [Tooltip("Microphone shout ping: visible range, directional echo cast distance, and shared lockout.")]
    [SerializeField] private PingProfile microphonePingProfile = new PingProfile(16f, 16f, 4f);

    [Header("Input")]
    [SerializeField] private PlayerInputManager inputManager;
    
    [Header("Feedback")]
    [SerializeField] private AudioSource pingSound;
    [SerializeField] private PingAudioOption pingAudioOption = PingAudioOption.CurrentSonar;
    [SerializeField] private AudioClip pulseClip;
    [SerializeField] private GameObject echoSoundPrefab;
    [SerializeField] private ParticleSystem pingRippleFX;
    [SerializeField] private GameObject echoRippleFX;

    [Header("Ray Settings")]
    [SerializeField] private Transform playerOrigin;
    [SerializeField] private float sphereCastRadius = 0.2f;

    [Header("Light Feedback")]
    [SerializeField] private Light pingFlashLight;
    [SerializeField] private float flashIntensity = 2f;
    [SerializeField] private float flashRange = 3f;
    [SerializeField] private float flashDuration = 0.3f;

    private float _nextPingTime = 0f;
    private float _defaultIntensity;
    private float _defaultRange;
    private Camera _mainCamera;
    private AudioClip _currentSonarClip;

    public static Func<float> RequestPing;
    public static Action PlayPingSound;
    public event Action<Vector3> OnPingEmitted;

#if UNITY_EDITOR
    private Vector3 _debugHitPoint;
    private Vector3 _debugDirection;
    private float _debugDistance;
#endif

    private void Awake()
    {
        if (pingSound != null)
            _currentSonarClip = pingSound.clip;

        _mainCamera = Camera.main;
        SetLightDefaultValues();
    }
    
    private void OnEnable()
    {
        PlayPingSound += PingSound;
        RequestPing += TryEmitFromExternal;
    }

    private void OnDisable()
    {
        PlayPingSound -= PingSound;
        RequestPing -= TryEmitFromExternal;
    }


    private void Update()
    {
        if (inputManager != null && inputManager.ReadPing())
        {
            EmitPing();
        }
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LogDebug("Editor test key pressed (SPACE)");
            EmitPing();
        }
#endif
    }


    /// <summary>
    /// Called externally by other systems (e.g. mic trigger) to attempt a ping.
    /// Respects the internal cooldown timer.
    /// </summary>
    private float TryEmitFromExternal()
    {
        float remainingCooldown = Mathf.Max(0f, _nextPingTime - Time.time);
        if (remainingCooldown > 0f)
            return remainingCooldown;

        EmitPing(PingInputSource.Microphone);
        return Mathf.Max(0f, _nextPingTime - Time.time);
    }

    /// <summary>
    /// Clears the shared sonar/microphone lockout so the next ping is accepted immediately.
    /// The tutorial uses this: it forces a microphone ping, which arms the longer microphone
    /// cooldown, and the very next lesson asks the player to reveal with the sonar again. Without
    /// this the sonar silently no-ops (no sound, no haptic) and the lesson reads as broken.
    /// </summary>
    public void ResetCooldown()
    {
        _nextPingTime = 0f;
    }

    /// <summary>
    /// Emits a ping using overlap detection and triggers directional echo feedback.
    /// Applies internal cooldown to prevent rapid reuse.
    /// </summary>
    private void EmitPing()
    {
        EmitPing(PingInputSource.SonarControl);
    }

    private void EmitPing(PingInputSource source)
    {
        if (Time.time < _nextPingTime)
            return;

        PingProfile profile = source == PingInputSource.Microphone
            ? microphonePingProfile
            : basicPingProfile;
        float visualRevealRange = Mathf.Max(0.01f, profile.visualRevealRange);
        float echoCastDistance = Mathf.Max(0.01f, profile.echoCastDistance);
        float cooldownSeconds = Mathf.Max(0f, profile.cooldownSeconds);

        LastPingInputSource = source;
        Vector3 origin = transform.position;

        Collider[] hits = Physics.OverlapSphere(origin, pingRadius, pingLayers);
        foreach (var hit in hits)
        {
            // Check for an EchoTarget or anything implementing IEchoInteractable
            if (hit.TryGetComponent<IEchoInteractable>(out var interactable))
            {
                interactable.OnPingHit();  // ✅ Register ping on that object
            }
            
            LogDebug("Ping detected: " + hit.name);
        }

        // Write the range-aware pulse before raising the legacy origin-only event.
        // SonarRevealController's event subscription sees the same origin/frame and
        // deliberately deduplicates it without replacing this pulse's profile.
        SonarRevealController.RevealGlobal(origin, visualRevealRange);
        OnPingEmitted?.Invoke(origin);

        EmitDirectionalEcho(echoCastDistance);

        PlaySelectedPingSound();
        EchoHaptics.PlayPing(HapticHand.Right);

        if (pingFlashLight != null)
            StartCoroutine(FlashLight());

        // Both inputs share this timer. The accepted ping's profile owns the full
        // lockout, so a microphone ping cannot be bypassed by the sonar control.
        _nextPingTime = Time.time + cooldownSeconds;
        
#if UNITY_EDITOR
        _debugHitPoint = origin;
        _debugDistance = pingRadius;
#endif
    }

    /// <summary>
    /// Sends a forward-facing echo pulse using a spherecast.
    /// If it hits, plays echo audio after a delay based on distance.
    /// </summary>
    private void EmitDirectionalEcho(float maxDistance)
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        Vector3 origin = _mainCamera != null ? _mainCamera.transform.position : transform.position;
        Vector3 direction = transform.forward;

        Ray ray = new Ray(origin, direction);
        //PlayDetachedParticle(ray);

        if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, maxDistance, pingLayers))
        {
            float distance = hit.distance;
            float delay = distance / 343f;
            EchoSurfaceResponse surfaceResponse = EchoSurface.Resolve(hit.collider);
            
            LogDebug(
                $"Echo hit: {hit.collider.name} at {distance:F2}m (delay: {delay:F2}s, " +
                $"surface: {surfaceResponse.SurfaceType}, pitch: {surfaceResponse.PitchMultiplier:F2}, " +
                $"volume x{surfaceResponse.VolumeMultiplier:F2})");
            StartCoroutine(PlayEchoAfterDelay(hit.point, delay, surfaceResponse));
            StartCoroutine(GenerateEchoParticleEffect(hit, delay));

#if UNITY_EDITOR
            _debugHitPoint = hit.point;
            _debugDirection = transform.forward;
            _debugDistance = distance;
#endif
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan, 1.0f);
        }
    }

    /// <summary>
    /// Plays the echo sound prefab at a location after a timed delay.
    /// </summary>
    private IEnumerator PlayEchoAfterDelay(Vector3 position, float delay, EchoSurfaceResponse surfaceResponse)
    {
        yield return new WaitForSeconds(delay);

        if (echoSoundPrefab != null)
        {
            GameObject echo = Instantiate(echoSoundPrefab, position, Quaternion.identity);
            if (echo.TryGetComponent(out EchoSoundController controller))
            {
                controller.ConfigureAndPlay(
                    surfaceResponse.PitchMultiplier,
                    surfaceResponse.VolumeMultiplier);
            }
            else
            {
                Debug.LogWarning("[PingEmitter] Echo sound prefab is missing EchoSoundController.", echo);
                Destroy(echo);
            }
        }
    }
    /// <summary>
    /// Instantiate the echo particle prefab at a location after a timed delay.
    /// </summary>
    private IEnumerator GenerateEchoParticleEffect(RaycastHit hit, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (echoRippleFX != null)
        {
            Vector3 spawnPosition = hit.point + hit.normal * 0.05f;
            Quaternion rotation = Quaternion.LookRotation(-hit.normal);
            Instantiate(echoRippleFX, spawnPosition, rotation);
        }
    }

    /// <summary>
    /// Smoothly boosts the point light to simulate a pulse flash, then fades it back to default.
    /// </summary>
    private IEnumerator FlashLight()
    {
        pingFlashLight.intensity = flashIntensity;
        pingFlashLight.range = flashRange;

        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;

            pingFlashLight.intensity = Mathf.Lerp(flashIntensity, _defaultIntensity, t);
            pingFlashLight.range = Mathf.Lerp(flashRange, _defaultRange, t);

            yield return null;
        }

        pingFlashLight.intensity = _defaultIntensity;
        pingFlashLight.range = _defaultRange;

        LogDebug("Ping flash light smoothly reset.");
    }
    private void SetLightDefaultValues()
    {
        if (pingFlashLight != null)
        {
            _defaultIntensity = pingFlashLight.intensity;
            _defaultRange = pingFlashLight.range;
        }
    }
    /// <summary>
    /// Plays the ping audio immediately if assigned.
    /// </summary>
    private void PingSound()
    {
        PlaySelectedPingSound();
    }

    private void PlaySelectedPingSound()
    {
        if (pingSound == null)
            return;

        AudioClip selectedClip = pingAudioOption == PingAudioOption.Pulse && pulseClip != null
            ? pulseClip
            : (_currentSonarClip != null ? _currentSonarClip : pingSound.clip);

        if (selectedClip != null)
            pingSound.PlayOneShot(selectedClip);
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
    /// Logs debug messages if debugging is enabled.
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
        Gizmos.DrawWireSphere(_debugHitPoint, _debugDistance);
    }
#endif
}
