using UnityEngine;

public enum EchoSurfaceType
{
    Default,
    Metal,
    Concrete,
    Wood,
    Glass,
    FabricAbsorptive
}

public readonly struct EchoSurfaceResponse
{
    public static EchoSurfaceResponse Default => new EchoSurfaceResponse(EchoSurfaceType.Default, 1f, 1f);

    public EchoSurfaceType SurfaceType { get; }
    public float PitchMultiplier { get; }
    public float VolumeMultiplier { get; }

    public EchoSurfaceResponse(EchoSurfaceType surfaceType, float pitchMultiplier, float volumeMultiplier)
    {
        SurfaceType = surfaceType;
        PitchMultiplier = pitchMultiplier;
        VolumeMultiplier = volumeMultiplier;
    }
}

/// <summary>
/// Optional metadata for a deliberately authored echo surface. Objects without
/// this component retain the original pitch and prefab-authored volume exactly.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Echo Room/Sonar/Echo Surface")]
public sealed class EchoSurface : MonoBehaviour
{
    private const float MinimumPitch = 0.85f;
    private const float MaximumPitch = 1.15f;
    private const float MinimumVolumeMultiplier = 0.55f;
    private const float MaximumVolumeMultiplier = 1.15f;

    [SerializeField] private EchoSurfaceType surfaceType = EchoSurfaceType.Default;

    [Header("Authoring Trim")]
    [Tooltip("Small multiplier applied after the built-in surface pitch. Final pitch is clamped to 0.85-1.15.")]
    [SerializeField, Range(0.95f, 1.05f)] private float pitchTrim = 1f;

    [Tooltip("Multiplier applied after the built-in surface loudness and size contribution. The echo prefab's base volume remains authoritative.")]
    [SerializeField, Range(0.85f, 1.15f)] private float volumeTrim = 1f;

    public EchoSurfaceType SurfaceType => surfaceType;

    /// <summary>
    /// Resolves metadata from the hit collider or its nearest parent. Untagged
    /// surfaces intentionally return an exact 1x pitch and 1x volume response.
    /// </summary>
    public static EchoSurfaceResponse Resolve(Collider hitCollider)
    {
        if (hitCollider == null)
            return EchoSurfaceResponse.Default;

        EchoSurface surface = hitCollider.GetComponentInParent<EchoSurface>();
        return surface != null ? surface.BuildResponse(hitCollider) : EchoSurfaceResponse.Default;
    }

    /// <summary>
    /// Builds a response for diagnostics and tests without requiring a collider.
    /// The size contribution is logarithmic around a 2 m reference dimension and
    /// is deliberately limited to -5%/+8% so large combined maze colliders do not
    /// overwhelm the authored material character.
    /// </summary>
    public static EchoSurfaceResponse Evaluate(EchoSurfaceType type, float largestDimension, float pitchTrim = 1f, float volumeTrim = 1f)
    {
        GetSurfaceMultipliers(type, out float materialPitch, out float materialVolume);

        if (type == EchoSurfaceType.Default)
            return EchoSurfaceResponse.Default;

        float safePitchTrim = Mathf.Clamp(pitchTrim, 0.95f, 1.05f);
        float safeVolumeTrim = Mathf.Clamp(volumeTrim, 0.85f, 1.15f);
        float sizeMultiplier = CalculateSizeMultiplier(largestDimension);

        float pitch = Mathf.Clamp(materialPitch * safePitchTrim, MinimumPitch, MaximumPitch);
        float volume = Mathf.Clamp(
            materialVolume * sizeMultiplier * safeVolumeTrim,
            MinimumVolumeMultiplier,
            MaximumVolumeMultiplier);

        return new EchoSurfaceResponse(type, pitch, volume);
    }

    private EchoSurfaceResponse BuildResponse(Collider hitCollider)
    {
        float largestDimension = GetLargestDimension(hitCollider);
        return Evaluate(surfaceType, largestDimension, pitchTrim, volumeTrim);
    }

    private float GetLargestDimension(Collider hitCollider)
    {
        Bounds bounds = hitCollider.bounds;
        Vector3 size = bounds.size;
        float largestDimension = Mathf.Max(size.x, Mathf.Max(size.y, size.z));

        if (largestDimension > 0.001f)
            return largestDimension;

        Renderer surfaceRenderer = hitCollider.GetComponent<Renderer>();
        if (surfaceRenderer == null)
            surfaceRenderer = GetComponentInChildren<Renderer>();

        if (surfaceRenderer == null)
            return 2f;

        size = surfaceRenderer.bounds.size;
        return Mathf.Max(size.x, Mathf.Max(size.y, size.z));
    }

    private static float CalculateSizeMultiplier(float largestDimension)
    {
        float safeDimension = Mathf.Max(0.1f, largestDimension);
        float logarithmicContribution = Mathf.Log(safeDimension / 2f, 2f) * 0.035f;
        return 1f + Mathf.Clamp(logarithmicContribution, -0.05f, 0.08f);
    }

    private static void GetSurfaceMultipliers(EchoSurfaceType type, out float pitch, out float volume)
    {
        switch (type)
        {
            case EchoSurfaceType.Metal:
                pitch = 1.08f;
                volume = 0.96f;
                break;
            case EchoSurfaceType.Concrete:
                pitch = 0.97f;
                volume = 0.86f;
                break;
            case EchoSurfaceType.Wood:
                pitch = 0.92f;
                volume = 0.78f;
                break;
            case EchoSurfaceType.Glass:
                pitch = 1.12f;
                volume = 0.72f;
                break;
            case EchoSurfaceType.FabricAbsorptive:
                pitch = 0.88f;
                volume = 0.58f;
                break;
            default:
                pitch = 1f;
                volume = 1f;
                break;
        }
    }
}
