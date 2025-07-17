using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads movement, sprint, and ping from controller-based InputActionProperties.
/// </summary>
public class ControllerInputSource : MonoBehaviour, IPlayerInputSource
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugging = false;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty leftHandMoveAction;
    [SerializeField] private InputActionProperty rightHandMoveAction;
    [SerializeField] private InputActionProperty sprintAction;
    [SerializeField] private InputActionProperty pingAction;

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
        // You can blend or choose stronger
        Vector2 chosen = (left.sqrMagnitude > right.sqrMagnitude) ? left : right;

        Log($"Move input: {chosen}");
        return chosen;
    }

    public bool GetSprintInput()
    {
        bool pressed = sprintAction.action?.IsPressed() ?? false;
        Log(pressed ? "Sprint pressed" : "");
        return pressed;
    }

    public bool GetPingInput()
    {
        bool performed = pingAction.action?.WasPerformedThisFrame() ?? false;
        if (performed) Log("Ping triggered");
        return performed;
    }

    private void Log(string message)
    {
        if (!enableDebugging || string.IsNullOrEmpty(message)) return;
        Debug.Log($"[ControllerInputSource] {message}");
    }
}