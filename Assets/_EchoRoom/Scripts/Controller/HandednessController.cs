using EchoRoom.Settings;
using EchoRoom.UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;


namespace EchoRoom.Controller
{
    /// <summary>
    /// Applies the Handedness preference to the things that live on a specific controller rather
    /// than in a binding string: the wrist panels, the menu pointer, the hand-mounted ping rig,
    /// and stick-down turn-around. Input bindings resolve themselves through HandedInput, so this
    /// component deliberately owns nothing that a control path could express.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class HandednessController : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] Transform leftController;
        [SerializeField] Transform rightController;

        [Header("Hand Mounted")]
        [Tooltip("Objects authored under one controller that should follow the hand in play -- the " +
                 "ping emitter and its sonar pulse. Reparented at runtime, restored in Both mode.")]
        [SerializeField] Transform[] handMountedObjects;

        [Header("Turn")]
        [Tooltip("Snap turn provider. Stick-down turn-around is switched off in single-hand play, " +
                 "where the same axis is reverse movement.")]
        [SerializeField] SnapTurnProvider snapTurnProvider;

        Transform[] authoredParents;
        bool authoredTurnAround;
        bool capturedAuthoredState;

        public static HandednessController Instance { get; private set; }

        public Transform ActiveHandTransform =>
            HandedInput.IsLeftActive ? leftController : rightController;

        /// <summary>The controller out of play, or null while both hands are in use.</summary>
        public Transform IdleHandTransform
        {
            get
            {
                if (!HandedInput.IsSingleHand) return null;
                return HandedInput.IsLeftActive ? rightController : leftController;
            }
        }

        void Awake()
        {
            if (Instance == null || Instance == this) Instance = this;
            ResolveSnapTurnProvider();
            CaptureAuthoredState();
        }

        void OnEnable()
        {
            EchoRoomSettings.Changed += OnSettingChanged;
        }

        void Start()
        {
            // Deliberately Start, not Awake: VRPauseMenu and MazeLevelTimer publish themselves in
            // their own Awake, and Awake ordering between unrelated components is not defined.
            Apply();
        }

        void OnDisable()
        {
            EchoRoomSettings.Changed -= OnSettingChanged;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnSettingChanged(EchoRoomSetting setting)
        {
            if (setting == EchoRoomSetting.Handedness) Apply();
        }

        public void Apply()
        {
            CaptureAuthoredState();

            Transform activeHand = ActiveHandTransform;
            Transform idleHand = IdleHandTransform;

            ApplyWristPanels(activeHand, idleHand);
            ApplyHandMountedObjects(activeHand);
            ApplyTurnAround();
        }

        void ApplyWristPanels(Transform activeHand, Transform idleHand)
        {
            // Instance is only populated once the menu's Awake has run, which a menu authored
            // inactive never does -- fall back to the search the menu's own statics use.
            VRPauseMenu menu = VRPauseMenu.Instance != null
                ? VRPauseMenu.Instance
                : FindFirstObjectByType<VRPauseMenu>(FindObjectsInactive.Include);
            if (menu != null)
            {
                menu.SetMenuHandAnchor(activeHand);
                menu.SetIdleHandRoot(idleHand);
            }

            MazeLevelTimer timer = FindFirstObjectByType<MazeLevelTimer>(FindObjectsInactive.Include);
            if (timer != null) timer.SetHandAnchor(activeHand);
        }

        /// <summary>
        /// The ping originates wherever its emitter is parented, so a left-handed player would
        /// otherwise shout out of the controller sitting on the table. Reparenting keeps the pulse
        /// and its forward echo cast attached to the hand the player is actually pointing.
        /// </summary>
        void ApplyHandMountedObjects(Transform activeHand)
        {
            if (handMountedObjects == null || authoredParents == null) return;

            for (int i = 0; i < handMountedObjects.Length; i++)
            {
                Transform mounted = handMountedObjects[i];
                if (mounted == null) continue;

                Transform target = HandedInput.IsSingleHand ? activeHand : authoredParents[i];
                if (target == null || mounted.parent == target) continue;

                // World pose is irrelevant here: these are zero-offset children of a controller.
                mounted.SetParent(target, false);
            }
        }

        void ApplyTurnAround()
        {
            if (snapTurnProvider == null) return;
            // One stick cannot mean both "walk backwards" and "spin 180 degrees".
            snapTurnProvider.enableTurnAround = authoredTurnAround && !HandedInput.IsSingleHand;
        }

        void CaptureAuthoredState()
        {
            if (capturedAuthoredState) return;
            ResolveSnapTurnProvider();
            capturedAuthoredState = true;

            authoredTurnAround = snapTurnProvider == null || snapTurnProvider.enableTurnAround;

            if (handMountedObjects == null)
            {
                authoredParents = System.Array.Empty<Transform>();
                return;
            }

            authoredParents = new Transform[handMountedObjects.Length];
            for (int i = 0; i < handMountedObjects.Length; i++)
                authoredParents[i] = handMountedObjects[i] != null ? handMountedObjects[i].parent : null;
        }

        void ResolveSnapTurnProvider()
        {
            if (snapTurnProvider == null)
                snapTurnProvider = GetComponentInChildren<SnapTurnProvider>(true);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            ResolveSnapTurnProvider();
        }
#endif
    }
}
