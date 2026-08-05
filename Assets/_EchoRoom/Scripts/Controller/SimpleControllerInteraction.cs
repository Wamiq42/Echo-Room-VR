using EchoRoom.Settings;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Provides a lightweight context interaction without grabbing or a grip system.
/// Existing InputAction bindings remain untouched; the active controller's trigger is
/// read directly and keyboard E remains handled by the interactables themselves.
/// </summary>
[DisallowMultipleComponent]
public sealed class SimpleControllerInteraction : MonoBehaviour
{
    [SerializeField, Min(0.5f)] private float interactionRange = 2.25f;
    [SerializeField, Range(-1f, 1f)] private float minimumAimDot = 0.35f;

    private bool _wasTriggerPressed;
    private Camera _camera;

    private void Update()
    {
        bool triggerPressed = ReadActiveTrigger();
        if (triggerPressed && !_wasTriggerPressed)
            TryInteract();

        _wasTriggerPressed = triggerPressed;
    }

    public bool TryInteract()
    {
        if (_camera == null)
            _camera = Camera.main;
        if (_camera == null)
            return false;

        HapticHand hand = HandedInput.ActiveHapticHand;

        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit, interactionRange))
        {
            EchoButtonInteractable hitButton = hit.collider.GetComponentInParent<EchoButtonInteractable>();
            if (hitButton != null && !hitButton.IsOn)
            {
                hitButton.TurnOn(hand);
                return true;
            }

            LeverInteractable hitLever = hit.collider.GetComponentInParent<LeverInteractable>();
            if (hitLever != null)
            {
                hitLever.Toggle(hand);
                return true;
            }
        }

        Component bestTarget = FindBestNearbyTarget();
        if (bestTarget is EchoButtonInteractable button && !button.IsOn)
        {
            button.TurnOn(hand);
            return true;
        }

        if (bestTarget is LeverInteractable lever)
        {
            lever.Toggle(hand);
            return true;
        }

        return false;
    }

    private Component FindBestNearbyTarget()
    {
        Vector3 origin = _camera.transform.position;
        Vector3 forward = _camera.transform.forward;
        Component best = null;
        float bestScore = float.MaxValue;

        EchoButtonInteractable[] buttons = FindObjectsOfType<EchoButtonInteractable>(false);
        for (int i = 0; i < buttons.Length; i++)
            Consider(buttons[i], origin, forward, ref best, ref bestScore);

        LeverInteractable[] levers = FindObjectsOfType<LeverInteractable>(false);
        for (int i = 0; i < levers.Length; i++)
            Consider(levers[i], origin, forward, ref best, ref bestScore);

        return best;
    }

    private void Consider(Component candidate, Vector3 origin, Vector3 forward, ref Component best, ref float bestScore)
    {
        if (candidate == null)
            return;

        Vector3 offset = candidate.transform.position - origin;
        float distance = offset.magnitude;
        if (distance > interactionRange || distance < 0.001f)
            return;

        float aimDot = Vector3.Dot(forward, offset / distance);
        if (aimDot < minimumAimDot)
            return;

        float score = distance + (1f - aimDot) * 1.5f;
        if (score >= bestScore)
            return;

        best = candidate;
        bestScore = score;
    }

    private static bool ReadActiveTrigger()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(HandedInput.ActiveNode);
        return device.isValid &&
               device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed) &&
               pressed;
    }
}
