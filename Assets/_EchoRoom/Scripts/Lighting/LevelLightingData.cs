using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelLightingData", menuName = "Echo Room/Level Lighting Data")]
public sealed class LevelLightingData : ScriptableObject
{
    [Serializable]
    public struct LightmapTextureSet
    {
        public Texture2D color;
        public Texture2D direction;
        public Texture2D shadowMask;
    }

    [Serializable]
    public struct RendererBinding
    {
        [Tooltip("Sibling-index path from the level root to the renderer Transform.")]
        public int[] childIndices;

        [Tooltip("Index within GetComponents<Renderer>() on the target Transform.")]
        public int rendererComponentIndex;

        [Tooltip("Readable path used for validation messages.")]
        public string rendererPath;

        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;
    }

    [SerializeField] private LightmapsMode lightmapsMode = LightmapsMode.NonDirectional;
    [SerializeField] private LightmapTextureSet[] lightmaps = Array.Empty<LightmapTextureSet>();
    [SerializeField] private RendererBinding[] rendererBindings = Array.Empty<RendererBinding>();

    [Header("Capture Information")]
    [SerializeField] private string sourceScene;
    [SerializeField] private string sourcePrefab;
    [SerializeField] private string capturedUtc;

    public LightmapsMode LightmapsMode => lightmapsMode;
    public LightmapTextureSet[] Lightmaps => lightmaps;
    public RendererBinding[] RendererBindings => rendererBindings;
    public string SourceScene => sourceScene;
    public string SourcePrefab => sourcePrefab;
    public string CapturedUtc => capturedUtc;

#if UNITY_EDITOR
    public void SetCapturedData(
        LightmapsMode mode,
        LightmapTextureSet[] capturedLightmaps,
        RendererBinding[] capturedBindings,
        string scenePath,
        string prefabPath,
        string captureTimeUtc)
    {
        lightmapsMode = mode;
        lightmaps = capturedLightmaps ?? Array.Empty<LightmapTextureSet>();
        rendererBindings = capturedBindings ?? Array.Empty<RendererBinding>();
        sourceScene = scenePath;
        sourcePrefab = prefabPath;
        capturedUtc = captureTimeUtc;
    }
#endif
}
