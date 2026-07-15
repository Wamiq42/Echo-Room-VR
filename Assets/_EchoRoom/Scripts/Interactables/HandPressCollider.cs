using System;
using UnityEngine;

public class HandPressCollider : MonoBehaviour
{
    [SerializeField] private HapticHand hand = HapticHand.None;
    [SerializeField, Min(0f)] private float wallContactCooldown = 0.12f;

    private float _nextWallHapticTime;

    public HapticHand Hand => hand;

    private void Awake()
    {
        if (hand == HapticHand.None)
            hand = InferHandFromHierarchy();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.timeScale <= 0.0001f || Time.unscaledTime < _nextWallHapticTime || !IsWallCollider(other))
            return;

        EchoHaptics.PlayWallTouch(hand);
        _nextWallHapticTime = Time.unscaledTime + wallContactCooldown;
    }

    private HapticHand InferHandFromHierarchy()
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name.IndexOf("Right Hand", StringComparison.OrdinalIgnoreCase) >= 0 ||
                current.name.StartsWith("R_", StringComparison.OrdinalIgnoreCase))
            {
                return HapticHand.Right;
            }

            if (current.name.IndexOf("Left Hand", StringComparison.OrdinalIgnoreCase) >= 0 ||
                current.name.StartsWith("L_", StringComparison.OrdinalIgnoreCase))
            {
                return HapticHand.Left;
            }

            current = current.parent;
        }

        // Unknown custom hand rigs cannot identify the interacting device reliably.
        // Both hands are used only as this explicit compatibility fallback.
        return HapticHand.Both;
    }

    private bool IsWallCollider(Collider other)
    {
        if (other == null || other.isTrigger || other.transform.root == transform.root)
            return false;

        Transform current = other.transform;
        while (current != null)
        {
            if (current.name.IndexOf("wall", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            current = current.parent;
        }

        return false;
    }
}
