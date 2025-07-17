using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets; // ✅ <--- add this

public class DynamicSprintController : MonoBehaviour
{
    [SerializeField] private bool enableDebugging = false;  // ✅ debug toggle on top

    [Header("References")]
    [SerializeField] private DynamicMoveProvider moveProvider;
    [SerializeField] private PlayerInputManager inputManager;


    [Header("Settings")]
    [SerializeField] private float normalSpeed = 1.5f;
    [SerializeField] private float sprintSpeed = 3f;
    

    private void Update()
    {
        HandleSprint();
    }

    /// <summary>
    /// Checks the sprint input and applies the appropriate speed to the move provider.
    /// </summary>
    private void HandleSprint()
    {
        if (moveProvider == null || inputManager == null)
            return;

        bool isSprinting = inputManager != null && inputManager.ReadSprint();
        moveProvider.moveSpeed = isSprinting ? sprintSpeed : normalSpeed;

        Log(isSprinting ? "Sprinting" : "Walking");
    }

    /// <summary>
    /// Logs debug messages if debugging is enabled.
    /// </summary>
    private void Log(string message)
    {
        if (!enableDebugging) return;
        Debug.Log($"[DynamicSprintController] {message}");
    }
}