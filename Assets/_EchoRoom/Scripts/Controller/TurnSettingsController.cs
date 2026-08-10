using EchoRoom.Settings;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;


namespace EchoRoom.Controller
{
    [DisallowMultipleComponent]
    public sealed class TurnSettingsController : MonoBehaviour
    {
        [SerializeField] SnapTurnProvider snapTurnProvider;
        [SerializeField] ContinuousTurnProvider smoothTurnProvider;

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
            if (snapTurnProvider == null) snapTurnProvider = GetComponent<SnapTurnProvider>();
            if (smoothTurnProvider == null)
                smoothTurnProvider = GetComponent<ContinuousTurnProvider>();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            ResolveProviders();
        }
#endif
    }
}
