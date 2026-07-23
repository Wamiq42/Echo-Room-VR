using System.Collections;
using EchoRoom.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class MazeLevelTimer : MonoBehaviour
{
    [Header("Maze Time Limits")]
    [SerializeField, Min(1f)] float mazeATimeSeconds = 180f;
    [SerializeField, Min(1f)] float mazeBTimeSeconds = 180f;
    [SerializeField, Min(1f)] float mazeCTimeSeconds = 180f;
    [SerializeField, Min(1f)] float mazeDTimeSeconds = 180f;

    [Header("Controller Display")]
    [SerializeField] string rightControllerName = "Right Controller";
    [SerializeField] Vector3 displayOffsetFromView = new Vector3(-0.13f, 0.035f, 0.015f);
    [SerializeField, Min(0.001f)] float displayScale = 0.035f;
    [FormerlySerializedAs("buttonRevealSeconds")]
    [SerializeField, Min(0.1f), Tooltip("How long the timer remains fully visible after pressing A or keyboard T.")]
    float visibleDurationSeconds = 3f;
    [FormerlySerializedAs("fadeSeconds")]
    [SerializeField, Min(0.01f)] float fadeInSeconds = 0.18f;
    [SerializeField, Min(0.01f)] float fadeOutSeconds = 0.22f;
    [SerializeField] Color normalColor = new Color(0.34f, 0.94f, 0.94f, 1f);
    [SerializeField] Color lowTimeColor = new Color(1f, 0.36f, 0.20f, 1f);
    [SerializeField, Min(1f)] float lowTimeThresholdSeconds = 30f;

    [Header("Automatic Warnings")]
    [SerializeField, Min(0.1f), Tooltip("Base visibility for the 60-second warning. Later warnings remain visible longer.")]
    float automaticRevealSeconds = 3f;
    [SerializeField] AudioClip warningAudioClip;
    [SerializeField, Range(0f, 1f)] float warningAudioVolume = 0.65f;
    [SerializeField, Min(0.1f)] float warningAudioDurationSeconds = 1.1f;

    [Header("Level Start Flash")]
    [SerializeField, Range(1, 8)] int startFlashCount = 4;
    [SerializeField, Min(0.05f)] float startFlashOnSeconds = 0.22f;
    [SerializeField, Min(0.05f)] float startFlashOffSeconds = 0.16f;

    GameManager gameManager;
    Transform rightController;
    Transform head;
    TextMeshPro timerText;
    AudioSource warningAudioSource;
    Coroutine flashRoutine;
    Coroutine warningAudioRoutine;
    float remainingSeconds;
    float targetAlpha;
    float revealUntilTime;
    bool timerRunning;
    bool startFlashActive;
    bool warningAudioMissingLogged;
    int automaticWarningMask;
#if ENABLE_INPUT_SYSTEM
    InputAction revealAction;
#endif

    static readonly float[] AutomaticWarningThresholds = { 60f, 30f, 10f };

    public float RemainingSeconds => remainingSeconds;
    public bool IsRunning => timerRunning;

    void Awake()
    {
        CreateRevealAction();
        ResolveTrackingTargets();
        EnsureDisplay();
        SetDisplayAlpha(0f);
    }

    void Start()
    {
        gameManager = GetComponent<GameManager>();
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("[MazeLevelTimer] A GameManager is required.", this);
            enabled = false;
            return;
        }

        gameManager.OnLevelLoaded += HandleLevelLoaded;
        gameManager.OnLevelCompleted += HandleLevelCompleted;

        if (gameManager.CurrentLevelInstance != null && !gameManager.IsTutorialLevelActive)
            BeginForLevel(gameManager.CurrentLevelInstance.name);
    }

    void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        revealAction?.Enable();
#endif
    }

    void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        revealAction?.Disable();
#endif
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        timerRunning = false;
        startFlashActive = false;
        StopWarningAudio();
        targetAlpha = 0f;
        SetDisplayAlpha(0f);
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnLevelLoaded -= HandleLevelLoaded;
            gameManager.OnLevelCompleted -= HandleLevelCompleted;
        }
#if ENABLE_INPUT_SYSTEM
        revealAction?.Dispose();
#endif
    }

    void Update()
    {
        if (!timerRunning) return;

        TickTimer(Time.deltaTime, Time.time, Time.deltaTime, RevealPressed());
    }

    void LateUpdate()
    {
        if (timerText == null) return;
        if (rightController == null || head == null) ResolveTrackingTargets();
        if (rightController == null || head == null) return;

        Vector3 offset = head.right * displayOffsetFromView.x +
                         head.up * displayOffsetFromView.y +
                         head.forward * displayOffsetFromView.z;
        timerText.transform.position = rightController.position + offset;
        timerText.transform.rotation = Quaternion.LookRotation(timerText.transform.position - head.position, head.up);
    }

    void HandleLevelLoaded(Level level)
    {
        BeginForLevel(level != null ? level.levelName : string.Empty);
    }

    void HandleLevelCompleted(Level level)
    {
        StopTimer();
    }

    void BeginForLevel(string levelName)
    {
        float duration = GetDuration(levelName);
        if (duration <= 0f)
        {
            StopTimer();
            return;
        }

        remainingSeconds = duration;
        timerRunning = false;
        revealUntilTime = 0f;
        automaticWarningMask = 0;
        StopWarningAudio();
        EnsureDisplay();
        UpdateDisplayText();
        SetDisplayAlpha(0f);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(StartTimerWhenTransitionComplete());
    }

    float GetDuration(string levelName)
    {
        string value = string.IsNullOrWhiteSpace(levelName) ? string.Empty : levelName.Trim().ToUpperInvariant();
        if (value == "MAZE A" || value.Contains("5X5_A")) return mazeATimeSeconds;
        if (value == "MAZE B" || value.Contains("5X5_B")) return mazeBTimeSeconds;
        if (value == "MAZE C" || value.Contains("5X5_C")) return mazeCTimeSeconds;
        if (value == "MAZE D" || value.Contains("5X5_D")) return mazeDTimeSeconds;
        return -1f;
    }

    void StopTimer()
    {
        timerRunning = false;
        remainingSeconds = 0f;
        revealUntilTime = 0f;
        startFlashActive = false;
        automaticWarningMask = 0;
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        StopWarningAudio();
        targetAlpha = 0f;
        SetDisplayAlpha(0f);
    }

    void ExpireTimer()
    {
        timerRunning = false;
        remainingSeconds = 0f;
        startFlashActive = false;
        StopWarningAudio();
        UpdateDisplayText();

        if (VRPauseMenu.TryShowTimeUpMenu())
        {
            // The controller display sits in front of the time-up panel in VR. Hide it once the
            // restart menu is available so the final 00:00 readout cannot cover its controls.
            targetAlpha = 0f;
            SetDisplayAlpha(0f);
        }
        else
        {
            targetAlpha = 1f;
            SetDisplayAlpha(1f);
            Debug.LogError("[MazeLevelTimer] Time expired, but the restart menu could not be found.", this);
        }
    }

    IEnumerator StartTimerWhenTransitionComplete()
    {
        while (VRLoadingScreen.Instance != null && VRLoadingScreen.Instance.IsTransitioning)
            yield return null;

        timerRunning = true;
        startFlashActive = true;
        for (int i = 0; i < startFlashCount; i++)
        {
            targetAlpha = 1f;
            SetDisplayAlpha(1f);
            yield return new WaitForSecondsRealtime(startFlashOnSeconds);
            targetAlpha = 0f;
            SetDisplayAlpha(0f);
            yield return new WaitForSecondsRealtime(startFlashOffSeconds);
        }
        startFlashActive = false;
        flashRoutine = null;
        targetAlpha = 0f;
    }

    void TickTimer(float scaledDeltaTime, float presentationTime, float presentationDeltaTime, bool revealPressed)
    {
        if (revealPressed)
            RevealFor(visibleDurationSeconds, presentationTime);

        // Scaled time deliberately stops the countdown while VRPauseMenu sets timeScale to zero.
        float previousSeconds = remainingSeconds;
        remainingSeconds = Mathf.Max(0f, remainingSeconds - Mathf.Max(0f, scaledDeltaTime));
        UpdateDisplayText();

        if (remainingSeconds <= 0f)
        {
            ExpireTimer();
            return;
        }

        TryTriggerAutomaticWarning(previousSeconds, remainingSeconds, presentationTime);

        if (!startFlashActive)
            targetAlpha = presentationTime < revealUntilTime ? 1f : 0f;
        FadeDisplay(presentationDeltaTime);

    }

    void TryTriggerAutomaticWarning(float previousSeconds, float currentSeconds, float presentationTime)
    {
        int mostUrgentCrossing = -1;
        for (int i = 0; i < AutomaticWarningThresholds.Length; i++)
        {
            int bit = 1 << i;
            float threshold = AutomaticWarningThresholds[i];
            if ((automaticWarningMask & bit) == 0 && previousSeconds > threshold && currentSeconds <= threshold)
                mostUrgentCrossing = i;
        }

        if (mostUrgentCrossing < 0) return;

        // If one long frame crosses more than one threshold, show only the most urgent warning and
        // retire every less-urgent threshold so warnings cannot pile up on the same frame.
        for (int i = 0; i <= mostUrgentCrossing; i++)
            automaticWarningMask |= 1 << i;

        float revealSeconds = automaticRevealSeconds + mostUrgentCrossing * 0.75f;
        RevealFor(revealSeconds, presentationTime);
        PlayWarningAudio(mostUrgentCrossing);
        Debug.Log("[MazeLevelTimer] Automatic warning at " +
                  AutomaticWarningThresholds[mostUrgentCrossing].ToString("0") +
                  " seconds remaining.", this);
    }

    void RevealFor(float seconds)
    {
        RevealFor(seconds, Time.time);
    }

    void RevealFor(float seconds, float presentationTime)
    {
        revealUntilTime = Mathf.Max(revealUntilTime, presentationTime + seconds);
        targetAlpha = 1f;
    }

    void FadeDisplay(float presentationDeltaTime)
    {
        if (timerText == null) return;
        Color color = timerText.color;
        float duration = color.a < targetAlpha ? fadeInSeconds : fadeOutSeconds;
        float speed = duration <= 0.001f ? 1000f : 1f / duration;
        color.a = Mathf.MoveTowards(color.a, targetAlpha, Mathf.Max(0f, presentationDeltaTime) * speed);
        timerText.color = color;
    }

    void UpdateDisplayText()
    {
        if (timerText == null) return;
        int totalSeconds = Mathf.CeilToInt(remainingSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"TIME  {minutes:00}:{seconds:00}";

        Color baseColor = remainingSeconds <= lowTimeThresholdSeconds ? lowTimeColor : normalColor;
        baseColor.a = timerText.color.a;
        timerText.color = baseColor;
    }

    void EnsureDisplay()
    {
        if (timerText != null) return;
        ResolveTrackingTargets();

        GameObject display = new GameObject("Maze Timer Display");
        if (rightController != null) display.transform.SetParent(rightController, true);
        display.transform.localScale = Vector3.one * displayScale;

        timerText = display.AddComponent<TextMeshPro>();
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.fontSize = 2.2f;
        timerText.fontStyle = FontStyles.Bold;
        timerText.enableWordWrapping = false;
        timerText.rectTransform.sizeDelta = new Vector2(3.8f, 0.8f);
        timerText.outlineWidth = 0.18f;
        timerText.outlineColor = new Color32(0, 8, 12, 220);
        timerText.text = "TIME  00:00";
        timerText.color = normalColor;

        warningAudioSource = display.AddComponent<AudioSource>();
        warningAudioSource.playOnAwake = false;
        warningAudioSource.loop = false;
        warningAudioSource.spatialBlend = 0f;
        warningAudioSource.dopplerLevel = 0f;
    }

    void ResolveTrackingTargets()
    {
        if (Camera.main != null) head = Camera.main.transform;
        if (rightController != null) return;

        Transform[] transforms = FindObjectsOfType<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == rightControllerName)
            {
                rightController = transforms[i];
                break;
            }
        }
    }

    void SetDisplayAlpha(float alpha)
    {
        if (timerText == null) return;
        Color color = timerText.color;
        color.a = alpha;
        timerText.color = color;
    }

    void PlayWarningAudio(int urgency)
    {
        if (warningAudioSource == null || warningAudioClip == null)
        {
            if (!warningAudioMissingLogged)
            {
                Debug.LogWarning("[MazeLevelTimer] Automatic timer warning audio is not assigned.", this);
                warningAudioMissingLogged = true;
            }
            return;
        }

        warningAudioMissingLogged = false;
        if (warningAudioRoutine != null) StopCoroutine(warningAudioRoutine);
        warningAudioSource.Stop();
        warningAudioSource.clip = warningAudioClip;
        warningAudioSource.volume = Mathf.Clamp01(warningAudioVolume * (0.65f + urgency * 0.175f));
        warningAudioSource.pitch = 0.9f + urgency * 0.15f;
        warningAudioSource.Play();
        warningAudioRoutine = StartCoroutine(StopWarningAudioAfter(
            warningAudioDurationSeconds + urgency * 0.25f));
    }

    IEnumerator StopWarningAudioAfter(float seconds)
    {
        // Match the scaled countdown and reveal window: a pause must not consume a warning.
        yield return new WaitForSeconds(seconds);
        if (warningAudioSource != null)
        {
            warningAudioSource.Stop();
            warningAudioSource.pitch = 1f;
        }
        warningAudioRoutine = null;
    }

    void StopWarningAudio()
    {
        if (warningAudioRoutine != null)
        {
            StopCoroutine(warningAudioRoutine);
            warningAudioRoutine = null;
        }
        if (warningAudioSource == null) return;
        warningAudioSource.Stop();
        warningAudioSource.pitch = 1f;
    }

    void CreateRevealAction()
    {
#if ENABLE_INPUT_SYSTEM
        revealAction = new InputAction("Reveal Maze Timer", InputActionType.Button);
        revealAction.AddBinding("<XRController>{RightHand}/primaryButton");
        revealAction.AddBinding("<OculusTouchController>{RightHand}/buttonA");
        revealAction.AddBinding("<Gamepad>/buttonSouth");
        revealAction.AddBinding("<Keyboard>/t");
#endif
    }

    bool RevealPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return revealAction != null && revealAction.WasPressedThisFrame();
#else
        return false;
#endif
    }
}
