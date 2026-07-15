using EchoRoom.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace EchoRoom.Controller
{
    /// <summary>
    /// Applies the persistent smooth/teleport preference without changing turn controls.
    /// Teleport aiming is available from either controller while its stick is held forward.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class TeleportLocomotionController : MonoBehaviour
    {
        [Header("Locomotion Providers")]
        [SerializeField] DynamicMoveProvider smoothMoveProvider;
        [SerializeField] DynamicSprintController sprintController;
        [SerializeField] EchoTeleportationProvider teleportationProvider;

        [Header("Teleport Interactors")]
        [SerializeField] XRRayInteractor leftTeleportInteractor;
        [SerializeField] XRRayInteractor rightTeleportInteractor;

        [Header("Input Actions")]
        [SerializeField] InputActionReference leftTeleportMode;
        [SerializeField] InputActionReference rightTeleportMode;
        [SerializeField] InputActionReference leftMove;
        [SerializeField] InputActionReference rightMove;

        bool deactivateLeftAfterInteraction;
        bool deactivateRightAfterInteraction;
        bool callbacksBound;

        public bool IsTeleportModeActive => EchoRoomSettings.LocomotionMode == LocomotionMode.Teleport;
        public bool IsLeftAimVisible => leftTeleportInteractor != null && leftTeleportInteractor.gameObject.activeSelf;
        public bool IsRightAimVisible => rightTeleportInteractor != null && rightTeleportInteractor.gameObject.activeSelf;

        void Awake()
        {
            ResolveReferences();
            HideTeleportAffordances();
        }

        void OnEnable()
        {
            EchoRoomSettings.Changed += OnSettingChanged;
            BindInputCallbacks();
            ApplySettings();
        }

        void Start()
        {
            // InputActionManager enables its assets during startup. Reapply afterward so the
            // persisted locomotion mode remains authoritative.
            ApplySettings();
        }

        void OnDisable()
        {
            EchoRoomSettings.Changed -= OnSettingChanged;
            UnbindInputCallbacks();
            HideTeleportAffordances();
            teleportationProvider?.CancelPendingRequest();
        }

        void Update()
        {
            // Selection-exit is processed by XRInteractionManager before this default Update.
            // Deferring deactivation gives TeleportationArea one frame to queue its request.
            if (deactivateLeftAfterInteraction)
            {
                SetInteractorActive(leftTeleportInteractor, false);
                deactivateLeftAfterInteraction = false;
            }

            if (deactivateRightAfterInteraction)
            {
                SetInteractorActive(rightTeleportInteractor, false);
                deactivateRightAfterInteraction = false;
            }
        }

        void OnSettingChanged(EchoRoomSetting setting)
        {
            if (setting == EchoRoomSetting.LocomotionMode)
                ApplySettings();
        }

        public void ApplySettings()
        {
            ResolveReferences();
            bool useTeleport = EchoRoomSettings.LocomotionMode == LocomotionMode.Teleport;

            if (smoothMoveProvider != null)
                smoothMoveProvider.enabled = !useTeleport;
            if (sprintController != null)
                sprintController.enabled = !useTeleport;
            if (teleportationProvider != null)
                teleportationProvider.enabled = useTeleport;

            SetActionEnabled(leftMove, !useTeleport);
            SetActionEnabled(rightMove, !useTeleport);
            SetActionEnabled(leftTeleportMode, useTeleport);
            SetActionEnabled(rightTeleportMode, useTeleport);

            if (!useTeleport)
            {
                HideTeleportAffordances();
                teleportationProvider?.CancelPendingRequest();
            }
        }

        void BindInputCallbacks()
        {
            if (callbacksBound)
                return;

            Bind(leftTeleportMode, OnLeftTeleportStarted, OnLeftTeleportCanceled);
            Bind(rightTeleportMode, OnRightTeleportStarted, OnRightTeleportCanceled);
            callbacksBound = true;
        }

        void UnbindInputCallbacks()
        {
            if (!callbacksBound)
                return;

            Unbind(leftTeleportMode, OnLeftTeleportStarted, OnLeftTeleportCanceled);
            Unbind(rightTeleportMode, OnRightTeleportStarted, OnRightTeleportCanceled);
            callbacksBound = false;
        }

        void OnLeftTeleportStarted(InputAction.CallbackContext context)
        {
            deactivateLeftAfterInteraction = false;
            if (IsTeleportModeActive)
                SetInteractorActive(leftTeleportInteractor, true);
        }

        void OnLeftTeleportCanceled(InputAction.CallbackContext context)
        {
            if (IsTeleportModeActive)
                deactivateLeftAfterInteraction = true;
        }

        void OnRightTeleportStarted(InputAction.CallbackContext context)
        {
            deactivateRightAfterInteraction = false;
            if (IsTeleportModeActive)
                SetInteractorActive(rightTeleportInteractor, true);
        }

        void OnRightTeleportCanceled(InputAction.CallbackContext context)
        {
            if (IsTeleportModeActive)
                deactivateRightAfterInteraction = true;
        }

        void HideTeleportAffordances()
        {
            deactivateLeftAfterInteraction = false;
            deactivateRightAfterInteraction = false;
            SetInteractorActive(leftTeleportInteractor, false);
            SetInteractorActive(rightTeleportInteractor, false);
        }

        static void Bind(InputActionReference reference, System.Action<InputAction.CallbackContext> performed,
            System.Action<InputAction.CallbackContext> canceled)
        {
            if (reference == null || reference.action == null)
                return;

            reference.action.performed += performed;
            reference.action.canceled += canceled;
        }

        static void Unbind(InputActionReference reference, System.Action<InputAction.CallbackContext> performed,
            System.Action<InputAction.CallbackContext> canceled)
        {
            if (reference == null || reference.action == null)
                return;

            reference.action.performed -= performed;
            reference.action.canceled -= canceled;
        }

        static void SetActionEnabled(InputActionReference reference, bool enabled)
        {
            if (reference == null || reference.action == null)
                return;

            if (enabled)
                reference.action.Enable();
            else
                reference.action.Disable();
        }

        static void SetInteractorActive(XRRayInteractor interactor, bool active)
        {
            if (interactor != null && interactor.gameObject.activeSelf != active)
                interactor.gameObject.SetActive(active);
        }

        void ResolveReferences()
        {
            if (smoothMoveProvider == null)
                smoothMoveProvider = GetComponentInParent<DynamicMoveProvider>(true);
            if (sprintController == null)
                sprintController = GetComponentInParent<DynamicSprintController>(true);
            if (teleportationProvider == null)
                teleportationProvider = GetComponent<EchoTeleportationProvider>();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }

}
