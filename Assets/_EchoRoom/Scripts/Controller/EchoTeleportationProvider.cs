using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace EchoRoom.Controller
{
    /// <summary>
    /// Adds explicit pending-request cancellation for safe runtime locomotion-mode switches.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EchoTeleportationProvider : TeleportationProvider
    {
        public void CancelPendingRequest()
        {
            validRequest = false;
            currentRequest = default;
        }
    }
}
