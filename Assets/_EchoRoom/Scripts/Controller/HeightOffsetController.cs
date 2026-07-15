using System.Collections;
using System.Collections.Generic;
using EchoRoom.Settings;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace EchoRoom.Controller
{
    /// <summary>
    /// Applies the saved player height relative to XROrigin's tracking-origin baseline.
    /// Only the CameraFloorOffsetObject is adjusted; the tracked camera and XR Origin root remain untouched.
    /// </summary>
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XROrigin))]
    public sealed class HeightOffsetController : MonoBehaviour
    {
        const float PositionEpsilon = 0.0001f;

        readonly List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();

        XROrigin xrOrigin;
        Transform cameraFloorOffsetTransform;
        Coroutine pendingReapply;
        float baselineLocalY;
        float lastAppliedOffset;
        bool hasBaseline;

        public float BaselineLocalY => baselineLocalY;
        public float AppliedHeightOffset => lastAppliedOffset;
        public float TargetLocalY => baselineLocalY + lastAppliedOffset;
        public Transform CameraFloorOffsetTransform => cameraFloorOffsetTransform;

        void Awake()
        {
            if (!ResolveReferences())
                return;

            CaptureInitialBaseline();
            ApplySettings();
        }

        void OnEnable()
        {
            EchoRoomSettings.Changed += OnSettingChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SubsystemManager.afterReloadSubsystems += OnSubsystemsReloaded;
            RefreshSubsystemSubscriptions();
            ApplySettings();
        }

        IEnumerator Start()
        {
            // XROrigin initializes its tracking mode in Start. Waiting one frame lets it write
            // the Floor (0) or Device/Unbounded (CameraYOffset) baseline before we add the preference.
            yield return null;
            ReconcileAfterXROriginChange();
        }

        void OnDisable()
        {
            EchoRoomSettings.Changed -= OnSettingChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SubsystemManager.afterReloadSubsystems -= OnSubsystemsReloaded;
            UnsubscribeInputSubsystems();

            if (pendingReapply != null)
            {
                StopCoroutine(pendingReapply);
                pendingReapply = null;
            }
        }

        void LateUpdate()
        {
            if (!ResolveReferences() || !hasBaseline)
                return;

            float desiredLocalY = baselineLocalY + ClampOffset(EchoRoomSettings.HeightOffset);
            if (!Mathf.Approximately(cameraFloorOffsetTransform.localPosition.y, desiredLocalY))
                ReconcileAfterXROriginChange();
        }

        public void ApplySettings()
        {
            if (!ResolveReferences())
                return;

            if (!hasBaseline)
                CaptureInitialBaseline();

            lastAppliedOffset = ClampOffset(EchoRoomSettings.HeightOffset);
            Vector3 localPosition = cameraFloorOffsetTransform.localPosition;
            localPosition.y = baselineLocalY + lastAppliedOffset;
            cameraFloorOffsetTransform.localPosition = localPosition;
        }

        void CaptureInitialBaseline()
        {
            baselineLocalY = cameraFloorOffsetTransform.localPosition.y;
            lastAppliedOffset = 0f;
            hasBaseline = true;
        }

        void ReconcileAfterXROriginChange()
        {
            if (!ResolveReferences())
                return;

            float currentLocalY = cameraFloorOffsetTransform.localPosition.y;
            if (TryGetTrackingOriginBaseline(out float trackingBaseline))
            {
                baselineLocalY = trackingBaseline;
                hasBaseline = true;
            }
            else if (!hasBaseline || !Mathf.Approximately(currentLocalY, baselineLocalY + lastAppliedOffset))
            {
                // Unknown/no XR runtime: treat an external reset as the new raw baseline. Never
                // subtract or add the preference here, which avoids cumulative drift.
                baselineLocalY = currentLocalY;
                hasBaseline = true;
            }

            ApplySettings();
        }

        bool TryGetTrackingOriginBaseline(out float baseline)
        {
            baseline = 0f;
            if (xrOrigin == null)
                return false;

            switch (xrOrigin.CurrentTrackingOriginMode)
            {
                case TrackingOriginModeFlags.Floor:
                    baseline = 0f;
                    return true;
                case TrackingOriginModeFlags.Device:
                case TrackingOriginModeFlags.Unbounded:
                    baseline = xrOrigin.CameraYOffset;
                    return true;
                default:
                    return false;
            }
        }

        void OnSettingChanged(EchoRoomSetting setting)
        {
            if (setting == EchoRoomSetting.HeightOffset)
                ApplySettings();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ScheduleReapply();

        void OnTrackingOriginUpdated(XRInputSubsystem subsystem) => ScheduleReapply();

        void OnSubsystemsReloaded()
        {
            RefreshSubsystemSubscriptions();
            ScheduleReapply();
        }

        void ScheduleReapply()
        {
            if (!isActiveAndEnabled)
                return;

            if (pendingReapply != null)
                StopCoroutine(pendingReapply);
            pendingReapply = StartCoroutine(ReapplyNextFrame());
        }

        IEnumerator ReapplyNextFrame()
        {
            // XROrigin also handles trackingOriginUpdated. Reapply on the following frame so its
            // baseline write always completes first regardless of callback subscription order.
            yield return null;
            pendingReapply = null;
            ReconcileAfterXROriginChange();
        }

        void RefreshSubsystemSubscriptions()
        {
            UnsubscribeInputSubsystems();
            SubsystemManager.GetSubsystems(inputSubsystems);
            for (int i = 0; i < inputSubsystems.Count; i++)
            {
                XRInputSubsystem subsystem = inputSubsystems[i];
                if (subsystem != null)
                    subsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
            }
        }

        void UnsubscribeInputSubsystems()
        {
            for (int i = 0; i < inputSubsystems.Count; i++)
            {
                XRInputSubsystem subsystem = inputSubsystems[i];
                if (subsystem != null)
                    subsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
            }
            inputSubsystems.Clear();
        }

        bool ResolveReferences()
        {
            if (xrOrigin == null)
                xrOrigin = GetComponent<XROrigin>();

            GameObject offsetObject = xrOrigin != null ? xrOrigin.CameraFloorOffsetObject : null;
            cameraFloorOffsetTransform = offsetObject != null ? offsetObject.transform : null;
            if (cameraFloorOffsetTransform != null)
                return true;

            Debug.LogError("[HeightOffsetController] XROrigin.CameraFloorOffsetObject must be assigned.", this);
            enabled = false;
            return false;
        }

        static float ClampOffset(float value)
        {
            // The shared settings currently expose -0.30..+0.30 m, inside the task's conservative
            // -0.50..+0.50 m safety envelope.
            return Mathf.Clamp(value, -0.5f, 0.5f);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Application.isPlaying)
                ResolveReferences();
        }
#endif
    }
}