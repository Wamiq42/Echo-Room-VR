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
        Vector2 left = leftHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector2 right = rightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector2 controllerMove = left.sqrMagnitude > right.sqrMagnitude ? left : right;
        Vector2 keyboardMove = ReadKeyboardMove();
        Vector2 chosen = keyboardMove.sqrMagnitude > controllerMove.sqrMagnitude
            ? keyboardMove
            : controllerMove;

        Log($"Move input: {chosen}");
        return chosen;
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

        // Preserve any existing InputAction mapping. The current scene has no Ping
        // action assigned, so the right-hand secondary button is a safe fallback.
        bool fallbackHeld = false;
        UnityEngine.XR.InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightController.isValid)
            rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out fallbackHeld);

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
