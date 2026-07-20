#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

/// <summary>
/// Editor-only. Stops Unity's XR simulator from translating the tracked headset with WASD in
/// every scene, including scenes where UnifiedLocomotionBridge is disabled. Mouse look and
/// simulated controller interactions stay untouched. This is the single owner of the
/// suppression; UnifiedLocomotionBridge only exposes the opt-out flag.
/// </summary>
public sealed class SimulatorTranslationSuppressor : MonoBehaviour
{
    private const int MaxFramesToWait = 120;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject host = new GameObject(nameof(SimulatorTranslationSuppressor));
        host.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(host);
        host.AddComponent<SimulatorTranslationSuppressor>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(SuppressSimulatorTranslationWhenReady());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        StartCoroutine(SuppressSimulatorTranslationWhenReady());
    }

    private IEnumerator SuppressSimulatorTranslationWhenReady()
    {
        for (int frame = 0; frame < MaxFramesToWait; frame++)
        {
            if (!IsSuppressionRequested())
                yield break;

            bool foundSimulator = false;

            XRInteractionSimulator interactionSimulator = FindFirstObjectByType<XRInteractionSimulator>(FindObjectsInactive.Include);
            if (interactionSimulator != null)
            {
                SetManualZero(interactionSimulator.translateXInput);
                SetManualZero(interactionSimulator.translateYInput);
                SetManualZero(interactionSimulator.translateZInput);
                foundSimulator = true;
            }

#pragma warning disable CS0618
            XRDeviceSimulator deviceSimulator = FindFirstObjectByType<XRDeviceSimulator>(FindObjectsInactive.Include);
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

    // Honours the per-scene opt-out on UnifiedLocomotionBridge, disabled component included.
    // Scenes without a bridge suppress by default.
    private static bool IsSuppressionRequested()
    {
        UnifiedLocomotionBridge bridge = FindFirstObjectByType<UnifiedLocomotionBridge>(FindObjectsInactive.Include);
        return bridge == null || bridge.SuppressSimulatorTranslation;
    }

    private static void SetManualZero(XRInputValueReader<float> inputReader)
    {
        inputReader.inputSourceMode = XRInputValueReader.InputSourceMode.ManualValue;
        inputReader.manualValue = 0f;
    }
}
#endif
