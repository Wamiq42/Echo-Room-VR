using System;
using UnityEngine;

namespace EchoRoom.Settings
{
    public enum LocomotionMode
    {
        Smooth = 0,
        Teleport = 1
    }

    public enum TurnMode
    {
        Snap = 0,
        Smooth = 1
    }

    /// <summary>
    /// Which controller carries gameplay input. Left and Right are one-handed play; the unused
    /// controller can be put down entirely.
    /// </summary>
    public enum Handedness
    {
        Both = 0,
        Left = 1,
        Right = 2
    }

    public enum EchoRoomSetting
    {
        LocomotionMode,
        TurnMode,
        VignetteEnabled,
        HeightOffset,
        TurnSpeed,
        MasterVolume,
        MusicVolume,
        SfxVolume,
        SubtitlesEnabled,
        Handedness
    }

    /// <summary>
    /// Persistent, UI-independent player preferences. Gameplay systems can safely read these
    /// values before any menu or settings panel has been instantiated.
    /// </summary>
    public static class EchoRoomSettings
    {
        const string Prefix = "EchoRoom.Settings.";
        const string LocomotionModeKey = Prefix + "LocomotionMode";
        const string TurnModeKey = Prefix + "TurnMode";
        const string VignetteEnabledKey = Prefix + "VignetteEnabled";
        const string HeightOffsetKey = Prefix + "HeightOffsetMeters";
        const string TurnSpeedKey = Prefix + "TurnSpeedDegreesPerSecond";
        const string HandednessKey = Prefix + "Handedness";

        // Reserved now so later audio/accessibility work extends the same settings namespace.
        const string MasterVolumeKey = Prefix + "MasterVolume";
        const string MusicVolumeKey = Prefix + "MusicVolume";
        const string SfxVolumeKey = Prefix + "SfxVolume";
        const string SubtitlesEnabledKey = Prefix + "SubtitlesEnabled";

        public const float MinimumTurnSpeed = 30f;
        public const float MaximumTurnSpeed = 120f;
        public const float TurnSpeedStep = 15f;
        public const float MinimumHeightOffset = -0.3f;
        public const float MaximumHeightOffset = 0.3f;
        public const float HeightOffsetStep = 0.05f;

        public static event Action<EchoRoomSetting> Changed;

        public static LocomotionMode LocomotionMode
        {
            get => ReadEnum(LocomotionModeKey, global::EchoRoom.Settings.LocomotionMode.Smooth);
            set => WriteInt(LocomotionModeKey, (int)value, EchoRoomSetting.LocomotionMode);
        }

        public static TurnMode TurnMode
        {
            get => ReadEnum(TurnModeKey, global::EchoRoom.Settings.TurnMode.Snap);
            set => WriteInt(TurnModeKey, (int)value, EchoRoomSetting.TurnMode);
        }

        public static Handedness Handedness
        {
            get => ReadEnum(HandednessKey, global::EchoRoom.Settings.Handedness.Both);
            set => WriteInt(HandednessKey, (int)value, EchoRoomSetting.Handedness);
        }

        public static bool VignetteEnabled
        {
            get => PlayerPrefs.GetInt(VignetteEnabledKey, 1) != 0;
            set => WriteInt(VignetteEnabledKey, value ? 1 : 0, EchoRoomSetting.VignetteEnabled);
        }

        public static float HeightOffset
        {
            get => Mathf.Clamp(PlayerPrefs.GetFloat(HeightOffsetKey, 0f), MinimumHeightOffset, MaximumHeightOffset);
            set => WriteFloat(HeightOffsetKey, Mathf.Clamp(value, MinimumHeightOffset, MaximumHeightOffset),
                EchoRoomSetting.HeightOffset);
        }

        public static float TurnSpeed
        {
            get => Mathf.Clamp(PlayerPrefs.GetFloat(TurnSpeedKey, 60f), MinimumTurnSpeed, MaximumTurnSpeed);
            set => WriteFloat(TurnSpeedKey, Mathf.Clamp(value, MinimumTurnSpeed, MaximumTurnSpeed),
                EchoRoomSetting.TurnSpeed);
        }

        public static float MasterVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
            set => WriteFloat(MasterVolumeKey, Mathf.Clamp01(value), EchoRoomSetting.MasterVolume);
        }

        public static float MusicVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
            set => WriteFloat(MusicVolumeKey, Mathf.Clamp01(value), EchoRoomSetting.MusicVolume);
        }

        public static float SfxVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
            set => WriteFloat(SfxVolumeKey, Mathf.Clamp01(value), EchoRoomSetting.SfxVolume);
        }

        public static bool SubtitlesEnabled
        {
            get => PlayerPrefs.GetInt(SubtitlesEnabledKey, 0) != 0;
            set => WriteInt(SubtitlesEnabledKey, value ? 1 : 0, EchoRoomSetting.SubtitlesEnabled);
        }

        static T ReadEnum<T>(string key, T fallback) where T : struct, Enum
        {
            int raw = PlayerPrefs.GetInt(key, Convert.ToInt32(fallback));
            return Enum.IsDefined(typeof(T), raw) ? (T)Enum.ToObject(typeof(T), raw) : fallback;
        }

        static void WriteInt(string key, int value, EchoRoomSetting setting)
        {
            if (PlayerPrefs.HasKey(key) && PlayerPrefs.GetInt(key) == value) return;
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            Changed?.Invoke(setting);
        }

        static void WriteFloat(string key, float value, EchoRoomSetting setting)
        {
            if (PlayerPrefs.HasKey(key) && Mathf.Approximately(PlayerPrefs.GetFloat(key), value)) return;
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            Changed?.Invoke(setting);
        }
    }
}
