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

    [Header("Level Start Flash")]
    [SerializeField, Range(1, 8)] int startFlashCount = 4;
    [SerializeField, Min(0.05f)] float startFlashOnSeconds = 0.22f;
    [SerializeField, Min(0.05f)] float startFlashOffSeconds = 0.16f;

    GameManager gameManager;
    Transform rightController;
    Transform head;
    TextMeshPro timerText;
    Coroutine flashRoutine;
    float remainingSeconds;
    float targetAlpha;
    float revealUntilUnscaledTime;
    bool timerRunning;
    bool startFlashActive;
#if ENABLE_INPUT_SYSTEM
    InputAction revealAction;
#endif

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

        if (RevealPressed())
            RevealFor(visibleDurationSeconds);

        // Scaled time deliberately stops the countdown while VRPauseMenu sets timeScale to zero.
        remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
        UpdateDisplayText();

        if (!startFlashActive)
            targetAlpha = Time.unscaledTime < revealUntilUnscaledTime ? 1f : 0f;
        FadeDisplay();

        if (remainingSeconds <= 0f)
            ExpireTimer();
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
        timerRunning = true;
        revealUntilUnscaledTime = 0f;
        EnsureDisplay();
        UpdateDisplayText();

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashAtLevelStart());
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
        revealUntilUnscaledTime = 0f;
        startFlashActive = false;
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        targetAlpha = 0f;
        SetDisplayAlpha(0f);
    }

    void ExpireTimer()
    {
        timerRunning = false;
        remainingSeconds = 0f;
        startFlashActive = false;
        targetAlpha = 1f;
        SetDisplayAlpha(1f);
        UpdateDisplayText();

        if (!VRPauseMenu.TryShowTimeUpMenu())
            Debug.LogError("[MazeLevelTimer] Time expired, but the restart menu could not be found.", this);
    }

    IEnumerator FlashAtLevelStart()
    {
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

    void RevealFor(float seconds)
    {
        revealUntilUnscaledTime = Mathf.Max(revealUntilUnscaledTime, Time.unscaledTime + seconds);
        targetAlpha = 1f;
    }

    void FadeDisplay()
    {
        if (timerText == null) return;
        Color color = timerText.color;
        float duration = color.a < targetAlpha ? fadeInSeconds : fadeOutSeconds;
        float speed = duration <= 0.001f ? 1000f : 1f / duration;
        color.a = Mathf.MoveTowards(color.a, targetAlpha, Time.unscaledDeltaTime * speed);
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
