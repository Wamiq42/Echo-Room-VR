using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

namespace EchoRoom.Diagnostics
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-900)]
    public sealed class AOPlaytestLogger : MonoBehaviour
    {
        [Header("Sampling")]
        [SerializeField, Min(0.25f)] private float sampleIntervalSeconds = 1f;
        [SerializeField, Min(1f)] private float flushIntervalSeconds = 5f;
        [SerializeField] private bool logSonarPulses = true;
        [SerializeField] private bool logViewedRevealMaterial = true;

        private const string RevealShaderName = "EchoRoom/EchoSonarReveal";
        private const string PulseArrayName = "_SonarPulses";
        private const string PulseRangesArrayName = "_SonarPulseRanges";
        private const string LingerOverrideName = "_EchoRevealLingerOverride";

        private static AOPlaytestLogger instance;

        private readonly List<XRDisplaySubsystem> xrDisplays = new List<XRDisplaySubsystem>();
        private readonly HashSet<int> seenPulseIds = new HashSet<int>();
        private readonly FrameTiming[] frameTimings = new FrameTiming[1];

        private StreamWriter writer;
        private string logPath;
        private float nextSampleTime;
        private float nextFlushTime;
        private double frameMsSum;
        private float frameMsMax;
        private int frameSamples;
        private string lastAOConfig;
        private string lastHealth;
        private string lastTarget;
        private bool closing;
        private double sessionStartRealtime;

        private struct AOConfig
        {
            public bool found;
            public bool active;
            public bool afterOpaque;
            public bool downsample;
            public bool pipelineDepth;
            public string featureName;
            public string rendererName;
            public string source;
            public string normalSamples;
            public string blurQuality;
            public string aoMethod;
            public float intensity;
            public float radius;
            public float falloff;
            public float directLightingStrength;
        }

        private struct TargetSnapshot
        {
            public bool found;
            public string objectPath;
            public string materialName;
            public float distance;
            public float aoVisibility;
            public float revealQuality;
            public float occlusionStrength;
            public bool hasOcclusionMap;
            public float calculatedReveal;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<AOPlaytestLogger>() != null)
                return;

            GameObject loggerObject = new GameObject("AO Playtest Logger");
            DontDestroyOnLoad(loggerObject);
            loggerObject.AddComponent<AOPlaytestLogger>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            sessionStartRealtime = Time.realtimeSinceStartupAsDouble;
            OpenLog();
            Application.logMessageReceived += OnUnityLog;

            WriteEvent("session_start",
                "{\"unity\":" + Q(Application.unityVersion) +
                ",\"platform\":" + Q(Application.platform.ToString()) +
                ",\"device\":" + Q(SystemInfo.deviceModel) +
                ",\"graphicsDevice\":" + Q(SystemInfo.graphicsDeviceName) +
                ",\"graphicsApi\":" + Q(SystemInfo.graphicsDeviceType.ToString()) +
                ",\"qualityLevel\":" + Q(QualitySettings.names[QualitySettings.GetQualityLevel()]) +
                ",\"targetFrameRate\":" + Application.targetFrameRate.ToString(CultureInfo.InvariantCulture) +
                ",\"logPath\":" + Q(logPath) + "}");

            CaptureConfiguration(true);
            nextSampleTime = Time.unscaledTime + 0.25f;
            nextFlushTime = Time.unscaledTime + flushIntervalSeconds;
            Debug.Log("[AOPlaytestLogger] Recording to: " + logPath);
        }

        private void Update()
        {
            float ms = Time.unscaledDeltaTime * 1000f;
            if (ms > 0f && ms < 1000f)
            {
                frameMsSum += ms;
                frameMsMax = Mathf.Max(frameMsMax, ms);
                frameSamples++;
            }

            FrameTimingManager.CaptureFrameTimings();

            if (Time.unscaledTime < nextSampleTime)
                return;

            nextSampleTime = Time.unscaledTime + Mathf.Max(0.25f, sampleIntervalSeconds);
            CaptureSample();

            if (Time.unscaledTime >= nextFlushTime)
            {
                Flush();
                nextFlushTime = Time.unscaledTime + Mathf.Max(1f, flushIntervalSeconds);
            }
        }

        public void MarkIssue(string note)
        {
            WriteEvent("tester_marker", "{\"note\":" + Q(note) + "}");
            Flush();
        }

        private void CaptureSample()
        {
            AOConfig config = CaptureConfiguration(false);
            Camera camera = Camera.main;
            TargetSnapshot target = logViewedRevealMaterial ? ReadTarget(camera) : new TargetSnapshot();
            CapturePulses();

            double averageMs = frameSamples > 0 ? frameMsSum / frameSamples : 0.0;
            double estimatedFps = averageMs > 0.0 ? 1000.0 / averageMs : 0.0;
            double cpuFrameMs = 0.0;
            double gpuFrameMs = 0.0;
            uint timingCount = FrameTimingManager.GetLatestTimings(1, frameTimings);
            if (timingCount > 0)
            {
                cpuFrameMs = frameTimings[0].cpuFrameTime;
                gpuFrameMs = frameTimings[0].gpuFrameTime;
            }

            bool xrRunning;
            float refreshRate;
            ReadXR(out xrRunning, out refreshRate);
            double targetMs = refreshRate > 1f ? 1000.0 / refreshRate :
                (Application.targetFrameRate > 0 ? 1000.0 / Application.targetFrameRate : 0.0);

            bool cameraDepth = false;
            string cameraName = "none";
            if (camera != null)
            {
                cameraName = camera.name;
                UniversalAdditionalCameraData data = camera.GetComponent<UniversalAdditionalCameraData>();
                cameraDepth = data != null && data.requiresDepthTexture;
            }

            string dataJson =
                "{\"avgFrameMs\":" + N(averageMs) +
                ",\"maxFrameMs\":" + N(frameMsMax) +
                ",\"estimatedFps\":" + N(estimatedFps) +
                ",\"cpuFrameMs\":" + N(cpuFrameMs) +
                ",\"gpuFrameMs\":" + N(gpuFrameMs) +
                ",\"targetFrameMs\":" + N(targetMs) +
                ",\"xrRunning\":" + B(xrRunning) +
                ",\"refreshHz\":" + N(refreshRate) +
                ",\"camera\":" + Q(cameraName) +
                ",\"cameraDepth\":" + B(cameraDepth) +
                ",\"aoFeatureFound\":" + B(config.found) +
                ",\"aoFeatureActive\":" + B(config.active) +
                ",\"afterOpaque\":" + B(config.afterOpaque) +
                ",\"viewTarget\":" + Q(target.found ? target.objectPath : "none") +
                ",\"viewMaterial\":" + Q(target.found ? target.materialName : "none") +
                ",\"viewDistance\":" + N(target.distance) +
                ",\"viewAOVisibility\":" + N(target.aoVisibility) +
                ",\"viewRevealQuality\":" + N(target.revealQuality) +
                ",\"viewHasOcclusionMap\":" + B(target.hasOcclusionMap) +
                ",\"calculatedReveal\":" + N(target.calculatedReveal) + "}";

            WriteEvent("sample", dataJson);
            EvaluateHealth(config, cameraDepth, target, averageMs, targetMs);

            frameMsSum = 0.0;
            frameMsMax = 0f;
            frameSamples = 0;
        }

        private AOConfig CaptureConfiguration(bool force)
        {
            AOConfig result = ReadAOConfig();
            Shader revealShader = Shader.Find(RevealShaderName);
            bool shaderSupported = revealShader != null && revealShader.isSupported;

            string fingerprint =
                result.found + "|" + result.active + "|" + result.afterOpaque + "|" +
                result.downsample + "|" + result.pipelineDepth + "|" + result.source + "|" +
                result.normalSamples + "|" + result.blurQuality + "|" + result.aoMethod + "|" +
                result.intensity.ToString("R", CultureInfo.InvariantCulture) + "|" +
                result.radius.ToString("R", CultureInfo.InvariantCulture) + "|" +
                result.directLightingStrength.ToString("R", CultureInfo.InvariantCulture) + "|" +
                shaderSupported;

            if (force || fingerprint != lastAOConfig)
            {
                lastAOConfig = fingerprint;
                WriteEvent("ao_config",
                    "{\"featureFound\":" + B(result.found) +
                    ",\"featureName\":" + Q(result.featureName) +
                    ",\"renderer\":" + Q(result.rendererName) +
                    ",\"active\":" + B(result.active) +
                    ",\"afterOpaque\":" + B(result.afterOpaque) +
                    ",\"downsample\":" + B(result.downsample) +
                    ",\"pipelineDepth\":" + B(result.pipelineDepth) +
                    ",\"source\":" + Q(result.source) +
                    ",\"normalSamples\":" + Q(result.normalSamples) +
                    ",\"blurQuality\":" + Q(result.blurQuality) +
                    ",\"aoMethod\":" + Q(result.aoMethod) +
                    ",\"intensity\":" + N(result.intensity) +
                    ",\"radius\":" + N(result.radius) +
                    ",\"falloff\":" + N(result.falloff) +
                    ",\"directLightingStrength\":" + N(result.directLightingStrength) +
                    ",\"revealShaderFound\":" + B(revealShader != null) +
                    ",\"revealShaderSupported\":" + B(shaderSupported) + "}");
                Flush();
            }

            return result;
        }

        private AOConfig ReadAOConfig()
        {
            AOConfig result = new AOConfig();
            UniversalRenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
                return result;

            result.pipelineDepth = pipeline.supportsCameraDepthTexture;
            object rendererListObject = ReadField(pipeline, "m_RendererDataList");
            ScriptableRendererData[] rendererList = rendererListObject as ScriptableRendererData[];
            if (rendererList == null || rendererList.Length == 0)
                return result;

            object defaultIndexObject = ReadField(pipeline, "m_DefaultRendererIndex");
            int defaultIndex = defaultIndexObject is int ? (int)defaultIndexObject : 0;
            defaultIndex = Mathf.Clamp(defaultIndex, 0, rendererList.Length - 1);
            ScriptableRendererData renderer = rendererList[defaultIndex];
            if (renderer == null)
                return result;

            result.rendererName = renderer.name;
            foreach (ScriptableRendererFeature feature in renderer.rendererFeatures)
            {
                if (feature == null)
                    continue;

                string combined = (feature.name + " " + feature.GetType().Name).ToLowerInvariant();
                if (!combined.Contains("ambientocclusion") && !combined.Contains("ambient occlusion"))
                    continue;

                result.found = true;
                result.featureName = feature.name;
                result.active = feature.isActive;
                object settings = ReadField(feature, "m_Settings");
                if (settings == null)
                    return result;

                result.afterOpaque = ReadBool(settings, "AfterOpaque");
                result.downsample = ReadBool(settings, "Downsample");
                result.source = ReadString(settings, "Source");
                result.normalSamples = ReadString(settings, "NormalSamples");
                result.blurQuality = ReadString(settings, "BlurQuality");
                result.aoMethod = ReadString(settings, "AOMethod");
                result.intensity = ReadFloat(settings, "Intensity");
                result.radius = ReadFloat(settings, "Radius");
                result.falloff = ReadFloat(settings, "Falloff");
                result.directLightingStrength = ReadFloat(settings, "DirectLightingStrength");
                return result;
            }

            return result;
        }

        private TargetSnapshot ReadTarget(Camera camera)
        {
            TargetSnapshot result = new TargetSnapshot();
            if (camera == null)
                return result;

            RaycastHit[] hits = Physics.RaycastAll(
                camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)),
                100f, ~0, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, delegate(RaycastHit a, RaycastHit b) { return a.distance.CompareTo(b.distance); });

            Shader revealShader = Shader.Find(RevealShaderName);
            foreach (RaycastHit hit in hits)
            {
                Renderer renderer = hit.collider.GetComponentInParent<Renderer>();
                if (renderer == null)
                    continue;

                Material material = null;
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null && materials[i].shader == revealShader)
                    {
                        material = materials[i];
                        break;
                    }
                }

                if (material == null)
                    continue;

                result.found = true;
                result.objectPath = HierarchyPath(renderer.transform);
                result.materialName = material.name;
                result.distance = hit.distance;
                result.aoVisibility = GetFloat(material, "_AOVisibilityStrength");
                result.revealQuality = GetFloat(material, "_RevealQuality");
                result.occlusionStrength = GetFloat(material, "_OcclusionStrength");
                result.hasOcclusionMap = material.HasProperty("_OcclusionMap") &&
                    material.GetTexture("_OcclusionMap") != null;
                result.calculatedReveal = CalculateReveal(material, hit.point);

                string fingerprint = result.objectPath + "|" + result.materialName + "|" +
                    result.aoVisibility.ToString("R", CultureInfo.InvariantCulture) + "|" +
                    result.revealQuality.ToString("R", CultureInfo.InvariantCulture) + "|" +
                    result.occlusionStrength.ToString("R", CultureInfo.InvariantCulture) + "|" +
                    result.hasOcclusionMap;

                if (fingerprint != lastTarget)
                {
                    lastTarget = fingerprint;
                    WriteEvent("target_change",
                        "{\"object\":" + Q(result.objectPath) +
                        ",\"material\":" + Q(result.materialName) +
                        ",\"distance\":" + N(result.distance) +
                        ",\"aoVisibility\":" + N(result.aoVisibility) +
                        ",\"revealQuality\":" + N(result.revealQuality) +
                        ",\"occlusionStrength\":" + N(result.occlusionStrength) +
                        ",\"hasOcclusionMap\":" + B(result.hasOcclusionMap) + "}");
                }

                return result;
            }

            if (lastTarget != "none")
            {
                lastTarget = "none";
                WriteEvent("target_change", "{\"object\":\"none\",\"material\":\"none\"}");
            }

            return result;
        }

        private float CalculateReveal(Material material, Vector3 point)
        {
            Vector4[] pulses = Shader.GetGlobalVectorArray(PulseArrayName);
            if (pulses == null || pulses.Length == 0)
                return 0f;

            Vector4[] pulseRanges = Shader.GetGlobalVectorArray(PulseRangesArrayName);

            float speed = Mathf.Max(0.001f, GetFloat(material, "_RevealSpeed"));
            float materialRadius = Mathf.Max(0.001f, GetFloat(material, "_RevealRadius"));
            float linger = Shader.GetGlobalFloat(LingerOverrideName);
            if (linger <= 0f)
                linger = Mathf.Max(0.001f, GetFloat(material, "_RevealLinger"));

            float strongest = 0f;
            for (int i = 0; i < pulses.Length; i++)
            {
                Vector4 pulse = pulses[i];
                if (pulse.w <= 0f)
                    continue;

                float maxRadius = pulseRanges != null && i < pulseRanges.Length && pulseRanges[i].x > 0f
                    ? pulseRanges[i].x
                    : materialRadius;

                float age = Time.time - pulse.w;
                if (age < 0f || age > maxRadius / speed + linger)
                    continue;

                Vector3 origin = new Vector3(pulse.x, pulse.y, pulse.z);
                float distance = Vector3.Distance(point, origin);
                float fromEdge = age * speed - distance;
                if (fromEdge <= 0f || distance > maxRadius)
                    continue;

                float timeSince = fromEdge / speed;
                float lingerAmount = Mathf.Clamp01(1f - timeSince / linger);
                float distanceFalloff = Mathf.Clamp01(1f - distance / maxRadius);
                strongest = Mathf.Max(strongest, lingerAmount * distanceFalloff);
            }

            return strongest;
        }

        private void CapturePulses()
        {
            if (!logSonarPulses)
                return;

            Vector4[] pulses = Shader.GetGlobalVectorArray(PulseArrayName);
            if (pulses == null)
                return;

            Vector4[] pulseRanges = Shader.GetGlobalVectorArray(PulseRangesArrayName);

            for (int i = 0; i < pulses.Length; i++)
            {
                Vector4 pulse = pulses[i];
                if (pulse.w <= 0f)
                    continue;

                int id = Mathf.RoundToInt(pulse.w * 1000f);
                if (!seenPulseIds.Add(id))
                    continue;

                WriteEvent("sonar_pulse",
                    "{\"slot\":" + i.ToString(CultureInfo.InvariantCulture) +
                    ",\"startTime\":" + N(pulse.w) +
                    ",\"visualRange\":" + N(pulseRanges != null && i < pulseRanges.Length ? pulseRanges[i].x : 0f) +
                    ",\"origin\":[" + N(pulse.x) + "," + N(pulse.y) + "," + N(pulse.z) + "]}");
                Flush();
            }
        }

        private void EvaluateHealth(
            AOConfig config, bool cameraDepth, TargetSnapshot target, double averageMs, double targetMs)
        {
            List<string> issues = new List<string>();
            if (!config.found) issues.Add("SSAO renderer feature is missing");
            else if (!config.active) issues.Add("SSAO renderer feature is inactive");
            if (config.afterOpaque) issues.Add("After Opaque is enabled, so the reveal shader cannot sample SSAO");
            if (!config.pipelineDepth) issues.Add("URP depth texture support is disabled");
            if (!cameraDepth) issues.Add("Main Camera is not requesting a depth texture");
            if (target.found && target.aoVisibility <= 0.001f)
                issues.Add("Viewed reveal material has AO Visibility Strength set to zero");
            if (averageMs > 0.0 && targetMs > 0.0 && averageMs > targetMs * 1.1)
                issues.Add("Average frame time is more than 10 percent over the XR frame budget");

            string health = issues.Count == 0 ? "OK" : string.Join("; ", issues.ToArray());
            if (health == lastHealth)
                return;

            lastHealth = health;
            WriteEvent(issues.Count == 0 ? "health_ok" : "health_warning",
                "{\"message\":" + Q(health) + "}");
            Flush();
        }

        private void ReadXR(out bool running, out float refreshRate)
        {
            running = false;
            refreshRate = 0f;
            xrDisplays.Clear();
            SubsystemManager.GetSubsystems(xrDisplays);
            for (int i = 0; i < xrDisplays.Count; i++)
            {
                XRDisplaySubsystem display = xrDisplays[i];
                if (display == null || !display.running)
                    continue;

                running = true;
                float rate;
                if (display.TryGetDisplayRefreshRate(out rate))
                    refreshRate = Mathf.Max(refreshRate, rate);
            }
        }

        private void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            if (closing || writer == null || condition.StartsWith("[AOPlaytestLogger]"))
                return;
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Warning)
                return;

            WriteEvent("unity_log",
                "{\"severity\":" + Q(type.ToString()) +
                ",\"message\":" + Q(condition) +
                ",\"stack\":" + Q(stackTrace) + "}");
            Flush();
        }

        private void OpenLog()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string directory = Path.Combine(projectRoot, "Logs", "AOPlaytests");
            Directory.CreateDirectory(directory);
            logPath = Path.Combine(directory, "AOPlaytest_latest.jsonl");
            FileStream stream = new FileStream(
                logPath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan);
            writer = new StreamWriter(stream, new UTF8Encoding(false), 4096);
            writer.AutoFlush = false;
        }

        private void WriteEvent(string type, string dataJson)
        {
            if (writer == null)
                return;

            writer.WriteLine(
                "{\"utc\":" + Q(DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)) +
                ",\"sessionSeconds\":" + N(Time.realtimeSinceStartupAsDouble - sessionStartRealtime) +
                ",\"frame\":" + Time.frameCount.ToString(CultureInfo.InvariantCulture) +
                ",\"type\":" + Q(type) +
                ",\"data\":" + dataJson + "}");
        }

        private void Flush()
        {
            if (writer != null)
                writer.Flush();
        }

        private void CloseLog(string reason)
        {
            if (closing)
                return;

            closing = true;
            Application.logMessageReceived -= OnUnityLog;
            if (writer != null)
            {
                WriteEvent("session_end", "{\"reason\":" + Q(reason) + "}");
                writer.Flush();
                writer.Dispose();
                writer = null;
            }
        }

        private void OnApplicationQuit()
        {
            CloseLog("application_quit");
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                CloseLog("component_destroyed");
                instance = null;
            }
        }

        private static object ReadField(object target, string name)
        {
            if (target == null)
                return null;

            Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                    return field.GetValue(target);
                type = type.BaseType;
            }

            return null;
        }

        private static bool ReadBool(object target, string field)
        {
            object value = ReadField(target, field);
            return value is bool && (bool)value;
        }

        private static float ReadFloat(object target, string field)
        {
            object value = ReadField(target, field);
            if (value is float) return (float)value;
            if (value is double) return (float)(double)value;
            return 0f;
        }

        private static string ReadString(object target, string field)
        {
            object value = ReadField(target, field);
            return value != null ? value.ToString() : "unknown";
        }

        private static float GetFloat(Material material, string property)
        {
            return material != null && material.HasProperty(property)
                ? material.GetFloat(property)
                : 0f;
        }

        private static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private static string Q(string value)
        {
            if (value == null)
                return "null";

            StringBuilder builder = new StringBuilder(value.Length + 8);
            builder.Append('\"');
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (c < 32) builder.Append("\\u").Append(((int)c).ToString("x4"));
                        else builder.Append(c);
                        break;
                }
            }
            builder.Append('\"');
            return builder.ToString();
        }

        private static string N(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return "0";
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string B(bool value)
        {
            return value ? "true" : "false";
        }
    }
}
