using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EchoRoom.XR
{
    /// <summary>
    /// Requests the highest display refresh rate the Android OpenXR runtime supports, capped at 90 Hz.
    /// The request is submitted whenever an OpenXR session begins, even when the runtime already
    /// reports 90 Hz, so Horizon OS records an app-specific preference for the focused session.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = "Echo Room Display Refresh Rate",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Echo Room",
        Desc = "Requests the highest Android OpenXR display refresh rate supported up to 90 Hz.",
        OpenxrExtensionStrings = ExtensionString,
        Version = "1.0.0",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = FeatureId)]
#endif
    public sealed class EchoRoomDisplayRefreshRateFeature : OpenXRFeature
    {
        public const string FeatureId = "com.echoroom.openxr.feature.displayrefreshrate";
        public const string ExtensionString = "XR_FB_display_refresh_rate";
        public const int MaximumRefreshRate = 90;
        public const int FallbackFrameRate = 72;

        /// <summary>The refresh rate accepted for the active session, or zero before a successful request.</summary>
        public static float AppliedRefreshRate { get; private set; }

        /// <summary>True after the runtime accepts this session's refresh-rate request.</summary>
        public static bool HasAppliedRefreshRate => AppliedRefreshRate > 0f;

        /// <summary>The refresh rates most recently enumerated from the runtime.</summary>
        public static float[] SupportedRefreshRates { get; private set; } = Array.Empty<float>();

        private static EchoRoomDisplayRefreshRateFeature s_instance;

        private ulong m_session;
        private EnumerateDisplayRefreshRatesDelegate m_enumerateRates;
        private RequestDisplayRefreshRateDelegate m_requestRate;
        private GetDisplayRefreshRateDelegate m_getRate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetInstanceProcAddrDelegate(
            ulong instance,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            out IntPtr function);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int EnumerateDisplayRefreshRatesDelegate(
            ulong session,
            uint capacityInput,
            out uint countOutput,
            [In, Out] float[] rates);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int RequestDisplayRefreshRateDelegate(ulong session, float refreshRate);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetDisplayRefreshRateDelegate(ulong session, out float refreshRate);

        private static bool Succeeded(int result)
        {
            return result >= 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySafeStartupFallback()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!HasAppliedRefreshRate)
                ApplyUnityFramePacing(FallbackFrameRate);
#endif
        }

        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            AppliedRefreshRate = 0f;
            SupportedRefreshRates = Array.Empty<float>();

            if (!OpenXRRuntime.IsExtensionEnabled(ExtensionString))
            {
                Debug.LogWarning(
                    $"[EchoRoomRefreshRate] {ExtensionString} is unavailable. Using the {FallbackFrameRate} FPS fallback.");
                return false;
            }

            if (xrGetInstanceProcAddr == IntPtr.Zero)
            {
                Debug.LogError(
                    $"[EchoRoomRefreshRate] xrGetInstanceProcAddr was null. Using the {FallbackFrameRate} FPS fallback.");
                return false;
            }

            var getProcAddr =
                Marshal.GetDelegateForFunctionPointer<GetInstanceProcAddrDelegate>(xrGetInstanceProcAddr);

            if (!TryResolve(getProcAddr, xrInstance, "xrEnumerateDisplayRefreshRatesFB", out m_enumerateRates) ||
                !TryResolve(getProcAddr, xrInstance, "xrRequestDisplayRefreshRateFB", out m_requestRate) ||
                !TryResolve(getProcAddr, xrInstance, "xrGetDisplayRefreshRateFB", out m_getRate))
            {
                return false;
            }

            s_instance = this;
            return true;
        }

        protected override void OnSessionBegin(ulong xrSession)
        {
            m_session = xrSession;

            if (!ApplyBestSupportedRate())
                ApplyFailureFallback();
        }

        protected override void OnSessionEnd(ulong xrSession)
        {
            m_session = 0;
            AppliedRefreshRate = 0f;
        }

        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            s_instance = null;
            m_session = 0;
            m_enumerateRates = null;
            m_requestRate = null;
            m_getRate = null;
            SupportedRefreshRates = Array.Empty<float>();
            AppliedRefreshRate = 0f;
        }

        /// <summary>
        /// Re-enumerates supported rates and re-applies the highest one up to 90 Hz for the active session.
        /// </summary>
        public static bool TryApplyBestSupportedRefreshRate()
        {
            if (s_instance == null || s_instance.m_session == 0)
            {
                Debug.LogWarning("[EchoRoomRefreshRate] No active OpenXR session is available.");
                return false;
            }

            bool applied = s_instance.ApplyBestSupportedRate();
            if (!applied)
                ApplyFailureFallback();

            return applied;
        }

        private static bool TryResolve<T>(
            GetInstanceProcAddrDelegate getProcAddr,
            ulong xrInstance,
            string functionName,
            out T function) where T : Delegate
        {
            function = null;

            int result = getProcAddr(xrInstance, functionName, out IntPtr address);
            if (!Succeeded(result) || address == IntPtr.Zero)
            {
                Debug.LogError(
                    $"[EchoRoomRefreshRate] Failed to resolve {functionName} (XrResult {result}).");
                return false;
            }

            function = Marshal.GetDelegateForFunctionPointer<T>(address);
            return true;
        }

        private bool ApplyBestSupportedRate()
        {
            if (!TryEnumerateRates(out float[] rates))
                return false;

            SupportedRefreshRates = rates;
            Debug.Log($"[EchoRoomRefreshRate] Runtime supports: {string.Join(", ", rates)} Hz.");

            float selectedRate = 0f;
            for (int i = 0; i < rates.Length; i++)
            {
                float rate = rates[i];
                if (rate <= MaximumRefreshRate && rate > selectedRate)
                    selectedRate = rate;
            }

            if (selectedRate <= 0f)
            {
                Debug.LogWarning(
                    $"[EchoRoomRefreshRate] No supported rate at or below {MaximumRefreshRate} Hz.");
                return false;
            }

            // Always submit the app preference. Quest Home may already report 90 Hz before focus,
            // then Horizon OS can apply the app's default 72 Hz mode unless this session requests 90.
            int getResult = m_getRate(m_session, out float currentRate);
            if (Succeeded(getResult))
            {
                Debug.Log(
                    $"[EchoRoomRefreshRate] Display currently reports {currentRate} Hz; " +
                    $"requesting {selectedRate} Hz for this app session.");
            }
            else
            {
                Debug.LogWarning(
                    $"[EchoRoomRefreshRate] Current rate query failed (XrResult {getResult}); " +
                    $"still requesting {selectedRate} Hz.");
            }

            int requestResult = m_requestRate(m_session, selectedRate);
            if (!Succeeded(requestResult))
            {
                Debug.LogWarning(
                    $"[EchoRoomRefreshRate] Request for {selectedRate} Hz was rejected " +
                    $"(XrResult {requestResult}).");
                return false;
            }

            AppliedRefreshRate = selectedRate;
            ApplyUnityFramePacing(Mathf.RoundToInt(selectedRate));

            Debug.Log(
                $"[EchoRoomRefreshRate] Applied {selectedRate} Hz; Unity VSync is disabled and " +
                $"Application.targetFrameRate is {Application.targetFrameRate}.");
            return true;
        }

        private bool TryEnumerateRates(out float[] rates)
        {
            rates = Array.Empty<float>();

            int countResult = m_enumerateRates(m_session, 0, out uint count, null);
            if (!Succeeded(countResult) || count == 0)
            {
                Debug.LogWarning(
                    $"[EchoRoomRefreshRate] Runtime reported no supported refresh rates " +
                    $"(XrResult {countResult}).");
                return false;
            }

            var buffer = new float[count];
            int enumerateResult = m_enumerateRates(m_session, count, out uint returnedCount, buffer);
            if (!Succeeded(enumerateResult) || returnedCount == 0)
            {
                Debug.LogWarning(
                    $"[EchoRoomRefreshRate] Failed to enumerate refresh rates " +
                    $"(XrResult {enumerateResult}).");
                return false;
            }

            if (returnedCount < buffer.Length)
                Array.Resize(ref buffer, (int)returnedCount);

            rates = buffer;
            return true;
        }

        private static void ApplyFailureFallback()
        {
            AppliedRefreshRate = 0f;
            ApplyUnityFramePacing(FallbackFrameRate);
            Debug.LogWarning(
                $"[EchoRoomRefreshRate] Using the safe {FallbackFrameRate} FPS fallback for this session.");
        }

        private static void ApplyUnityFramePacing(int frameRate)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = frameRate;
        }
    }
}