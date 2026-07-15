using UnityEngine;

/// <summary>
/// Controls the single playback and lifetime of a spawned echo sound.
/// Call ConfigureAndPlay after instantiation so pitch and volume are applied
/// before the AudioSource starts.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EchoSoundController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private float lifetime = 5f;

    [Header("Debug")]
    [SerializeField] private bool isDebugging = false;

    private const float MinimumPitch = 0.85f;
    private const float MaximumPitch = 1.15f;
    private const float MinimumVolumeMultiplier = 0.55f;
    private const float MaximumVolumeMultiplier = 1.15f;

    private AudioSource audioSource;
    private float baseVolume;
    private bool playbackStarted;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        baseVolume = Mathf.Clamp01(audioSource.volume);

        // Spawned echoes must be configured before they play. This also guards
        // older prefab data that may still have Play On Awake enabled.
        audioSource.playOnAwake = false;
        audioSource.Stop();

        audioSource.spatialBlend = 1f;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    /// <summary>
    /// Applies bounded material/size multipliers to the prefab-authored sound,
    /// then starts it exactly once. This controller owns the only lifetime
    /// destruction scheduled for a correctly configured echo instance.
    /// </summary>
    public bool ConfigureAndPlay(float pitchMultiplier, float volumeMultiplier)
    {
        if (playbackStarted)
        {
            LogDebug("Ignored duplicate ConfigureAndPlay call.");
            return false;
        }

        playbackStarted = true;
        audioSource.pitch = Mathf.Clamp(pitchMultiplier, MinimumPitch, MaximumPitch);

        float safeVolumeMultiplier = Mathf.Clamp(
            volumeMultiplier,
            MinimumVolumeMultiplier,
            MaximumVolumeMultiplier);
        audioSource.volume = Mathf.Clamp01(baseVolume * safeVolumeMultiplier);

        if (audioSource.clip == null)
        {
            LogDebug("No AudioClip assigned to echo.");
            Destroy(gameObject, Mathf.Max(0.1f, lifetime));
            return false;
        }

        audioSource.Play();
        Destroy(gameObject, Mathf.Max(0.1f, lifetime));
        return true;
    }

    /// <summary>
    /// Assigns a custom clip at runtime before echo playback.
    /// </summary>
    public void SetClip(AudioClip clip)
    {
        audioSource.clip = clip;
    }

    private void LogDebug(string msg)
    {
        if (!isDebugging) return;
        Debug.Log($"[EchoSoundController] {msg}");
    }
}
