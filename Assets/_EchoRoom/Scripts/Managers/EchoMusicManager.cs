using System.Collections;
using EchoRoom.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class EchoMusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip ambientMusic;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.42f;
    [SerializeField, Range(0f, 1f)] private float menuVolume = 0.5f;
    [SerializeField, Min(0.05f)] private float fadeSeconds = 1.25f;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private static EchoMusicManager instance;
    private AudioSource ambientSource;
    private AudioSource menuSource;
    private Coroutine fadeRoutine;
    private bool pauseMenuOpen;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            instance.ApplyClipsIfMissing(ambientMusic, menuMusic);
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ConfigureSources();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        VRPauseMenu.OnMenuStateChanged += OnPauseMenuStateChanged;
    }

    private void Start()
    {
        RefreshMusicForCurrentState(true);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        VRPauseMenu.OnMenuStateChanged -= OnPauseMenuStateChanged;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pauseMenuOpen = false;
        RefreshMusicForCurrentState(false);
    }

    private void OnPauseMenuStateChanged(VRPauseMenu.MenuState state)
    {
        pauseMenuOpen = state != VRPauseMenu.MenuState.Hidden;
        RefreshMusicForCurrentState(false);
    }

    private void RefreshMusicForCurrentState(bool immediate)
    {
        bool useMenuMusic = pauseMenuOpen || SceneManager.GetActiveScene().name == mainMenuSceneName;
        FadeTo(useMenuMusic ? MusicMode.Menu : MusicMode.Ambient, immediate);
    }

    private void ConfigureSources()
    {
        ambientSource = CreateSource("Ambient Music", ambientMusic);
        menuSource = CreateSource("Menu Music", menuMusic);
    }

    private AudioSource CreateSource(string sourceName, AudioClip clip)
    {
        var child = new GameObject(sourceName);
        child.transform.SetParent(transform, false);

        AudioSource source = child.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        source.volume = 0f;
        return source;
    }

    private void ApplyClipsIfMissing(AudioClip ambientClip, AudioClip menuClip)
    {
        if (ambientMusic == null && ambientClip != null) ambientMusic = ambientClip;
        if (menuMusic == null && menuClip != null) menuMusic = menuClip;
        if (ambientSource != null && ambientSource.clip == null) ambientSource.clip = ambientMusic;
        if (menuSource != null && menuSource.clip == null) menuSource.clip = menuMusic;
    }

    private void FadeTo(MusicMode mode, bool immediate)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(mode, immediate ? 0f : fadeSeconds));
    }

    private IEnumerator FadeRoutine(MusicMode mode, float duration)
    {
        float ambientStart = ambientSource.volume;
        float menuStart = menuSource.volume;
        float ambientTarget = mode == MusicMode.Ambient ? ambientVolume : 0f;
        float menuTarget = mode == MusicMode.Menu ? menuVolume : 0f;

        EnsurePlaying(ambientSource, ambientTarget > 0f || ambientStart > 0f);
        EnsurePlaying(menuSource, menuTarget > 0f || menuStart > 0f);

        if (duration <= 0f)
        {
            ambientSource.volume = ambientTarget;
            menuSource.volume = menuTarget;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                ambientSource.volume = Mathf.Lerp(ambientStart, ambientTarget, t);
                menuSource.volume = Mathf.Lerp(menuStart, menuTarget, t);
                yield return null;
            }
        }

        ambientSource.volume = ambientTarget;
        menuSource.volume = menuTarget;
        StopIfSilent(ambientSource);
        StopIfSilent(menuSource);
        fadeRoutine = null;
    }

    private static void EnsurePlaying(AudioSource source, bool shouldPlay)
    {
        if (source == null || source.clip == null || !shouldPlay || source.isPlaying) return;
        source.Play();
    }

    private static void StopIfSilent(AudioSource source)
    {
        if (source != null && source.volume <= 0.0001f && source.isPlaying)
            source.Stop();
    }

    private enum MusicMode
    {
        Ambient,
        Menu
    }
}
