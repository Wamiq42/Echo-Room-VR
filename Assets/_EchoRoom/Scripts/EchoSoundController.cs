using UnityEngine;

/// <summary>
/// Controls playback and lifetime of a spawned echo sound.
/// Automatically sets spatial settings and self-destroys after a short delay.
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

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    private void Start()
    {
        if (audioSource.clip == null)
        {
            LogDebug("No AudioClip assigned to echo.");
            return;
        }

        audioSource.Play();
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Assigns a custom clip at runtime before echo playback.
    /// </summary>
    public void SetClip(AudioClip clip)
    {
        audioSource.clip = clip;
    }

    /// <summary>
    /// Adjusts the volume of the echo at runtime.
    /// </summary>
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    private void LogDebug(string msg)
    {
        if (!isDebugging) return;
        Debug.Log($"[EchoSoundController] {msg}");
    }
}