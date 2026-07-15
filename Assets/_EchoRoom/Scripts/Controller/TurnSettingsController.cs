using EchoRoom.Settings;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

#pragma warning disable CS0618 // The existing rig still serializes XRI's legacy action-based providers.

namespace EchoRoom.Controller
{
    [DisallowMultipleComponent]
    public sealed class TurnSettingsController : MonoBehaviour
    {
        [SerializeField] ActionBasedSnapTurnProvider snapTurnProvider;
        [SerializeField] ActionBasedContinuousTurnProvider smoothTurnProvider;

        void Reset() => ResolveProviders();

        void Awake()
        {
            ResolveProviders();
            ApplySettings();
        }

        void OnEnable()
        {
            EchoRoomSettings.Changed += OnSettingChanged;
            ApplySettings();
        }

        void OnDisable()
        {
            EchoRoomSettings.Changed -= OnSettingChanged;
        }

        void OnSettingChanged(EchoRoomSetting setting)
        {
            if (setting == EchoRoomSetting.TurnMode || setting == EchoRoomSetting.TurnSpeed)
                ApplySettings();
        }

        public void ApplySettings()
        {
            ResolveProviders();
            if (snapTurnProvider == null || smoothTurnProvider == null)
            {
                Debug.LogError("[TurnSettingsController] Both turn providers must be assigned.", this);
                return;
            }

            smoothTurnProvider.turnSpeed = EchoRoomSettings.TurnSpeed;
            bool useSnap = EchoRoomSettings.TurnMode == TurnMode.Snap;
            snapTurnProvider.enabled = useSnap;
            smoothTurnProvider.enabled = !useSnap;
        }

        void ResolveProviders()
        {
            if (snapTurnProvider == null) snapTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();
            if (smoothTurnProvider == null)
                smoothTurnProvider = GetComponent<ActionBasedContinuousTurnProvider>();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            ResolveProviders();
        }
#endif
    }
}

#pragma warning restore CS0618
