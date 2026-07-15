using System;
using UnityEngine;

public static class PrefabLightmapRuntime
{
    private static bool initialized;
    private static LightmapData[] sceneLightmaps = Array.Empty<LightmapData>();
    private static LightmapsMode sceneLightmapsMode;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        initialized = false;
        sceneLightmaps = Array.Empty<LightmapData>();
        sceneLightmapsMode = LightmapsMode.NonDirectional;
    }

    public static void InitializeSceneLighting()
    {
        if (initialized) return;

        LightmapData[] current = LightmapSettings.lightmaps;
        sceneLightmaps = current != null ? (LightmapData[])current.Clone() : Array.Empty<LightmapData>();
        sceneLightmapsMode = LightmapSettings.lightmapsMode;
        initialized = true;
    }

    public static bool Apply(GameObject levelRoot, LevelLightingData lightingData)
    {
        InitializeSceneLighting();

        if (levelRoot == null || lightingData == null)
        {
            RestoreSceneLighting();
            return false;
        }

        LevelLightingData.LightmapTextureSet[] levelMaps = lightingData.Lightmaps;
        LightmapData[] combined = new LightmapData[sceneLightmaps.Length + levelMaps.Length];
        Array.Copy(sceneLightmaps, combined, sceneLightmaps.Length);

        for (int i = 0; i < levelMaps.Length; i++)
        {
            LevelLightingData.LightmapTextureSet source = levelMaps[i];
            combined[sceneLightmaps.Length + i] = new LightmapData
            {
                lightmapColor = source.color,
                lightmapDir = source.direction,
                shadowMask = source.shadowMask
            };
        }

        LightmapSettings.lightmapsMode = ResolveLightmapsMode(lightingData);
        LightmapSettings.lightmaps = combined;

        int appliedCount = 0;
        LevelLightingData.RendererBinding[] bindings = lightingData.RendererBindings;
        for (int i = 0; i < bindings.Length; i++)
        {
            LevelLightingData.RendererBinding binding = bindings[i];
            Renderer target = ResolveRenderer(levelRoot.transform, binding);
            if (target == null)
            {
                Debug.LogWarning("[PrefabLightmapRuntime] Could not resolve renderer '" +
                                 binding.rendererPath + "' under '" + levelRoot.name + "'.");
                continue;
            }

            if (binding.lightmapIndex < 0 || binding.lightmapIndex >= levelMaps.Length)
            {
                Debug.LogWarning("[PrefabLightmapRuntime] Invalid local lightmap index " +
                                 binding.lightmapIndex + " for renderer '" + binding.rendererPath + "'.");
                continue;
            }

            target.lightmapIndex = sceneLightmaps.Length + binding.lightmapIndex;
            target.lightmapScaleOffset = binding.lightmapScaleOffset;
            appliedCount++;
        }

        return appliedCount == bindings.Length;
    }

    private static LightmapsMode ResolveLightmapsMode(LevelLightingData lightingData)
    {
        LightmapsMode requestedMode = lightingData.LightmapsMode;
        if (Enum.IsDefined(typeof(LightmapsMode), requestedMode))
            return requestedMode;

        LevelLightingData.LightmapTextureSet[] maps = lightingData.Lightmaps;
        bool hasDirectionMaps = false;
        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i].direction == null) continue;
            hasDirectionMaps = true;
            break;
        }

        LightmapsMode fallback = hasDirectionMaps ? (LightmapsMode)1 : LightmapsMode.NonDirectional;
        Debug.LogWarning("[PrefabLightmapRuntime] Lighting asset '" + lightingData.name +
                         "' contains invalid lightmaps mode " + (int)requestedMode +
                         ". Falling back to " + fallback + ".");
        return fallback;
    }

    public static void RestoreSceneLighting()
    {
        if (!initialized) return;
        LightmapSettings.lightmapsMode = sceneLightmapsMode;
        LightmapSettings.lightmaps = (LightmapData[])sceneLightmaps.Clone();
    }

    private static Renderer ResolveRenderer(
        Transform levelRoot,
        LevelLightingData.RendererBinding binding)
    {
        Transform current = levelRoot;
        int[] childIndices = binding.childIndices;

        if (childIndices != null)
        {
            for (int i = 0; i < childIndices.Length; i++)
            {
                int childIndex = childIndices[i];
                if (childIndex < 0 || childIndex >= current.childCount) return null;
                current = current.GetChild(childIndex);
            }
        }

        Renderer[] renderers = current.GetComponents<Renderer>();
        if (binding.rendererComponentIndex < 0 || binding.rendererComponentIndex >= renderers.Length)
            return null;

        return renderers[binding.rendererComponentIndex];
    }
}
