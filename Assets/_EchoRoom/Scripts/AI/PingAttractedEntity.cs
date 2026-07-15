using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PingAttractedEntity : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PingEmitter pingEmitter;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform resetPoint;
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private AudioSource proximityAudio;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float patrolWaitTime = 1f;
    [SerializeField] private float waypointReachDistance = 0.35f;

    [Header("Ping Danger")]
    [SerializeField] private float captureRange = 2f;
    [SerializeField] private float pingDangerTimer = 8f;
    [SerializeField] private bool resetOnCapture = true;
    [SerializeField] private bool resetToInitialPlayerSpawn = true;
    [SerializeField] private bool respawnPlayerWithGameManager = true;
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Proximity Sound")]
    [SerializeField] private float maxAudioDistance = 12f;
    [SerializeField] private float minVolume = 0f;
    [SerializeField] private float maxVolume = 1f;

    [Header("Proximity Haptics")]
    [SerializeField, Min(0.1f)] private float maxHapticDistance = 8f;
    [SerializeField, Range(0f, 1f)] private float minHapticAmplitude = 0.08f;
    [SerializeField, Range(0f, 1f)] private float maxHapticAmplitude = 0.55f;
    [SerializeField, Min(0.01f)] private float hapticDuration = 0.08f;
    [SerializeField, Min(0.02f)] private float farHapticInterval = 0.45f;
    [SerializeField, Min(0.02f)] private float nearHapticInterval = 0.12f;

    [Header("Reactive Smoke")]
    [SerializeField] private Transform reactiveVisual;
    [SerializeField] private ParticleSystem reactiveSmoke;
    [SerializeField] private ParticleSystemRenderer reactiveSmokeRenderer;
    [SerializeField] private float nearReactionDistance = 2.5f;
    [SerializeField] private float farReactionDistance = 10f;
    [SerializeField] private float farVelocityMultiplier = 0.75f;
    [SerializeField] private float nearVelocityMultiplier = 1.65f;
    [SerializeField] private float proximityNoiseMultiplier = 1.35f;

    [Header("Failed Search Outburst")]
    [SerializeField] private float failedSearchDuration = 0.9f;
    [SerializeField] private float anticipationDuration = 0.15f;
    [SerializeField] private float burstRiseDuration = 0.12f;
    [SerializeField] private float recoveryDuration = 0.65f;
    [SerializeField] private float outburstScaleMultiplier = 1.4f;
    [SerializeField] private float outburstVelocityMultiplier = 3f;
    [SerializeField] private float outburstNoiseMultiplier = 1.8f;
    [SerializeField] private float outburstEyeGlowMultiplier = 2.2f;
    [SerializeField] private float postOutburstWait = 0.75f;
    [SerializeField] private AudioClip outburstClip;
    [SerializeField, Range(0f, 1f)] private float outburstVolume = 0.8f;

    private NavMeshAgent _agent;
    private int _patrolIndex;
    private float _waitUntil;
    private float _dangerUntil;
    private bool _hasPingTarget;
    private Vector3 _pingTarget;
    private Vector3 _fallbackSpawnPosition;
    private Quaternion _fallbackSpawnRotation;
    private bool _hasInitialPlayerSpawn;
    private PingEmitter _subscribedPingEmitter;
    private Coroutine _outburstRoutine;
    private int _pingSequence;
    private bool _isOutbursting;
    private float _outburstIntensity;
    private float _outburstScale = 1f;
    private Vector3 _reactiveBaseScale;
    private float _baseVelocityMultiplier = 1f;
    private float _baseNoiseStrengthMultiplier = 1f;
    private float _baseEyeGlowStrength = 0.62f;
    private bool _reactiveDefaultsCached;
    private MaterialPropertyBlock _smokePropertyBlock;
    private float _nextProximityHapticTime;
    private bool _proximityHapticsActive;

    private bool IsDangerous => Time.time < _dangerUntil;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        ResolveReferences();
        ResolveReactiveReferences();
        CacheReactiveDefaults();

        _agent.speed = moveSpeed;

        if (playerRoot != null)
        {
            _fallbackSpawnPosition = playerRoot.position;
            _fallbackSpawnRotation = playerRoot.rotation;
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        ResolveReferences();
        CacheInitialPlayerSpawn();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToPingEmitter();
    }

    private void OnDisable()
    {
        UnsubscribeFromPingEmitter();
        CancelOutburst(false);
        StopProximityHaptics();
    }

    private void OnDestroy()
    {
        StopProximityHaptics();
    }

    private void Update()
    {
        ResolveReferences();
        ResolveReactiveReferences();
        SubscribeToPingEmitter();

        if (_agent != null)
            _agent.speed = moveSpeed;

        UpdateProximityAudio();
        UpdateProximityHaptics();
        UpdateReactiveVisuals();

        if (_isOutbursting)
            return;

        if (_hasPingTarget && !IsDangerous)
        {
            _hasPingTarget = false;
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.ResetPath();
            Log("Ping search timer ended. Returning to patrol.");
        }

        if (_hasPingTarget && IsDangerous)
            MoveToPingTarget();
        else
            Patrol();
    }

    private void HandlePing(Vector3 pingOrigin)
    {
        _pingSequence++;
        CancelOutburst(true);

        _pingTarget = pingOrigin;
        _hasPingTarget = true;
        _dangerUntil = Time.time + pingDangerTimer;

        if (_agent != null && _agent.enabled)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_pingTarget);
        }

        Log("Heard ping and created search radius.");
    }

    private void MoveToPingTarget()
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;

        if (!_agent.hasPath)
            _agent.SetDestination(_pingTarget);

        float distanceToPing = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_pingTarget.x, _pingTarget.z));
        float arrivalDistance = Mathf.Max(waypointReachDistance, captureRange);

        if (!_agent.pathPending && distanceToPing <= arrivalDistance)
        {
            if (IsPlayerInCaptureRange())
            {
                CapturePlayer();
            }
            else
            {
                BeginFailedSearchOutburst();
            }
        }
    }

    private void BeginFailedSearchOutburst()
    {
        if (_isOutbursting)
            return;

        _hasPingTarget = false;
        _isOutbursting = true;

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        _outburstRoutine = StartCoroutine(FailedSearchOutburst(_pingSequence));
        Log("Reached ping radius. Searching for the player.");
    }

    private IEnumerator FailedSearchOutburst(int pingSequence)
    {
        float elapsed = 0f;

        while (elapsed < Mathf.Max(0f, failedSearchDuration))
        {
            if (pingSequence != _pingSequence)
                yield break;

            if (IsPlayerInCaptureRange())
            {
                FinishOutburstState();
                CapturePlayer();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        float startScale = _outburstScale;
        elapsed = 0f;

        while (elapsed < Mathf.Max(0.01f, anticipationDuration))
        {
            if (pingSequence != _pingSequence)
                yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, anticipationDuration));
            _outburstScale = Mathf.Lerp(startScale, 0.86f, t);
            yield return null;
        }

        if (outburstClip != null && proximityAudio != null)
            proximityAudio.PlayOneShot(outburstClip, outburstVolume);

        elapsed = 0f;
        while (elapsed < Mathf.Max(0.01f, burstRiseDuration))
        {
            if (pingSequence != _pingSequence)
                yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, burstRiseDuration));
            t = t * t * (3f - 2f * t);
            _outburstScale = Mathf.Lerp(0.86f, outburstScaleMultiplier, t);
            _outburstIntensity = t;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < Mathf.Max(0.01f, recoveryDuration))
        {
            if (pingSequence != _pingSequence)
                yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, recoveryDuration));
            t = t * t * (3f - 2f * t);
            _outburstScale = Mathf.Lerp(outburstScaleMultiplier, 1f, t);
            _outburstIntensity = 1f - t;
            yield return null;
        }

        FinishOutburstState();
        _waitUntil = Time.time + postOutburstWait;
        Log("Player not found. Angry smoke outburst completed.");
    }

    private void FinishOutburstState()
    {
        _outburstRoutine = null;
        _isOutbursting = false;
        _outburstIntensity = 0f;
        _outburstScale = 1f;

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            _agent.isStopped = false;

        UpdateReactiveVisuals();
    }

    private void CancelOutburst(bool resumeAgent)
    {
        if (_outburstRoutine != null)
            StopCoroutine(_outburstRoutine);

        _outburstRoutine = null;
        _isOutbursting = false;
        _outburstIntensity = 0f;
        _outburstScale = 1f;

        if (resumeAgent && _agent != null && _agent.enabled && _agent.isOnNavMesh)
            _agent.isStopped = false;

        UpdateReactiveVisuals();
    }

    private void ResolveReactiveReferences()
    {
        if (reactiveVisual == null)
            reactiveVisual = transform.Find("Vertical Circular Glow Smoke");

        if (reactiveSmoke == null && reactiveVisual != null)
        {
            Transform smokeTransform = reactiveVisual.Find("Luminous Smoke");
            if (smokeTransform != null)
                reactiveSmoke = smokeTransform.GetComponent<ParticleSystem>();
        }

        if (reactiveSmokeRenderer == null && reactiveSmoke != null)
            reactiveSmokeRenderer = reactiveSmoke.GetComponent<ParticleSystemRenderer>();
    }

    private void CacheReactiveDefaults()
    {
        if (_reactiveDefaultsCached || reactiveVisual == null || reactiveSmoke == null)
            return;

        _reactiveBaseScale = reactiveVisual.localScale;
        _baseVelocityMultiplier = reactiveSmoke.velocityOverLifetime.speedModifierMultiplier;
        _baseNoiseStrengthMultiplier = reactiveSmoke.noise.strengthMultiplier;

        if (reactiveSmokeRenderer != null && reactiveSmokeRenderer.sharedMaterial != null &&
            reactiveSmokeRenderer.sharedMaterial.HasProperty("_EyeGlowStrength"))
        {
            _baseEyeGlowStrength = reactiveSmokeRenderer.sharedMaterial.GetFloat("_EyeGlowStrength");
        }

        _smokePropertyBlock = new MaterialPropertyBlock();
        _reactiveDefaultsCached = true;
    }

    private void UpdateReactiveVisuals()
    {
        ResolveReactiveReferences();
        CacheReactiveDefaults();

        if (!_reactiveDefaultsCached)
            return;

        float proximity = 0f;
        if (playerRoot != null)
        {
            float distance = Vector3.Distance(transform.position, playerRoot.position);
            proximity = 1f - Mathf.InverseLerp(nearReactionDistance, Mathf.Max(nearReactionDistance + 0.01f, farReactionDistance), distance);
            proximity = proximity * proximity * (3f - 2f * proximity);
        }

        float distanceVelocity = Mathf.Lerp(farVelocityMultiplier, nearVelocityMultiplier, proximity);
        var velocity = reactiveSmoke.velocityOverLifetime;
        velocity.speedModifierMultiplier = _baseVelocityMultiplier * distanceVelocity *
                                           Mathf.Lerp(1f, outburstVelocityMultiplier, _outburstIntensity);

        var noise = reactiveSmoke.noise;
        noise.strengthMultiplier = _baseNoiseStrengthMultiplier *
                                   Mathf.Lerp(1f, proximityNoiseMultiplier, proximity) *
                                   Mathf.Lerp(1f, outburstNoiseMultiplier, _outburstIntensity);

        reactiveVisual.localScale = _reactiveBaseScale * _outburstScale;

        if (reactiveSmokeRenderer != null && reactiveSmokeRenderer.sharedMaterial != null &&
            reactiveSmokeRenderer.sharedMaterial.HasProperty("_EyeGlowStrength"))
        {
            if (_smokePropertyBlock == null)
                _smokePropertyBlock = new MaterialPropertyBlock();

            reactiveSmokeRenderer.GetPropertyBlock(_smokePropertyBlock);
            float glow = _baseEyeGlowStrength * Mathf.Lerp(1f, 1.35f, proximity) *
                         Mathf.Lerp(1f, outburstEyeGlowMultiplier, _outburstIntensity);
            _smokePropertyBlock.SetFloat("_EyeGlowStrength", glow);
            reactiveSmokeRenderer.SetPropertyBlock(_smokePropertyBlock);
        }
    }

    private void Patrol()
    {
        if (_agent == null || !_agent.enabled) return;
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;

        if (Time.time < _waitUntil) return;

        Transform target = patrolWaypoints[_patrolIndex];
        if (target == null)
        {
            AdvancePatrol();
            return;
        }

        if (!_agent.hasPath)
            _agent.SetDestination(target.position);

        if (!_agent.pathPending && _agent.remainingDistance <= waypointReachDistance)
        {
            AdvancePatrol();
            _waitUntil = Time.time + patrolWaitTime;
        }
    }

    private void AdvancePatrol()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;
        _patrolIndex = (_patrolIndex + 1) % patrolWaypoints.Length;

        Transform target = patrolWaypoints[_patrolIndex];
        if (target != null && _agent != null && _agent.enabled)
            _agent.SetDestination(target.position);
    }

    private bool IsPlayerInCaptureRange()
    {
        if (playerRoot == null && Camera.main == null) return false;

        Vector3 point = playerRoot != null ? playerRoot.position : Camera.main.transform.position;

        if (playerRoot != null)
        {
            CharacterController controller = playerRoot.GetComponentInChildren<CharacterController>();
            if (controller != null)
                point = controller.bounds.center;
        }

        if (Camera.main != null)
        {
            Vector3 headPoint = Camera.main.transform.position;
            float headToPing = Vector2.Distance(new Vector2(_pingTarget.x, _pingTarget.z), new Vector2(headPoint.x, headPoint.z));
            float bodyToPing = Vector2.Distance(new Vector2(_pingTarget.x, _pingTarget.z), new Vector2(point.x, point.z));
            if (headToPing < bodyToPing)
                point = headPoint;
        }

        float distanceFromPing = Vector2.Distance(new Vector2(_pingTarget.x, _pingTarget.z), new Vector2(point.x, point.z));
        return distanceFromPing <= captureRange;
    }

    private void CapturePlayer()
    {
        StopProximityHaptics();
        CancelOutburst(true);
        _dangerUntil = 0f;
        _hasPingTarget = false;

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            _agent.ResetPath();

        Log("Player captured.");

        if (EchoRoom.UI.VRPauseMenu.TryShowCapturedMenu())
        {
            Log("Player captured; showing captured menu.");
            return;
        }

        if (!resetOnCapture) return;

        if (respawnPlayerWithGameManager && GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
            return;
        }

        ResetPlayerToSpawn();
    }

    private void ResetPlayerToSpawn()
    {
        if (playerRoot == null) return;

        Vector3 targetPosition = resetPoint != null ? resetPoint.position : _fallbackSpawnPosition;
        Quaternion targetRotation = resetPoint != null ? resetPoint.rotation : _fallbackSpawnRotation;

        CharacterController controller = playerRoot.GetComponentInChildren<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        playerRoot.SetPositionAndRotation(targetPosition, targetRotation);

        if (controller != null)
            controller.enabled = true;
    }

    private void UpdateProximityAudio()
    {
        if (proximityAudio == null || playerRoot == null) return;

        float distance = Vector3.Distance(transform.position, playerRoot.position);
        float t = 1f - Mathf.Clamp01(distance / Mathf.Max(0.01f, maxAudioDistance));
        proximityAudio.volume = Mathf.Lerp(minVolume, maxVolume, t);

        if (proximityAudio.clip != null && !proximityAudio.isPlaying)
            proximityAudio.Play();
    }

    private void UpdateProximityHaptics()
    {
        if (playerRoot == null || Time.timeScale <= 0.0001f ||
            (EchoRoom.UI.VRPauseMenu.Instance != null && EchoRoom.UI.VRPauseMenu.Instance.IsOpen))
        {
            StopProximityHaptics();
            return;
        }

        float distance = Vector3.Distance(transform.position, playerRoot.position);
        float distanceLimit = Mathf.Max(0.1f, maxHapticDistance);
        if (distance >= distanceLimit)
        {
            StopProximityHaptics();
            return;
        }

        if (Time.unscaledTime < _nextProximityHapticTime)
            return;

        float closeness = 1f - Mathf.Clamp01(distance / distanceLimit);
        closeness = Mathf.SmoothStep(0f, 1f, closeness);
        float amplitude = Mathf.Lerp(minHapticAmplitude, maxHapticAmplitude, closeness);
        float interval = Mathf.Lerp(farHapticInterval, nearHapticInterval, closeness);

        // Entity proximity is body-level danger feedback, so it is deliberately
        // bilateral rather than attributed to an interacting hand.
        EchoHaptics.Request(HapticHand.Both, amplitude, hapticDuration, HapticReason.EntityProximity);
        _proximityHapticsActive = true;
        _nextProximityHapticTime = Time.unscaledTime + Mathf.Max(0.02f, interval);
    }

    private void StopProximityHaptics()
    {
        if (!_proximityHapticsActive)
            return;

        EchoHaptics.Stop(HapticHand.Both);
        _proximityHapticsActive = false;
        _nextProximityHapticTime = 0f;
    }

    private void ResolveReferences()
    {
        if (pingEmitter == null)
            pingEmitter = FindObjectOfType<PingEmitter>();

        if (playerRoot == null)
        {
            GameObject rig = GameObject.Find("XR Origin (XR Rig)");
            if (rig != null)
                playerRoot = rig.transform;
        }

        if (playerRoot == null && Camera.main != null)
            playerRoot = Camera.main.transform.root;

        if (proximityAudio == null)
            proximityAudio = GetComponent<AudioSource>();
    }

    private void SubscribeToPingEmitter()
    {
        if (_subscribedPingEmitter == pingEmitter)
            return;

        UnsubscribeFromPingEmitter();

        if (pingEmitter == null)
            return;

        pingEmitter.OnPingEmitted += HandlePing;
        _subscribedPingEmitter = pingEmitter;
    }

    private void UnsubscribeFromPingEmitter()
    {
        if (_subscribedPingEmitter != null)
            _subscribedPingEmitter.OnPingEmitted -= HandlePing;

        _subscribedPingEmitter = null;
    }

    private void CacheInitialPlayerSpawn()
    {
        if (playerRoot == null) return;

        _fallbackSpawnPosition = playerRoot.position;
        _fallbackSpawnRotation = playerRoot.rotation;
        _hasInitialPlayerSpawn = true;
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log("[PingAttractedEntity] " + message, this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxAudioDistance);
    }
}
