using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

#if UNITY_EDITOR
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
#endif

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
    [Tooltip("Stops Unity's XR simulator from also translating the tracked headset with WASD. Mouse look and simulated controller interactions remain enabled.")]
    [SerializeField] private bool suppressSimulatorTranslation = true;

    private bool _isConfigured;

    private void Awake()
    {
        ConfigureMoveProvider();
    }

    private void OnEnable()
    {
        ConfigureMoveProvider();

#if UNITY_EDITOR
        if (suppressSimulatorTranslation)
            StartCoroutine(SuppressSimulatorTranslationWhenReady());
#endif
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

#if UNITY_EDITOR
    private IEnumerator SuppressSimulatorTranslationWhenReady()
    {
        const int maxFramesToWait = 120;

        for (int frame = 0; frame < maxFramesToWait; frame++)
        {
            bool foundSimulator = false;

            XRInteractionSimulator interactionSimulator = FindFirstObjectByType<XRInteractionSimulator>();
            if (interactionSimulator != null)
            {
                SetManualZero(interactionSimulator.translateXInput);
                SetManualZero(interactionSimulator.translateYInput);
                SetManualZero(interactionSimulator.translateZInput);
                foundSimulator = true;
            }

#pragma warning disable CS0618
            XRDeviceSimulator deviceSimulator = FindFirstObjectByType<XRDeviceSimulator>();
            if (deviceSimulator != null)
            {
                deviceSimulator.keyboardXTranslateSpeed = 0f;
                deviceSimulator.keyboardYTranslateSpeed = 0f;
                deviceSimulator.keyboardZTranslateSpeed = 0f;
                foundSimulator = true;
            }
#pragma warning restore CS0618

            if (foundSimulator)
                yield break;

            yield return null;
        }
    }

    private static void SetManualZero(XRInputValueReader<float> inputReader)
    {
        inputReader.inputSourceMode = XRInputValueReader.InputSourceMode.ManualValue;
        inputReader.manualValue = 0f;
    }
#endif
}
