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

    private void OnEnable()
    {
        leftHandMoveAction.action?.Enable();
        rightHandMoveAction.action?.Enable();
        sprintAction.action?.Enable();
        pingAction.action?.Enable();
    }

    private void OnDisable()
    {
        leftHandMoveAction.action?.Disable();
        rightHandMoveAction.action?.Disable();
        sprintAction.action?.Disable();
        pingAction.action?.Disable();
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

        // Preserve any existing InputAction mapping. The current scene has no Ping action
        // assigned, so the active hand's sonar button is the fallback: B on the right in
        // two-handed play, A/X on the one controller in single-hand play.
        bool fallbackHeld = false;
        UnityEngine.XR.InputDevice controller = InputDevices.GetDeviceAtXRNode(HandedInput.ActiveNode);
        if (controller.isValid)
            controller.TryGetFeatureValue(HandedInput.SonarPingUsage, out fallbackHeld);

        bool fallbackPerformed = pingAction.action == null && fallbackHeld && !_wasFallbackPingPressed;
        _wasFallbackPingPressed = fallbackHeld;
        performed |= fallbackPerformed;

        if (performed) Log("Ping triggered");
        return performed;
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
