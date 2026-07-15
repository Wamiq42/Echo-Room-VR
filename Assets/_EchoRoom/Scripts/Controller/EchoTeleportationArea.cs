using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace EchoRoom.Controller
{
    /// <summary>
    /// Floor-only teleport area that rejects hidden, steep, or body-blocked destinations.
    /// Direct line-of-sight prevents projectile arcs from bypassing maze walls.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EchoTeleportationArea : TeleportationArea
    {
        [SerializeField, Range(0f, 45f)] float maximumSlopeDegrees = 20f;
        [SerializeField, Min(0.1f)] float playerRadius = 0.3f;
        [SerializeField, Min(0.5f)] float playerHeight = 1.7f;
        [SerializeField] LayerMask obstructionMask = ~0;

        readonly Collider[] clearanceResults = new Collider[24];

        protected override bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit,
            ref TeleportRequest teleportRequest)
        {
            return base.GenerateTeleportRequest(interactor, raycastHit, ref teleportRequest) &&
                   IsDestinationValid(interactor, raycastHit);
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            if (!base.IsSelectableBy(interactor))
                return false;

            return interactor is XRRayInteractor rayInteractor &&
                   rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) &&
                   IsDestinationValid(rayInteractor, hit);
        }

        bool IsDestinationValid(IXRInteractor interactor, RaycastHit hit)
        {
            if (hit.collider == null || !BelongsToThisArea(hit.collider))
                return false;

            if (Vector3.Angle(Vector3.up, hit.normal) > maximumSlopeDegrees)
                return false;

            Vector3 sightOrigin = Camera.main != null
                ? Camera.main.transform.position
                : interactor.transform.position;
            Vector3 sightTarget = hit.point + Vector3.up * 0.08f;
            if (Physics.Linecast(sightOrigin, sightTarget, out RaycastHit obstruction, obstructionMask,
                    QueryTriggerInteraction.Ignore) && !BelongsToThisArea(obstruction.collider))
            {
                return false;
            }

            float radius = Mathf.Max(0.1f, playerRadius);
            float height = Mathf.Max(radius * 2f, playerHeight);
            Vector3 bottom = hit.point + Vector3.up * (radius + 0.04f);
            Vector3 top = hit.point + Vector3.up * (height - radius);
            int count = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, clearanceResults, obstructionMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider candidate = clearanceResults[i];
                if (candidate == null || BelongsToThisArea(candidate) || IsPlayerCollider(candidate))
                    continue;

                return false;
            }

            return count < clearanceResults.Length;
        }

        bool BelongsToThisArea(Collider candidate)
        {
            return candidate != null &&
                   (candidate.gameObject == gameObject || candidate.transform.IsChildOf(transform));
        }

        static bool IsPlayerCollider(Collider candidate)
        {
            return candidate.GetComponentInParent<XROrigin>() != null;
        }
    }
}
