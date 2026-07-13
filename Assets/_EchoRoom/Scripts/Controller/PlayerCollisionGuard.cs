using UnityEngine;

[DefaultExecutionOrder(10000)]
public class PlayerCollisionGuard : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Collider[] blockingColliders;
    [SerializeField] private float rollbackSkin = 0.02f;

    private Vector3 _lastValidPosition;
    private Vector3 _lastValidHeadPosition;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (headTransform == null && Camera.main != null)
            headTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        _lastValidPosition = transform.position;
        _lastValidHeadPosition = GetTrackedPosition();
        Application.onBeforeRender += GuardPosition;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= GuardPosition;
    }

    private void LateUpdate()
    {
        GuardPosition();
    }

    private void GuardPosition()
    {
        if (characterController == null || blockingColliders == null || blockingColliders.Length == 0)
            return;

        Vector3 currentTrackedPosition = GetTrackedPosition();
        Vector3 delta = currentTrackedPosition - _lastValidHeadPosition;
        delta.y = 0f;

        if (delta.sqrMagnitude > 0.000001f && HitsBlockingCollider(_lastValidHeadPosition, delta, out RaycastHit hit))
        {
            Vector3 correctedTrackedPosition = _lastValidHeadPosition + delta.normalized * Mathf.Max(0f, hit.distance - rollbackSkin);
            ApplyTrackedPositionCorrection(correctedTrackedPosition, currentTrackedPosition);
            return;
        }

        if (OverlapsBlockingCollider(currentTrackedPosition))
        {
            ApplyTrackedPositionCorrection(_lastValidHeadPosition, currentTrackedPosition);
            return;
        }

        _lastValidPosition = transform.position;
        _lastValidHeadPosition = currentTrackedPosition;
    }

    private Vector3 GetTrackedPosition()
    {
        Transform trackedTransform = headTransform != null ? headTransform : transform;
        Vector3 trackedPosition = trackedTransform.position;
        trackedPosition.y = transform.position.y + characterController.center.y;
        return trackedPosition;
    }

    private void ApplyTrackedPositionCorrection(Vector3 correctedTrackedPosition, Vector3 currentTrackedPosition)
    {
        Vector3 correction = correctedTrackedPosition - currentTrackedPosition;
        correction.y = 0f;
        transform.position += correction;
        _lastValidPosition = transform.position;
        _lastValidHeadPosition = GetTrackedPosition();
    }

    private bool HitsBlockingCollider(Vector3 originPosition, Vector3 delta, out RaycastHit nearestHit)
    {
        GetCapsule(originPosition, out Vector3 bottom, out Vector3 top, out float radius);
        float distance = delta.magnitude;
        RaycastHit[] hits = Physics.CapsuleCastAll(
            bottom,
            top,
            radius,
            delta.normalized,
            distance + rollbackSkin,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        nearestHit = default;
        float nearestDistance = float.PositiveInfinity;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (!IsBlockingCollider(hit.collider) || hit.distance >= nearestDistance)
                continue;

            nearestHit = hit;
            nearestDistance = hit.distance;
            found = true;
        }

        return found;
    }

    private bool OverlapsBlockingCollider(Vector3 position)
    {
        GetCapsule(position, out Vector3 bottom, out Vector3 top, out float radius);
        Collider[] overlaps = Physics.OverlapCapsule(
            bottom,
            top,
            radius,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        foreach (Collider overlap in overlaps)
        {
            if (IsBlockingCollider(overlap))
                return true;
        }

        return false;
    }

    private void GetCapsule(Vector3 rootPosition, out Vector3 bottom, out Vector3 top, out float radius)
    {
        radius = characterController.radius;
        float height = Mathf.Max(characterController.height, radius * 2f);
        Vector3 center = rootPosition + characterController.center;
        float halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);
        bottom = center + Vector3.down * halfSegment;
        top = center + Vector3.up * halfSegment;
    }

    private bool IsBlockingCollider(Collider candidate)
    {
        if (candidate == null || candidate.isTrigger)
            return false;

        foreach (Collider blockingCollider in blockingColliders)
        {
            if (candidate == blockingCollider)
                return true;
        }

        return false;
    }
}
