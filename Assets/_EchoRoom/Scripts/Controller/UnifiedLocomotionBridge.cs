using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Feeds desktop and VR movement through one DynamicMoveProvider so the XR Origin,
/// CharacterController, camera, and controller objects move as one collision-safe rig.
/// </summary>
[DefaultExecutionOrder(-300)]
public sealed class UnifiedLocomotionBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private DynamicMoveProvider moveProvider;

    [Header("Editor Simulation")]
    [Tooltip("Stops Unity's XR simulator from also translating the tracked headset with WASD. Mouse look and simulated controller interactions remain enabled. Applied by SimulatorTranslationSuppressor, which also runs when this component is disabled.")]
    [SerializeField] private bool suppressSimulatorTranslation = true;

    private bool _isConfigured;

    // Read by the editor-only SimulatorTranslationSuppressor, which owns the actual suppression.
    public bool SuppressSimulatorTranslation => suppressSimulatorTranslation;

    private void Awake()
    {
        ConfigureMoveProvider();
    }

    private void OnEnable()
    {
        ConfigureMoveProvider();
    }

    private void Update()
    {
        if (!_isConfigured)
            return;

        moveProvider.leftHandMoveInput.manualValue = inputManager.ReadMove();
        moveProvider.rightHandMoveInput.manualValue = Vector2.zero;
    }

    private void OnDisable()
    {
        if (moveProvider == null)
            return;

        moveProvider.leftHandMoveInput.manualValue = Vector2.zero;
        moveProvider.rightHandMoveInput.manualValue = Vector2.zero;
    }

    private void ConfigureMoveProvider()
    {
        if (_isConfigured || inputManager == null || moveProvider == null)
            return;

        moveProvider.leftHandMoveInput.inputSourceMode = XRInputValueReader.InputSourceMode.ManualValue;
        moveProvider.rightHandMoveInput.inputSourceMode = XRInputValueReader.InputSourceMode.ManualValue;
        moveProvider.leftHandMoveInput.manualValue = Vector2.zero;
        moveProvider.rightHandMoveInput.manualValue = Vector2.zero;
        _isConfigured = true;
    }
}
