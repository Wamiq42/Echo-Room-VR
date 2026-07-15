using System;
using EchoRoom.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoRoom.UI
{
    /// <summary>Owns bindings for the shared settings screen and releases every binding on dispose.</summary>
    internal sealed class VRSettingsPanelController : IDisposable
    {
        readonly Action onBack;
        readonly Button locomotionModeButton;
        readonly Button turnModeButton;
        readonly Button turnSpeedDownButton;
        readonly Button turnSpeedUpButton;
        readonly Label turnSpeedValue;
        readonly Button vignetteButton;
        readonly Button heightOffsetDownButton;
        readonly Button heightOffsetUpButton;
        readonly Label heightOffsetValue;
        readonly Button backButton;
        bool disposed;

        public bool IsValid { get; }

        public VRSettingsPanelController(VisualElement root, Action onBack)
        {
            this.onBack = onBack;
            locomotionModeButton = root?.Q<Button>("locomotion-mode-button");
            turnModeButton = root?.Q<Button>("turn-mode-button");
            turnSpeedDownButton = root?.Q<Button>("turn-speed-down-button");
            turnSpeedUpButton = root?.Q<Button>("turn-speed-up-button");
            turnSpeedValue = root?.Q<Label>("turn-speed-value");
            vignetteButton = root?.Q<Button>("vignette-button");
            heightOffsetDownButton = root?.Q<Button>("height-offset-down-button");
            heightOffsetUpButton = root?.Q<Button>("height-offset-up-button");
            heightOffsetValue = root?.Q<Label>("height-offset-value");
            backButton = root?.Q<Button>("settings-back-button");

            IsValid = locomotionModeButton != null && turnModeButton != null &&
                      turnSpeedDownButton != null && turnSpeedUpButton != null && turnSpeedValue != null &&
                      vignetteButton != null && heightOffsetDownButton != null && heightOffsetUpButton != null &&
                      heightOffsetValue != null && backButton != null;
            if (!IsValid)
            {
                Debug.LogError("[VRSettings] One or more required settings UI elements are missing.");
                return;
            }

            locomotionModeButton.clicked += ToggleLocomotionMode;
            turnModeButton.clicked += ToggleTurnMode;
            turnSpeedDownButton.clicked += DecreaseTurnSpeed;
            turnSpeedUpButton.clicked += IncreaseTurnSpeed;
            vignetteButton.clicked += ToggleVignette;
            heightOffsetDownButton.clicked += DecreaseHeightOffset;
            heightOffsetUpButton.clicked += IncreaseHeightOffset;
            backButton.clicked += ReturnToPreviousScreen;
            EchoRoomSettings.Changed += OnSettingChanged;
            Refresh();
        }

        public void Refresh()
        {
            if (!IsValid || disposed) return;
            locomotionModeButton.text = EchoRoomSettings.LocomotionMode == LocomotionMode.Smooth
                ? "SMOOTH"
                : "TELEPORT";
            turnModeButton.text = EchoRoomSettings.TurnMode == TurnMode.Snap ? "SNAP" : "SMOOTH";
            turnSpeedValue.text = Mathf.RoundToInt(EchoRoomSettings.TurnSpeed) + " DEG/S";
            vignetteButton.text = EchoRoomSettings.VignetteEnabled ? "ON" : "OFF";
            heightOffsetValue.text = EchoRoomSettings.HeightOffset.ToString("+0.00;-0.00;0.00") + " M";

            bool smoothTurning = EchoRoomSettings.TurnMode == TurnMode.Smooth;
            turnSpeedDownButton.SetEnabled(smoothTurning &&
                                           EchoRoomSettings.TurnSpeed > EchoRoomSettings.MinimumTurnSpeed);
            turnSpeedUpButton.SetEnabled(smoothTurning &&
                                         EchoRoomSettings.TurnSpeed < EchoRoomSettings.MaximumTurnSpeed);
            heightOffsetDownButton.SetEnabled(EchoRoomSettings.HeightOffset > EchoRoomSettings.MinimumHeightOffset);
            heightOffsetUpButton.SetEnabled(EchoRoomSettings.HeightOffset < EchoRoomSettings.MaximumHeightOffset);
        }

        void ToggleLocomotionMode()
        {
            EchoRoomSettings.LocomotionMode = EchoRoomSettings.LocomotionMode == LocomotionMode.Smooth
                ? LocomotionMode.Teleport
                : LocomotionMode.Smooth;
        }

        void ToggleTurnMode()
        {
            EchoRoomSettings.TurnMode = EchoRoomSettings.TurnMode == TurnMode.Snap
                ? TurnMode.Smooth
                : TurnMode.Snap;
        }

        void DecreaseTurnSpeed() => EchoRoomSettings.TurnSpeed -= EchoRoomSettings.TurnSpeedStep;
        void IncreaseTurnSpeed() => EchoRoomSettings.TurnSpeed += EchoRoomSettings.TurnSpeedStep;
        void ToggleVignette() => EchoRoomSettings.VignetteEnabled = !EchoRoomSettings.VignetteEnabled;
        void DecreaseHeightOffset() => EchoRoomSettings.HeightOffset -= EchoRoomSettings.HeightOffsetStep;
        void IncreaseHeightOffset() => EchoRoomSettings.HeightOffset += EchoRoomSettings.HeightOffsetStep;
        void ReturnToPreviousScreen() => onBack?.Invoke();
        void OnSettingChanged(EchoRoomSetting _) => Refresh();

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            EchoRoomSettings.Changed -= OnSettingChanged;
            if (!IsValid) return;
            locomotionModeButton.clicked -= ToggleLocomotionMode;
            turnModeButton.clicked -= ToggleTurnMode;
            turnSpeedDownButton.clicked -= DecreaseTurnSpeed;
            turnSpeedUpButton.clicked -= IncreaseTurnSpeed;
            vignetteButton.clicked -= ToggleVignette;
            heightOffsetDownButton.clicked -= DecreaseHeightOffset;
            heightOffsetUpButton.clicked -= IncreaseHeightOffset;
            backButton.clicked -= ReturnToPreviousScreen;
        }
    }
}
