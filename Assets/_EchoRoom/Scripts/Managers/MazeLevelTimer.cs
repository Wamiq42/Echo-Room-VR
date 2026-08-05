using System.Collections;
using EchoRoom.Settings;
using EchoRoom.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Drives the wrist status panel for the timed mazes: the countdown on top, the current
/// objective and lever progress below. The panel is hidden by default -- the player peeks at it
/// with the face button handedness assigns -- and flashes itself on level start, on every lever
/// change, and on the automatic
/// low-time warnings, so progress is never missed without leaving a permanent HUD in a game
/// built around darkness.
/// </summary>
[DisallowMultipleComponent]
public sealed class MazeLevelTimer : MonoBehaviour
{
    [Header("Maze Time Limits")]
    [SerializeField, Min(1f)] float mazeATimeSeconds = 360f;
    [SerializeField, Min(1f)] float mazeBTimeSeconds = 360f;
    [SerializeField, Min(1f)] float mazeCTimeSeconds = 360f;
    [SerializeField, Min(1f)] float mazeDTimeSeconds = 360f;

    [Header("Controller Display")]
    [FormerlySerializedAs("rightController")]
    [SerializeField, Tooltip("Controller the panel is strapped to. HandednessController overrides " +
                             "this at runtime so the panel always rides the hand in play.")]
    Transform handAnchor;
    [SerializeField] Transform head;
    [SerializeField] Vector3 displayOffsetFromView = new Vector3(-0.13f, 0.035f, 0.015f);
    [SerializeField, Min(0.00001f), Tooltip("World size of one panel pixel. The panel layout is 560x300.")]
    float panelWorldScale = 0.00028f;
    [FormerlySerializedAs("buttonRevealSeconds")]
    [SerializeField, Min(0.1f), Tooltip("How long the panel remains fully visible after pressing A or keyboard T.")]
    float visibleDurationSeconds = 3f;
    [FormerlySerializedAs("fadeSeconds")]
    [SerializeField, Min(0.01f)] float fadeInSeconds = 0.18f;
    [SerializeField, Min(0.01f)] float fadeOutSeconds = 0.22f;
    [SerializeField, Min(1f)] float lowTimeThresholdSeconds = 30f;

    [Header("Objective")]
    [SerializeField, Min(0.1f), Tooltip("How long the panel auto-reveals when a lever changes state.")]
    float objectiveFlashSeconds = 2.5f;

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

    const string PanelLayoutResource = "UI/VRObjectivePanel";
    const string PanelStylesResource = "UI/VRObjectivePanelStyles";
    const string PanelSettingsResource = "UI/VRMenuPanelSettings";
    static readonly Vector2 PanelSize = new Vector2(560f, 300f);

    const string ObjectiveFindLevers = "FIND THE LEVERS AND ACTIVATE THEM TO ESCAPE";
    const string ObjectiveAllLeversActive = "ALL LEVERS ACTIVE - FIND THE EXIT";

    GameManager gameManager;
    UIDocument panelDocument;
    PanelSettings panelSettingsInstance;
    VisualElement panelRoot;
    Label timerLabel;
    Label objectiveLabel;
    Label progressLabel;
    EchoPuzzleController puzzle;
    AudioSource warningAudioSource;
    Coroutine flashRoutine;
    Coroutine warningAudioRoutine;
    float remainingSeconds;
    float targetAlpha;
    float displayAlpha;
    float revealUntilTime;
    bool timerRunning;
    bool startFlashActive;
    bool warningAudioMissingLogged;
    bool handAnchorMissingLogged;
    int automaticWarningMask;

    // Tutorial demo: the same panel and countdown, but it can never fail the player.
    bool demoMode;
    string demoObjective = ObjectiveFindLevers;
    int demoActivated;
    int demoTotal;

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
        LeverInteractable.OnAnyLeverStateChanged += HandleLeverStateChanged;
        EchoButtonInteractable.OnAnyButtonPressed += HandleButtonPressed;
        EchoRoomSettings.Changed += OnSettingChanged;
    }

    void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        revealAction?.Disable();
#endif
        LeverInteractable.OnAnyLeverStateChanged -= HandleLeverStateChanged;
        EchoButtonInteractable.OnAnyButtonPressed -= HandleButtonPressed;
        EchoRoomSettings.Changed -= OnSettingChanged;

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
        if (panelSettingsInstance != null) Destroy(panelSettingsInstance);
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
        if (panelDocument == null) return;
        if (handAnchor == null || head == null) ResolveTrackingTargets();
        if (handAnchor == null || head == null) return;

        Transform panel = panelDocument.transform;
        Vector3 offset = head.right * displayOffsetFromView.x +
                         head.up * displayOffsetFromView.y +
                         head.forward * displayOffsetFromView.z;
        panel.position = handAnchor.position + offset;
        // The extra 180 and the mirrored X are what world-space UI Toolkit needs to read
        // the right way round, matching the tutorial prompt's placement.
        panel.rotation = Quaternion.LookRotation(panel.position - head.position, head.up) *
                         Quaternion.Euler(0f, 180f, 0f);
        panel.localScale = new Vector3(-panelWorldScale, panelWorldScale, panelWorldScale);
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

        demoMode = false;
        puzzle = null;
        remainingSeconds = duration;
        timerRunning = false;
        revealUntilTime = 0f;
        automaticWarningMask = 0;
        StopWarningAudio();
        EnsureDisplay();
        UpdateDisplayText();
        RefreshObjective();
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

    /// <summary>
    /// Runs the real countdown and panel for the tutorial so "press A to check your timer"
    /// teaches something that actually exists. Expiry is neutered while this is active.
    /// </summary>
    public void BeginTutorialDemo(float seconds, string objectiveLine, int activated, int total)
    {
        demoMode = true;
        demoObjective = string.IsNullOrWhiteSpace(objectiveLine) ? ObjectiveFindLevers : objectiveLine;
        demoActivated = Mathf.Max(0, activated);
        demoTotal = Mathf.Max(0, total);
        puzzle = null;

        EnsureDisplay();
        remainingSeconds = Mathf.Max(1f, seconds);
        timerRunning = false;
        revealUntilTime = 0f;
        automaticWarningMask = 0;
        StopWarningAudio();
        UpdateDisplayText();
        RefreshObjective();
        SetDisplayAlpha(0f);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(StartTimerWhenTransitionComplete());
    }

    /// <summary>Advances the tutorial's demo objective count and flashes the panel.</summary>
    public void SetTutorialProgress(int activated)
    {
        if (!demoMode) return;
        demoActivated = Mathf.Clamp(activated, 0, demoTotal);
        RefreshObjective();
        RevealFor(objectiveFlashSeconds, Time.time);
    }

    public void EndTutorialDemo()
    {
        if (!demoMode) return;
        demoMode = false;
        StopTimer();
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

        // The tutorial's demo countdown is a lesson, not a fail state.
        if (demoMode)
        {
            targetAlpha = 0f;
            SetDisplayAlpha(0f);
            return;
        }

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

    // ---------- Objective ----------

    void HandleLeverStateChanged(LeverInteractable lever)
    {
        FlashObjectiveProgress();
    }

    void HandleButtonPressed(EchoButtonInteractable button)
    {
        FlashObjectiveProgress();
    }

    /// <summary>
    /// A lever moved: refresh the count and auto-reveal the panel briefly so the player feels
    /// the progress without having to remember to peek.
    /// </summary>
    void FlashObjectiveProgress()
    {
        if (!timerRunning) return;
        RefreshObjective();
        RevealFor(objectiveFlashSeconds, Time.time);
    }

    void ResolvePuzzle()
    {
        if (puzzle != null) return;
        GameObject level = gameManager != null ? gameManager.CurrentLevelInstance : null;
        if (level != null) puzzle = level.GetComponentInChildren<EchoPuzzleController>(true);
    }

    void RefreshObjective()
    {
        if (objectiveLabel == null || progressLabel == null) return;

        int activated;
        int total;
        string goal;

        if (demoMode)
        {
            activated = demoActivated;
            total = demoTotal;
            goal = demoObjective;
        }
        else
        {
            ResolvePuzzle();
            total = puzzle != null ? puzzle.TargetCount : 0;
            activated = puzzle != null ? puzzle.ActivatedCount : 0;
            goal = ObjectiveFindLevers;
        }

        bool complete = total > 0 && activated >= total;
        if (complete) goal = ObjectiveAllLeversActive;

        objectiveLabel.text = goal;
        progressLabel.text = total > 0 ? activated + " / " + total : string.Empty;
        progressLabel.style.display = total > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        if (panelRoot != null) panelRoot.EnableInClassList("complete", complete);
    }

    // ---------- Display ----------

    void FadeDisplay(float presentationDeltaTime)
    {
        if (panelRoot == null) return;
        float duration = displayAlpha < targetAlpha ? fadeInSeconds : fadeOutSeconds;
        float speed = duration <= 0.001f ? 1000f : 1f / duration;
        SetDisplayAlpha(Mathf.MoveTowards(displayAlpha, targetAlpha,
            Mathf.Max(0f, presentationDeltaTime) * speed));
    }

    void UpdateDisplayText()
    {
        if (timerLabel == null) return;
        int totalSeconds = Mathf.CeilToInt(remainingSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerLabel.text = $"TIME  {minutes:00}:{seconds:00}";

        if (panelRoot != null)
            panelRoot.EnableInClassList("low-time", remainingSeconds <= lowTimeThresholdSeconds);
    }

    void EnsureDisplay()
    {
        if (panelDocument != null) return;
        ResolveTrackingTargets();

        VisualTreeAsset layout = Resources.Load<VisualTreeAsset>(PanelLayoutResource);
        StyleSheet styles = Resources.Load<StyleSheet>(PanelStylesResource);
        PanelSettings sharedSettings = Resources.Load<PanelSettings>(PanelSettingsResource);

        if (layout == null || styles == null || sharedSettings == null)
        {
            Debug.LogError("[MazeLevelTimer] Objective panel assets are missing. Expected Resources/" +
                           PanelLayoutResource + ".uxml, Resources/" + PanelStylesResource +
                           ".uss, and Resources/" + PanelSettingsResource + ".asset.", this);
            return;
        }

        GameObject panelObject = new GameObject("Maze Objective Panel");
        EchoRoom.UI.UITKOverlayLayer.Apply(panelObject, false);
        panelObject.SetActive(false);
        panelObject.transform.SetParent(transform, false);

        // A private PanelSettings copy. Sharing the menu asset would merge this document with
        // the tutorial prompt's, and both are on screen at once during the tutorial.
        panelSettingsInstance = Instantiate(sharedSettings);
        panelSettingsInstance.name = sharedSettings.name + " (Objective Panel)";

        panelDocument = panelObject.AddComponent<UIDocument>();
        panelDocument.panelSettings = panelSettingsInstance;
        panelDocument.visualTreeAsset = layout;
        panelDocument.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
        panelDocument.worldSpaceSize = PanelSize;
        panelDocument.pivot = Pivot.Center;
        panelDocument.position = Position.Absolute;
        panelDocument.sortingOrder = short.MaxValue;
        panelObject.transform.localScale = new Vector3(-panelWorldScale, panelWorldScale, panelWorldScale);
        panelObject.SetActive(true);

        VisualElement documentRoot = panelDocument.rootVisualElement;
        if (!documentRoot.styleSheets.Contains(styles)) documentRoot.styleSheets.Add(styles);
        documentRoot.pickingMode = PickingMode.Ignore;
        documentRoot.Query<VisualElement>().ForEach(element => element.pickingMode = PickingMode.Ignore);

        panelRoot = documentRoot.Q<VisualElement>("objective-root");
        timerLabel = documentRoot.Q<Label>("objective-timer");
        objectiveLabel = documentRoot.Q<Label>("objective-text");
        progressLabel = documentRoot.Q<Label>("objective-progress");

        if (panelRoot == null || timerLabel == null || objectiveLabel == null || progressLabel == null)
        {
            Debug.LogError("[MazeLevelTimer] VRObjectivePanel.uxml is missing the root, timer, " +
                           "objective, or progress element.", this);
            Destroy(panelObject);
            panelDocument = null;
            panelRoot = null;
            timerLabel = null;
            objectiveLabel = null;
            progressLabel = null;
            return;
        }

        if (warningAudioSource == null)
        {
            warningAudioSource = panelObject.AddComponent<AudioSource>();
            warningAudioSource.playOnAwake = false;
            warningAudioSource.loop = false;
            warningAudioSource.spatialBlend = 0f;
            warningAudioSource.dopplerLevel = 0f;
        }

        UpdateDisplayText();
        RefreshObjective();
        SetDisplayAlpha(0f);
    }

    void ResolveTrackingTargets()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;
        if (handAnchor != null) return;

        handAnchor = transform;
        if (handAnchorMissingLogged) return;
        handAnchorMissingLogged = true;
        Debug.LogError("[MazeLevelTimer] No controller transform is assigned, so the readout " +
                       "falls back to '" + name + "' and will not follow the hand.", this);
    }

    /// <summary>
    /// Re-points the wrist panel at the controller handedness put in play. Called by
    /// HandednessController rather than serialized, because the answer changes at runtime.
    /// </summary>
    public void SetHandAnchor(Transform anchor)
    {
        if (anchor == null) return;
        handAnchor = anchor;
        handAnchorMissingLogged = false;
    }

    void SetDisplayAlpha(float alpha)
    {
        displayAlpha = Mathf.Clamp01(alpha);
        if (panelRoot == null) return;
        panelRoot.style.opacity = displayAlpha;
        panelRoot.style.display = displayAlpha > 0.001f ? DisplayStyle.Flex : DisplayStyle.None;
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
        // A in two-handed play; B/Y on the single controller, because A/X is the sonar there.
        revealAction.AddBinding(HandedInput.ObjectivePanelPath);
        revealAction.AddBinding("<Gamepad>/buttonSouth");
        revealAction.AddBinding("<Keyboard>/t");
#endif
    }

    void RebuildRevealAction()
    {
#if ENABLE_INPUT_SYSTEM
        bool wasEnabled = revealAction != null && revealAction.enabled;
        revealAction?.Disable();
        revealAction?.Dispose();
        revealAction = null;
        CreateRevealAction();
        if (wasEnabled) revealAction?.Enable();
#endif
    }

    void OnSettingChanged(EchoRoomSetting setting)
    {
        if (setting == EchoRoomSetting.Handedness) RebuildRevealAction();
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
