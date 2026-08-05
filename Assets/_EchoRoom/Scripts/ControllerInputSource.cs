using EchoRoom.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

/// <summary>
/// Reads movement, sprint, and ping from controller-based InputActionProperties.
/// </summary>
public class ControllerInputSource : MonoBehaviour, IPlayerInputSource
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugging = false;

    [Header("Desktop Fallback")]
    [Tooltip("Allow WASD and Left Shift to drive the same locomotion actions used by VR controllers.")]
    [SerializeField] private bool enableKeyboardMovement = true;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty leftHandMoveAction;
    [SerializeField] private InputActionProperty rightHandMoveAction;
    [SerializeField] private InputActionProperty sprintAction;
    [SerializeField] private InputActionProperty pingAction;
    [SerializeField] private InputActionProperty gripAction;

    private bool _wasFallbackPingPressed;
    private InputAction _fallbackPingAction;

    private void OnEnable()
    {
        leftHandMoveAction.action?.Enable();
        rightHandMoveAction.action?.Enable();
        sprintAction.action?.Enable();
        pingAction.action?.Enable();
        RebuildFallbackPingAction();
        EchoRoomSettings.Changed += OnSettingChanged;
    }

    private void OnDisable()
    {
        EchoRoomSettings.Changed -= OnSettingChanged;
        leftHandMoveAction.action?.Disable();
        rightHandMoveAction.action?.Disable();
        sprintAction.action?.Disable();
        pingAction.action?.Disable();
        DisposeFallbackPingAction();
        _wasFallbackPingPressed = false;
    }

    public Vector2 GetMoveInput()
    {
        Vector2 controllerMove = ReadControllerMove();
        Vector2 keyboardMove = ReadKeyboardMove();
        Vector2 chosen = keyboardMove.sqrMagnitude > controllerMove.sqrMagnitude
            ? keyboardMove
            : controllerMove;

        Log($"Move input: {chosen}");
        return chosen;
    }

    /// <summary>
    /// One stick cannot both strafe and turn. Single-hand play surrenders the horizontal axis to
    /// snap turn and keeps only forward/back, and ignores the other controller entirely so one
    /// left switched on beside the player can never drive the rig.
    /// </summary>
    private Vector2 ReadControllerMove()
    {
        Vector2 left = leftHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector2 right = rightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

        if (!HandedInput.IsSingleHand)
            return left.sqrMagnitude > right.sqrMagnitude ? left : right;

        Vector2 active = HandedInput.IsLeftActive ? left : right;
        return new Vector2(0f, active.y);
    }

    public bool GetSprintInput()
    {
        bool controllerPressed = sprintAction.action?.IsPressed() ?? false;
        bool keyboardPressed = enableKeyboardMovement &&
                               Keyboard.current != null &&
                               Keyboard.current.leftShiftKey.isPressed;
        bool pressed = controllerPressed || keyboardPressed;
        Log(pressed ? "Sprint pressed" : "");
        return pressed;
    }

    public bool GetPingInput()
    {
        bool performed = pingAction.action?.WasPerformedThisFrame() ?? false;

        // The scene's historical Ping InputActionReference points at an action that no longer
        // exists in XRI Default Input Actions. Keep accepting a valid authored replacement, but
        // always own a live binding for the current handedness so Quest/OpenXR is not dependent on
        // the legacy XR.InputDevices bridge below.
        performed |= _fallbackPingAction != null && _fallbackPingAction.WasPressedThisFrame();

        // Legacy backstop for runtimes that expose the XR feature before Input System controls
        // resolve. Edge detection prevents the held feature from emitting every frame.
        bool fallbackHeld = false;
        UnityEngine.XR.InputDevice controller = InputDevices.GetDeviceAtXRNode(HandedInput.ActiveNode);
        if (controller.isValid)
            controller.TryGetFeatureValue(HandedInput.SonarPingUsage, out fallbackHeld);

        bool fallbackPerformed = fallbackHeld && !_wasFallbackPingPressed;
        _wasFallbackPingPressed = fallbackHeld;
        performed |= fallbackPerformed;

        if (performed) Log("Ping triggered");
        return performed;
    }

    private void OnSettingChanged(EchoRoomSetting setting)
    {
        if (setting != EchoRoomSetting.Handedness)
            return;

        _wasFallbackPingPressed = false;
        RebuildFallbackPingAction();
    }

    private void RebuildFallbackPingAction()
    {
        DisposeFallbackPingAction();

        _fallbackPingAction = new InputAction("Handed Sonar Ping", InputActionType.Button);
        _fallbackPingAction.AddBinding(HandedInput.SonarPingPath);
        if (isActiveAndEnabled)
            _fallbackPingAction.Enable();
    }

    private void DisposeFallbackPingAction()
    {
        if (_fallbackPingAction == null)
            return;

        _fallbackPingAction.Disable();
        _fallbackPingAction.Dispose();
        _fallbackPingAction = null;
    }

    public bool GetGripInput()
    {
        return gripAction.action != null && gripAction.action.IsPressed();
    }

    private Vector2 ReadKeyboardMove()
    {
        if (!enableKeyboardMovement || Keyboard.current == null)
            return Vector2.zero;

        Vector2 move = Vector2.zero;
        if (Keyboard.current.aKey.isPressed) move.x -= 1f;
        if (Keyboard.current.dKey.isPressed) move.x += 1f;
        if (Keyboard.current.sKey.isPressed) move.y -= 1f;
        if (Keyboard.current.wKey.isPressed) move.y += 1f;

        return Vector2.ClampMagnitude(move, 1f);
    }

    private void Log(string message)
    {
        if (!enableDebugging || string.IsNullOrEmpty(message)) return;
        Debug.Log($"[ControllerInputSource] {message}");
    }
}
