using UnityEngine.XR;

namespace EchoRoom.Settings
{
    /// <summary>
    /// The single answer to "which controller is this input on?". Bindings in this project are
    /// built in code in four separate files; without one table they drift apart and a handedness
    /// switch leaves some system still listening to a controller the player has put down.
    /// Nothing here is cached -- reads are cheap PlayerPrefs-backed lookups and the setting can
    /// change mid-session from the pause menu.
    /// </summary>
    public static class HandedInput
    {
        const string LeftDevice = "<XRController>{LeftHand}/";
        const string RightDevice = "<XRController>{RightHand}/";

        public static Handedness Preference => EchoRoomSettings.Handedness;

        /// <summary>True while all gameplay input is collapsed onto one controller.</summary>
        public static bool IsSingleHand => Preference != Handedness.Both;

        /// <summary>
        /// The controller gameplay input reads from. Both-mode answers Right so the historical
        /// two-handed scheme is untouched -- every binding it owned stays exactly where it was.
        /// </summary>
        public static Handedness ActiveHand =>
            Preference == Handedness.Left ? Handedness.Left : Handedness.Right;

        public static bool IsLeftActive => ActiveHand == Handedness.Left;
        public static XRNode ActiveNode => IsLeftActive ? XRNode.LeftHand : XRNode.RightHand;
        public static XRNode InactiveNode => IsLeftActive ? XRNode.RightHand : XRNode.LeftHand;
        public static HapticHand ActiveHapticHand => IsLeftActive ? HapticHand.Left : HapticHand.Right;

        static string ActiveDevice => IsLeftActive ? LeftDevice : RightDevice;

        // ---------- Control paths (Input System) ----------

        /// <summary>Sonar ping. Single-hand play moves it to A/X; two-handed play keeps B.</summary>
        public static string SonarPingPath =>
            IsSingleHand ? ActiveDevice + "primaryButton" : RightDevice + "secondaryButton";

        /// <summary>Objective/timer panel. Single-hand play moves it to B/Y; two-handed keeps A.</summary>
        public static string ObjectivePanelPath =>
            IsSingleHand ? ActiveDevice + "secondaryButton" : RightDevice + "primaryButton";

        /// <summary>
        /// Microphone push-to-talk. B/Y is spoken for in single-hand play, so the shout moves to
        /// the grip -- the only remaining control that can be comfortably held.
        /// </summary>
        public static string MicrophonePingPath =>
            IsSingleHand ? ActiveDevice + "{GripButton}" : LeftDevice + "secondaryButton";


        /// <summary>Legacy XR feature for the sonar ping, for the raw InputDevices read path.</summary>
        public static InputFeatureUsage<bool> SonarPingUsage =>
            IsSingleHand ? CommonUsages.primaryButton : CommonUsages.secondaryButton;

        // ---------- Labels for player-facing copy ----------

        public static string PrimaryFaceLabel => IsLeftActive ? "X" : "A";
        public static string SecondaryFaceLabel => IsLeftActive ? "Y" : "B";

        public static string SonarPingLabel => IsSingleHand ? PrimaryFaceLabel : "B";
        public static string ObjectivePanelLabel => IsSingleHand ? SecondaryFaceLabel : "A";
        public static string MicrophonePingLabel => IsSingleHand ? "GRIP" : "Y";


        public static string HandednessLabel
        {
            get
            {
                switch (Preference)
                {
                    case Handedness.Left: return "LEFT";
                    case Handedness.Right: return "RIGHT";
                    default: return "BOTH";
                }
            }
        }

        // ---------- Helpers ----------

        /// <summary>
        /// Collapses a haptic request onto the controller the player is actually holding. Without
        /// this a one-handed player silently loses every cue authored for the other hand, including
        /// the Both-hand entity-proximity rumble that is meant to be a warning.
        /// </summary>
        public static HapticHand ResolveHapticHand(HapticHand requested)
        {
            if (requested == HapticHand.None || !IsSingleHand) return requested;
            return ActiveHapticHand;
        }
    }
}
