using EchoRoom.Settings;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

namespace EchoRoom.Controller
{
    /// <summary>
    /// Drives XRI's tunneling vignette from measured artificial XR Origin motion.
    /// Physical head motion does not move the origin Transform, while teleport mode and snap
    /// turning are excluded by the persisted locomotion settings.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TunnelingVignetteController), typeof(MeshRenderer))]
    public sealed class ComfortVignetteController : MonoBehaviour, ITunnelingVignetteProvider
    {
        [Header("References")]
        [SerializeField] TunnelingVignetteController tunnelingVignette;
        [SerializeField] MeshRenderer vignetteRenderer;
        [SerializeField] Transform motionRoot;

        [Header("Comfort Profile")]
        [SerializeField] VignetteParameters parameters = CreateDefaultParameters();

        [Header("Actual Motion Detection")]
        [SerializeField, Min(0f)] float movementThresholdMetersPerSecond = 0.05f;
        [SerializeField, Min(0f)] float turnThresholdDegreesPerSecond = 3f;
        [SerializeField, Min(0.01f)] float maximumContinuousDisplacementPerFrame = 1f;
        [SerializeField, Range(1f, 90f)] float maximumContinuousTurnPerFrame = 40f;
        [SerializeField, Min(0f)] float motionReleaseHoldSeconds = 0.08f;

        Vector3 previousPosition;
        Quaternion previousRotation;
        float lastMotionTime = float.NegativeInfinity;
        bool hasPreviousPose;
        bool effectRequested;
        bool actualMotionDetected;

        public VignetteParameters vignetteParameters => parameters;
        public bool IsActualMotionDetected => actualMotionDetected;
        public bool IsEffectRequested => effectRequested;
        public bool IsSettingEnabled => EchoRoomSettings.VignetteEnabled;

        void Reset()
        {
            ResolveReferences();
            parameters = CreateDefaultParameters();
        }

        void Awake()
        {
            ResolveReferences();
            CapturePose();
            ApplySetting();
        }

        void OnEnable()
        {
            EchoRoomSettings.Changed += OnSettingChanged;
            CapturePose();
            ApplySetting();
        }

        void OnDisable()
        {
            EchoRoomSettings.Changed -= OnSettingChanged;
            SetEffectRequested(false);
            actualMotionDetected = false;
        }

        void LateUpdate()
        {
            ResolveReferences();
            if (motionRoot == null || tunnelingVignette == null || vignetteRenderer == null)
                return;

            Vector3 currentPosition = motionRoot.position;
            Quaternion currentRotation = motionRoot.rotation;
            if (!hasPreviousPose)
            {
                previousPosition = currentPosition;
                previousRotation = currentRotation;
                hasPreviousPose = true;
                return;
            }

            float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            Vector3 planarDelta = Vector3.ProjectOnPlane(currentPosition - previousPosition, motionRoot.up);
            float rotationDelta = Quaternion.Angle(previousRotation, currentRotation);
            float movementSpeed = planarDelta.magnitude / deltaTime;
            float turnSpeed = rotationDelta / deltaTime;

            bool smoothMovement = EchoRoomSettings.LocomotionMode == LocomotionMode.Smooth &&
                                  planarDelta.magnitude <= maximumContinuousDisplacementPerFrame &&
                                  movementSpeed >= movementThresholdMetersPerSecond;
            bool smoothTurning = EchoRoomSettings.TurnMode == TurnMode.Smooth &&
                                 rotationDelta <= maximumContinuousTurnPerFrame &&
                                 turnSpeed >= turnThresholdDegreesPerSecond;

            actualMotionDetected = smoothMovement || smoothTurning;
            float now = Time.unscaledTime;
            if (actualMotionDetected)
                lastMotionTime = now;

            bool settingEnabled = EchoRoomSettings.VignetteEnabled;
            bool gameplayRunning = Time.timeScale > 0.001f;
            bool withinReleaseHold = now - lastMotionTime <= motionReleaseHoldSeconds;
            SetEffectRequested(settingEnabled && gameplayRunning && (actualMotionDetected || withinReleaseHold));

            if (vignetteRenderer.enabled != settingEnabled)
                vignetteRenderer.enabled = settingEnabled;

            previousPosition = currentPosition;
            previousRotation = currentRotation;
        }

        void OnSettingChanged(EchoRoomSetting setting)
        {
            if (setting == EchoRoomSetting.VignetteEnabled ||
                setting == EchoRoomSetting.LocomotionMode ||
                setting == EchoRoomSetting.TurnMode)
            {
                CapturePose();
                ApplySetting();
            }
        }

        void ApplySetting()
        {
            ResolveReferences();
            bool enabledBySetting = EchoRoomSettings.VignetteEnabled;
            if (vignetteRenderer != null)
                vignetteRenderer.enabled = enabledBySetting;

            actualMotionDetected = false;
            lastMotionTime = float.NegativeInfinity;
            if (!enabledBySetting || EchoRoomSettings.LocomotionMode == LocomotionMode.Teleport ||
                Time.timeScale <= 0.001f)
            {
                SetEffectRequested(false);
            }
        }

        void SetEffectRequested(bool requested)
        {
            if (effectRequested == requested)
                return;

            if (tunnelingVignette == null)
            {
                ResolveReferences();
                if (tunnelingVignette == null)
                    return;
            }

            effectRequested = requested;
            if (requested)
                tunnelingVignette.BeginTunnelingVignette(this);
            else
                tunnelingVignette.EndTunnelingVignette(this);
        }

        void CapturePose()
        {
            ResolveReferences();
            if (motionRoot == null)
            {
                hasPreviousPose = false;
                return;
            }

            previousPosition = motionRoot.position;
            previousRotation = motionRoot.rotation;
            hasPreviousPose = true;
        }

        void ResolveReferences()
        {
            if (tunnelingVignette == null)
                tunnelingVignette = GetComponent<TunnelingVignetteController>();
            if (vignetteRenderer == null)
                vignetteRenderer = GetComponent<MeshRenderer>();
            if (motionRoot == null)
            {
                XROrigin xrOrigin = GetComponentInParent<XROrigin>();
                if (xrOrigin != null)
                    motionRoot = xrOrigin.Origin != null ? xrOrigin.Origin.transform : xrOrigin.transform;
            }
        }

        static VignetteParameters CreateDefaultParameters()
        {
            return new VignetteParameters
            {
                apertureSize = 0.72f,
                featheringEffect = 0.22f,
                easeInTime = 0.18f,
                easeOutTime = 0.22f,
                easeInTimeLock = false,
                easeOutDelayTime = 0f,
                vignetteColor = Color.black,
                vignetteColorBlend = Color.black,
                apertureVerticalPosition = 0f,
            };
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}