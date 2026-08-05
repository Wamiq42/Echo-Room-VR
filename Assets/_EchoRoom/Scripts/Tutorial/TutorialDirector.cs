using System.Collections;
using EchoRoom.Settings;
using EchoRoom.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;
using UnityEngine.UIElements;

public static class TutorialProgress
{
    const string Status = "EchoRoom.Tutorial.Status.v1";
    const string Request = "EchoRoom.Tutorial.Requested";
    public static bool ShouldOffer => PlayerPrefs.GetInt(Status, 0) == 0;
    public static bool IsTutorialRequested => PlayerPrefs.GetInt(Request, 0) == 1;
    public static void RequestPlay() { PlayerPrefs.SetInt(Request, 1); PlayerPrefs.Save(); }
    public static bool ConsumeRequest()
    {
        bool value = PlayerPrefs.GetInt(Request, 0) == 1;
        if (value) { PlayerPrefs.SetInt(Request, 0); PlayerPrefs.Save(); }
        return value;
    }
    public static void Skip() { PlayerPrefs.SetInt(Status, 1); PlayerPrefs.SetInt(Request, 0); PlayerPrefs.Save(); }
    public static void Complete() { PlayerPrefs.SetInt(Status, 2); PlayerPrefs.SetInt(Request, 0); PlayerPrefs.Save(); }
}

static class TutorialMenuBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            GameObject promptHost = new GameObject("Tutorial First Run Prompt");
            UITKOverlayLayer.Apply(promptHost, false);
            promptHost.AddComponent<TutorialMenuPrompt>();
        }
    }
}

sealed class TutorialMenuPrompt : MonoBehaviour
{
    VisualElement overlay;

    IEnumerator Start()
    {
        yield return null;
        var menu = FindObjectOfType<EchoRoom.UI.VRMainMenu>();
        var document = menu != null ? menu.GetComponent<UIDocument>() : null;
        if (document == null) yield break;
        VisualElement root = document.rootVisualElement;
        AddReplayButton(root);
        if (!TutorialProgress.ShouldOffer) yield break;

        while (!EchoRoom.UI.VRMainMenu.IsPrivacyPolicyAccepted)
            yield return null;


        overlay = new VisualElement { name = "tutorial-offer-screen" };
        overlay.style.position = Position.Absolute;
        overlay.style.left = overlay.style.right = overlay.style.top = overlay.style.bottom = 0;
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;
        overlay.style.backgroundColor = new Color(0.015f, 0.025f, 0.035f, 0.98f);
        overlay.style.paddingLeft = overlay.style.paddingRight = 70;

        var title = new Label("BEFORE YOU ENTER");
        title.style.fontSize = 52; title.style.color = new Color(0.35f, 0.9f, 1f);
        title.style.unityFontStyleAndWeight = FontStyle.Bold; title.style.marginBottom = 24;
        overlay.Add(title);

        var copy = new Label("Would you like to play the short tutorial first?\nLearn sonar, movement, interaction, and why every ping has a cost.");
        copy.style.fontSize = 29; copy.style.color = Color.white; copy.style.whiteSpace = WhiteSpace.Normal;
        copy.style.unityTextAlign = TextAnchor.MiddleCenter; copy.style.marginBottom = 34;
        overlay.Add(copy);
        overlay.Add(MakeButton("PLAY TUTORIAL", PlayTutorial));
        overlay.Add(MakeButton("SKIP FOR NOW", SkipTutorial));
        root.Add(overlay);
    }

    // The main menu buttons get their look and their :hover/:focus highlight from VRMenu.uss,
    // which VRMainMenu adds to the same rootVisualElement this overlay is parented to.
    // Inline styles outrank pseudo-class rules, so style through the stylesheet classes instead.
    static Button MakeButton(string label, System.Action action)
    {
        var button = new Button(action) { text = label };
        button.AddToClassList("menu-button");
        button.AddToClassList("main-menu-button");
        return button;
    }

    void AddReplayButton(VisualElement root)
    {
        var start = root.Q<VisualElement>("start-screen");
        if (start == null) return;
        var existing = root.Q<Button>("tutorial-replay-button");
        if (existing != null) return;
        var button = MakeButton("PLAY TUTORIAL", PlayTutorial);
        button.name = "tutorial-replay-button";
        start.Add(button);
    }

    void PlayTutorial()
    {
        EnsureInitialProgress();
        TutorialProgress.RequestPlay();
        var loading = FindObjectOfType<EchoRoom.UI.VRLoadingScreen>(true);
        if (loading != null) loading.LoadScene("MainScene", "ENTERING TUTORIAL");
        else SceneManager.LoadScene("MainScene");
    }

    static void EnsureInitialProgress()
    {
        if (PuzzleProgressSaveSystem.HasSaveFile) return;
        LevelData[] data = Resources.FindObjectsOfTypeAll<LevelData>();
        if (data.Length == 0 || data[0].levels == null || data[0].levels.Length == 0) return;
        Level first = data[0].levels[0];
        string id = first != null ? first.GetStableId(0) : "Puzzle_01";
        PuzzleProgressData progress = PuzzleProgressSaveSystem.CreateNew(id);
        progress.puzzleToLoad = id;
        PuzzleProgressSaveSystem.Save(progress);
    }

    void SkipTutorial()
    {
        TutorialProgress.Skip();
        if (overlay != null) overlay.RemoveFromHierarchy();
    }
}

[DisallowMultipleComponent]
public sealed class TutorialDirector : MonoBehaviour
{
    internal readonly struct PromptDiagnosticState
    {
        public readonly GameObject PromptObject;
        public readonly bool DocumentReady;
        public readonly bool RootReady;
        public readonly bool TextReady;
        public readonly bool Attached;
        public readonly bool Active;
        public readonly float Opacity;
        public readonly string Text;
        public readonly string ElementName;

        public bool Created => DocumentReady && RootReady && TextReady;

        public PromptDiagnosticState(GameObject promptObject, bool documentReady, bool rootReady,
            bool textReady, bool attached, bool active, float opacity, string text, string elementName)
        {
            PromptObject = promptObject;
            DocumentReady = documentReady;
            RootReady = rootReady;
            TextReady = textReady;
            Attached = attached;
            Active = active;
            Opacity = opacity;
            Text = text;
            ElementName = elementName;
        }
    }

    internal readonly struct RuntimeDiagnosticState
    {
        public readonly string Step;
        public readonly GameObject LevelRoot;
        public readonly Transform Player;
        public readonly Transform InteractionWall;
        public readonly bool ButtonActivated;
        public readonly bool LeverActivated;
        public readonly AudioSource EntityAudio;
        public readonly AudioSource HeartbeatAudio;
        public readonly PromptDiagnosticState Prompt;

        public RuntimeDiagnosticState(string step, GameObject levelRoot, Transform player,
            Transform interactionWall, bool buttonActivated, bool leverActivated,
            AudioSource entityAudio, AudioSource heartbeatAudio, PromptDiagnosticState prompt)
        {
            Step = step;
            LevelRoot = levelRoot;
            Player = player;
            InteractionWall = interactionWall;
            ButtonActivated = buttonActivated;
            LeverActivated = leverActivated;
            EntityAudio = entityAudio;
            HeartbeatAudio = heartbeatAudio;
            Prompt = prompt;
        }
    }

    enum Step { Ping, SecondReveal, Move, Button, Lever, Ending, Auditor, Done }
    const float WarningHoldDuration = 2f;
    const float TextFadeDuration = 0.65f;
    const float WarningFadeDuration = 2.5f;
    const float ScreenFadeDuration = 5f;
    const float EntitySoundFadeInDuration = 1.5f;
    const float EntitySoundFadeOutDuration = 1.75f;

    // Built from HandedInput rather than hard-coded, because the handedness setting remaps every
    // one of these. In left-hand mode the sonar is X, the objective panel is Y, the microphone is
    // the grip and the trigger is on the left -- a tutorial that still said "Press B" would be
    // teaching the player four things that are all false.
    static string SonarMessage =>
        $"SONAR\nPress {HandedInput.SonarPingLabel} to reveal the corridor.";
    static string MicrophoneMessage =>
        $"MICROPHONE\nHold {HandedInput.MicrophonePingLabel} and speak for a stronger microphone ping.";
    static string DirectionMessage =>
        $"OBJECTIVE\nPress {HandedInput.ObjectivePanelLabel} anytime to check your timer and goal.\n" +
        "Head straight, then turn left.";
    static string InteractionMessage =>
        $"INTERACTION\nMove close, aim at the button or lever, and press " +
        $"{(HandedInput.IsLeftActive ? "LEFT" : "RIGHT")} TRIGGER.";
    // Touch controllers only carry a Menu button on the left hand, so hold-to-pause is the only
    // pause a right-hand-only player has. Taught here so it is never discovered by accident.
    static string PauseMessage =>
        $"PAUSE\nHold {VRPauseMenu.HoldPauseButtonLabel} for a moment to open the menu.";
    const string WarningMessage = "WARNING\nSonar can attract unwanted attention.\nSomething dangerous may be listening.";

    // The Auditor showcase. This is deliberately NOT the live hunt: the entity stands still and is
    // explained, because a chase at the end of a tutorial teaches nothing and loses players.
    // It is revealed by the player's own ping so the lesson lands physically -- I made a sound,
    // and it was there -- which is also the game's whole premise.
    const string AuditorTurnMessage = "BEHIND YOU\nSomething is standing in the dark. Turn around.";
    static string AuditorPingMessage =>
        $"SONAR\nPress {HandedInput.SonarPingLabel}. See what is listening.";
    const string AuditorBeatOne = "THE AUDITOR\nFacility records list it as a monitoring unit. " +
                                  "Whatever it was built to listen for, it is still listening.";
    const string AuditorBeatTwo = "IT HUNTS SOUND\nEvery ping is a beacon. It walks to where your " +
                                  "sound was born, and searches the dark around it.";
    const string AuditorBeatThree = "IF IT FINDS YOU\nIt puts you back where you started. " +
                                    "Stand still. Stay silent. It cannot see you.";
    const string AuditorWallText = "LOOK BEHIND YOU";

    const float AuditorDarkBeatSeconds = 1.5f;
    const float AuditorTurnHoldSeconds = 2.25f;
    const float AuditorBeatSeconds = 5.5f;
    // No escape hatch on a forced-input gate at the end of a tutorial would be a softlock.
    const float AuditorPingTimeoutSeconds = 15f;
    const float PauseLessonDelaySeconds = 6f;
    const float PauseLessonHoldSeconds = 5f;

    // The tutorial runs a real countdown so "press A to check your timer" teaches something that
    // exists. It is long enough that it can never run out during the lesson.
    const float TutorialTimerSeconds = 360f;
    const string TutorialObjective = "ACTIVATE THE BUTTON AND THE LEVER";
    const int TutorialObjectiveTotal = 2;

    // Placement is built from a yaw-only head frame, never the full head rotation. Using
    // head.up/head.forward meant a player who happened to be looking down when a beat fired got
    // the panel pinned to the floor, and it stayed there -- placement only runs once per prompt.
    const float PromptDistance = 0.9f;
    const float PromptHeightOffset = -0.05f;   // ~3 degrees below the eye line at 0.9 m.
    const float PromptRecentreAngle = 35f;     // Deadzone: below this the panel stays world-locked.
    const float PromptRecentreDuration = 0.45f;
    const float PromptHeightTolerance = 0.15f; // Re-place if the player's eye height moves this far.
    const float PromptWorldScale = 0.0005f;
    static readonly Vector2 PromptPanelSize = new Vector2(700f, 260f);
    const string PromptLayoutResource = "UI/VRTutorialPrompt";
    const string PromptStylesResource = "UI/VRTutorialPromptStyles";
    const string PromptPanelSettingsResource = "UI/VRMenuPanelSettings";

    Step step;
    GameObject levelRoot;
    Transform player, startCheckpoint, interactionWall;
    Transform head;
    PingEmitter pingEmitter;
    MazeLevelTimer mazeTimer;
    AudioSource entityAudio, heartbeatAudio;
    UIDocument tutorialPromptDocument;
    VisualElement tutorialPromptRoot;
    Label tutorialPromptTitle;
    Label tutorialPromptBody;
    string currentTutorialPromptText = string.Empty;
    bool tutorialPromptPresented;
    float tutorialPromptOpacity;
    bool buttonActivated, leverActivated;
    bool activeTutorial;
    bool promptPlacementErrorLogged;
    Vector3 promptAnchorForward;
    Coroutine promptRecentreRoutine;
    Transform auditor;
    TextMesh auditorWallText;
    bool auditorPingReceived;
    Coroutine pauseLessonRoutine;

    void Awake()
    {
        activeTutorial = TutorialProgress.IsTutorialRequested;
        if (!activeTutorial) { enabled = false; return; }
    }

    IEnumerator Start()
    {
        while (GameManager.Instance == null || !GameManager.Instance.IsTutorialLevelActive || GameManager.Instance.CurrentLevelInstance == null)
            yield return null;
        TutorialProgress.ConsumeRequest();
        Setup();
    }

    void OnEnable()
    {
        EchoButtonInteractable.OnAnyButtonPressed += OnButton;
        LeverInteractable.OnAnyLeverTurnedOn += OnLever;
    }

    void OnDisable()
    {
        EchoButtonInteractable.OnAnyButtonPressed -= OnButton;
        LeverInteractable.OnAnyLeverTurnedOn -= OnLever;
        if (pingEmitter != null) pingEmitter.OnPingEmitted -= OnPing;
        if (mazeTimer != null) mazeTimer.EndTutorialDemo();
        StopAllCoroutines();
        // StopAllCoroutines does not clear our handle, and a stale non-null handle would block
        // UpdatePromptFollow forever after a re-enable.
        promptRecentreRoutine = null;
        pauseLessonRoutine = null;
        SetPromptPresentation(false, 0f);
    }

    void Update()
    {
        // Runs before the activeTutorial guard: the ending and Auditor beats still show prompts
        // after the interactive part of the tutorial is over, and those must follow too.
        UpdatePromptFollow();

        if (!activeTutorial || player == null) return;

        if (step == Step.Move && interactionWall != null && HorizontalDistance(player.position, interactionWall.position) <= 5f)
        {
            step = Step.Button;
            buttonActivated = false;
            leverActivated = false;
            // Kill the pause lesson outright rather than trusting its step check: if it is mid
            // SwapPrompt right now, both coroutines would be cross-fading the same panel.
            if (pauseLessonRoutine != null)
            {
                StopCoroutine(pauseLessonRoutine);
                pauseLessonRoutine = null;
            }
            StartCoroutine(SwapPrompt(InteractionMessage));
        }
    }

    void Setup()
    {
        levelRoot = GameManager.Instance != null ? GameManager.Instance.CurrentLevelInstance : null;
        if (levelRoot == null) { Debug.LogError("[Tutorial] T-junction not found."); return; }

        DisableOtherLevels();
        levelRoot.SetActive(true);
        player = FindSceneObject("XR Origin (XR Rig)")?.transform;
        startCheckpoint = levelRoot.transform.Find("StartCheckpoint");
        if (startCheckpoint != null) MovePlayer(startCheckpoint.position, startCheckpoint.rotation);
        else Debug.LogError("[Tutorial] StartCheckpoint is missing from the T-junction prefab.");

        pingEmitter = FindObjectOfType<PingEmitter>();
        if (pingEmitter != null) pingEmitter.OnPingEmitted += OnPing;

        BuildInteractionLesson();
        BuildEndingLesson();
        BuildAuditorLesson();
        BuildTutorialPrompt();
        Darken();

        step = Step.Ping;
        ShowPrompt(SonarMessage);
    }

    void DisableOtherLevels()
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root != levelRoot && (root.name.StartsWith("Maze_5x5") || root.name == "Level - 1 (echo puzzle)"))
                root.SetActive(false);
    }

    void MovePlayer(Vector3 position, Quaternion rotation)
    {
        if (player == null) return;
        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        player.SetPositionAndRotation(position, rotation);
        if (controller != null) controller.enabled = true;
    }

    void BuildInteractionLesson()
    {
        Transform button = levelRoot.transform.Find("Tutorial Button");
        Transform lever = levelRoot.transform.Find("Tutorial Lever");

        if (button != null) button.gameObject.SetActive(true);
        else Debug.LogError("[Tutorial] Tutorial Button is missing from the T-junction prefab.");

        if (lever != null) lever.gameObject.SetActive(true);
        else Debug.LogError("[Tutorial] Tutorial Lever is missing from the T-junction prefab.");
    }

    void BuildEndingLesson()
    {
        Transform endingAudio = levelRoot.transform.Find("Tutorial Ending Audio");
        if (endingAudio == null)
        {
            Debug.LogError("[Tutorial] Tutorial Ending Audio is missing from the T-junction prefab.");
            return;
        }

        Transform entitySound = endingAudio.Find("Entity Sound");
        Transform heartbeat = endingAudio.Find("Heartbeat");
        entityAudio = entitySound != null ? entitySound.GetComponent<AudioSource>() : null;
        heartbeatAudio = heartbeat != null ? heartbeat.GetComponent<AudioSource>() : null;

        if (entityAudio == null) Debug.LogError("[Tutorial] Ending Entity Sound AudioSource is missing.");
        if (heartbeatAudio == null) Debug.LogError("[Tutorial] Ending Heartbeat AudioSource is missing.");
    }

    void BuildTutorialPrompt()
    {
        interactionWall = levelRoot.transform.Find("Interaction Wall");
        ResolveHead();

        CreateTutorialPrompt();
    }

    void CreateTutorialPrompt()
    {
        VisualTreeAsset layout = Resources.Load<VisualTreeAsset>(PromptLayoutResource);
        StyleSheet styles = Resources.Load<StyleSheet>(PromptStylesResource);
        PanelSettings panelSettings = Resources.Load<PanelSettings>(PromptPanelSettingsResource);

        if (layout == null || styles == null || panelSettings == null)
        {
            Debug.LogError("[Tutorial] UI Toolkit prompt assets are missing. Expected Resources/" +
                           PromptLayoutResource + ".uxml, Resources/" + PromptStylesResource +
                           ".uss, and Resources/" + PromptPanelSettingsResource + ".asset.");
            return;
        }

        GameObject promptObject = new GameObject("Tutorial UI Toolkit Prompt");
        UITKOverlayLayer.Apply(promptObject, false);
        promptObject.SetActive(false);
        promptObject.transform.SetParent(transform, false);

        tutorialPromptDocument = promptObject.AddComponent<UIDocument>();
        tutorialPromptDocument.panelSettings = panelSettings;
        tutorialPromptDocument.visualTreeAsset = layout;
        tutorialPromptDocument.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
        tutorialPromptDocument.worldSpaceSize = PromptPanelSize;
        tutorialPromptDocument.pivot = Pivot.Center;
        tutorialPromptDocument.position = Position.Absolute;
        tutorialPromptDocument.sortingOrder = short.MaxValue;
        promptObject.transform.localScale = new Vector3(-PromptWorldScale, PromptWorldScale, PromptWorldScale);
        promptObject.SetActive(true);

        VisualElement documentRoot = tutorialPromptDocument.rootVisualElement;
        documentRoot.pickingMode = PickingMode.Ignore;
        documentRoot.Query<VisualElement>().ForEach(element => element.pickingMode = PickingMode.Ignore);

        tutorialPromptRoot = documentRoot.Q<VisualElement>("tutorial-prompt-root");
        tutorialPromptTitle = documentRoot.Q<Label>("tutorial-prompt-title");
        tutorialPromptBody = documentRoot.Q<Label>("tutorial-prompt-body");
        if (tutorialPromptRoot == null || tutorialPromptTitle == null || tutorialPromptBody == null)
        {
            Debug.LogError("[Tutorial] VRTutorialPrompt.uxml is missing the prompt root, title, " +
                           "or body element.");
            Destroy(promptObject);
            tutorialPromptDocument = null;
            tutorialPromptRoot = null;
            tutorialPromptTitle = null;
            tutorialPromptBody = null;
            return;
        }

        SetPromptPresentation(false, 0f);
    }

    void ResolveHead()
    {
        if (head != null) return;
        head = Camera.main != null ? Camera.main.transform : null;
        if (head == null && player != null)
            head = player.Find("Camera Offset/Main Camera");
    }

    /// <summary>
    /// The player's facing direction flattened to horizontal. Falls back to the last good value
    /// when the player looks straight up or down, where the projection collapses to zero.
    /// </summary>
    Vector3 HeadForwardFlat()
    {
        Vector3 flat = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (flat.sqrMagnitude < 0.0001f)
            flat = promptAnchorForward.sqrMagnitude > 0.0001f ? promptAnchorForward : Vector3.forward;
        return flat.normalized;
    }

    Vector3 PromptPositionFor(Vector3 forwardFlat) =>
        head.position + forwardFlat * PromptDistance + Vector3.up * PromptHeightOffset;

    // World up, not head up, so the panel never inherits head roll. The trailing 180 and the
    // mirrored X scale are what world-space UI Toolkit needs to read the right way round.
    static Quaternion PromptRotationFor(Vector3 forwardFlat) =>
        Quaternion.LookRotation(forwardFlat, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);

    bool TryPlacePromptAtHeadPose()
    {
        ResolveHead();
        if (tutorialPromptDocument == null || head == null)
        {
            if (!promptPlacementErrorLogged)
            {
                Debug.LogError("[Tutorial] Main Camera is missing; the world-space prompt cannot be placed safely.");
                promptPlacementErrorLogged = true;
            }
            return false;
        }

        promptPlacementErrorLogged = false;

        if (promptRecentreRoutine != null)
        {
            StopCoroutine(promptRecentreRoutine);
            promptRecentreRoutine = null;
        }

        Vector3 forwardFlat = HeadForwardFlat();
        Transform prompt = tutorialPromptDocument.transform;
        prompt.position = PromptPositionFor(forwardFlat);
        prompt.rotation = PromptRotationFor(forwardFlat);
        prompt.localScale = new Vector3(-PromptWorldScale, PromptWorldScale, PromptWorldScale);
        promptAnchorForward = forwardFlat;
        return true;
    }

    /// <summary>
    /// Keeps the prompt reachable without making it swim. Inside the deadzone the panel stays
    /// world-locked, which is what stops it inducing nausea; past it, the panel eases round to
    /// the new facing so a prompt can never be stranded behind the player.
    /// </summary>
    void UpdatePromptFollow()
    {
        if (!tutorialPromptPresented || tutorialPromptDocument == null) return;
        if (promptRecentreRoutine != null) return;
        ResolveHead();
        if (head == null) return;

        Vector3 forwardFlat = HeadForwardFlat();
        bool turnedAway = Vector3.Angle(promptAnchorForward, forwardFlat) >= PromptRecentreAngle;

        // Height drift matters as much as yaw here. The very first prompt is placed from Setup(),
        // which can run before XR tracking has reported a real eye height -- so panel one gets
        // pinned near the rig origin, at the floor. This also covers a player who stands up,
        // sits down, or recentres their guardian part-way through the tutorial.
        float targetHeight = head.position.y + PromptHeightOffset;
        bool heightDrifted =
            Mathf.Abs(tutorialPromptDocument.transform.position.y - targetHeight) > PromptHeightTolerance;

        if (!turnedAway && !heightDrifted) return;

        promptRecentreRoutine = StartCoroutine(RecentrePrompt());
    }

    IEnumerator RecentrePrompt()
    {
        Transform prompt = tutorialPromptDocument.transform;
        Vector3 startPosition = prompt.position;
        Quaternion startRotation = prompt.rotation;
        float elapsed = 0f;

        while (elapsed < PromptRecentreDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            // Re-read the target every frame: the player is usually still turning while this runs.
            Vector3 forwardFlat = HeadForwardFlat();
            float t = Mathf.SmoothStep(0f, 1f, elapsed / PromptRecentreDuration);
            prompt.position = Vector3.Lerp(startPosition, PromptPositionFor(forwardFlat), t);
            prompt.rotation = Quaternion.Slerp(startRotation, PromptRotationFor(forwardFlat), t);
            promptAnchorForward = forwardFlat;
            yield return null;
        }

        Vector3 settled = HeadForwardFlat();
        prompt.position = PromptPositionFor(settled);
        prompt.rotation = PromptRotationFor(settled);
        promptAnchorForward = settled;
        promptRecentreRoutine = null;
    }

    void ShowPrompt(string message)
    {
        if (!SetPromptCopy(message)) return;
        if (!TryPlacePromptAtHeadPose())
        {
            SetPromptPresentation(false, 0f);
            return;
        }
        SetPromptPresentation(true, 1f);
    }

    IEnumerator FadePrompt(float targetAlpha, float duration)
    {
        if (tutorialPromptRoot == null) yield break;

        float startAlpha = tutorialPromptOpacity;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetPromptPresentation(true, Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }

        SetPromptPresentation(targetAlpha > 0f, targetAlpha);
    }

    IEnumerator SwapPrompt(string incoming)
    {
        if (tutorialPromptRoot == null || tutorialPromptTitle == null || tutorialPromptBody == null) yield break;

        if (tutorialPromptPresented)
            yield return FadePrompt(0f, TextFadeDuration);

        if (!SetPromptCopy(incoming)) yield break;
        if (!TryPlacePromptAtHeadPose())
        {
            SetPromptPresentation(false, 0f);
            yield break;
        }
        SetPromptPresentation(true, 0f);
        yield return FadePrompt(1f, TextFadeDuration);
    }

    bool SetPromptCopy(string message)
    {
        if (tutorialPromptRoot == null || tutorialPromptTitle == null || tutorialPromptBody == null) return false;

        currentTutorialPromptText = message ?? string.Empty;
        int lineBreak = currentTutorialPromptText.IndexOf('\n');
        string title = lineBreak >= 0 ? currentTutorialPromptText.Substring(0, lineBreak) : currentTutorialPromptText;
        string body = lineBreak >= 0 ? currentTutorialPromptText.Substring(lineBreak + 1) : string.Empty;
        tutorialPromptTitle.text = title;
        tutorialPromptBody.text = body;
        tutorialPromptBody.EnableInClassList("compact-copy", body.Length > 60 || body.IndexOf('\n') >= 0);
        bool warning = title == "WARNING";
        tutorialPromptRoot.EnableInClassList("warning-state", warning);
        tutorialPromptTitle.EnableInClassList("warning-text", warning);
        // The three dossier beats read as records, not alarms, so they get their own amber state.
        bool auditorBeat = message == AuditorBeatOne || message == AuditorBeatTwo ||
                           message == AuditorBeatThree;
        tutorialPromptRoot.EnableInClassList("auditor-state", auditorBeat);
        return true;
    }

    void SetPromptPresentation(bool presented, float opacity)
    {
        tutorialPromptPresented = presented;
        tutorialPromptOpacity = Mathf.Clamp01(opacity);
        if (tutorialPromptRoot == null) return;
        tutorialPromptRoot.style.display = presented ? DisplayStyle.Flex : DisplayStyle.None;
        tutorialPromptRoot.style.opacity = tutorialPromptOpacity;
    }

    internal bool TryGetRuntimeDiagnosticState(out RuntimeDiagnosticState state)
    {
        bool documentReady = tutorialPromptDocument != null;
        bool rootReady = tutorialPromptRoot != null;
        bool textReady = tutorialPromptTitle != null && tutorialPromptBody != null;
        bool attached = rootReady && tutorialPromptRoot.panel != null;
        GameObject promptObject = documentReady ? tutorialPromptDocument.gameObject : null;
        bool active = tutorialPromptPresented && documentReady && tutorialPromptDocument.enabled &&
                      promptObject.activeInHierarchy && attached;
        var prompt = new PromptDiagnosticState(promptObject, documentReady, rootReady, textReady,
            attached, active, tutorialPromptOpacity, currentTutorialPromptText,
            textReady ? tutorialPromptBody.name : "NULL");
        state = new RuntimeDiagnosticState(step.ToString(), levelRoot, player, interactionWall,
            buttonActivated, leverActivated, entityAudio, heartbeatAudio, prompt);
        return levelRoot != null && player != null;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }

    static void Darken()
    {
        RenderSettings.ambientLight = Color.black;
        foreach (var light in FindObjectsOfType<Light>(false))
            if (light.type == LightType.Directional) light.enabled = false;
    }

    void OnPing(Vector3 origin)
    {
        if (step == Step.Auditor)
        {
            auditorPingReceived = true;
            return;
        }

        if (step == Step.Ping)
        {
            if (pingEmitter != null && pingEmitter.LastPingInputSource != PingEmitter.PingInputSource.SonarControl)
                return;

            step = Step.SecondReveal;
            StartCoroutine(SwapPrompt(MicrophoneMessage));
        }
        else if (step == Step.SecondReveal)
        {
            if (pingEmitter != null && pingEmitter.LastPingInputSource != PingEmitter.PingInputSource.Microphone)
                return;

            step = Step.Move;
            pauseLessonRoutine = StartCoroutine(TeachPauseDuringWalk());

            // The microphone ping that got us here just armed the longer shared lockout, and the
            // sonar and microphone share it. This lesson then asks the player to walk a dark
            // corridor, so without handing the sonar straight back their next B press is silently
            // rejected -- no sound, no haptic, no reveal -- and the lesson reads as broken.
            if (pingEmitter != null) pingEmitter.ResetCooldown();

            StartTutorialTimerDemo();
            StartCoroutine(SwapPrompt(DirectionMessage));
        }

    }

    /// <summary>
    /// Slides the pause lesson in while the player is walking the corridor, then hands the screen
    /// back to the direction prompt. Deliberately not gated on the player performing it: opening
    /// the pause menu mid-tutorial stops time and throws away the moment, and this is a binding
    /// worth knowing rather than a skill worth drilling.
    /// </summary>
    IEnumerator TeachPauseDuringWalk()
    {
        yield return new WaitForSecondsRealtime(PauseLessonDelaySeconds);
        if (step != Step.Move) yield break;

        yield return SwapPrompt(PauseMessage);
        yield return new WaitForSecondsRealtime(PauseLessonHoldSeconds);

        // The player may have reached the interaction wall while this was on screen; in that case
        // Update has already moved us on and swapped in the interaction copy. Do not stomp it.
        if (step != Step.Move) yield break;
        yield return SwapPrompt(DirectionMessage);
        pauseLessonRoutine = null;
    }

    void StartTutorialTimerDemo()
    {
        if (mazeTimer == null) mazeTimer = FindObjectOfType<MazeLevelTimer>();
        if (mazeTimer == null)
        {
            Debug.LogError("[Tutorial] No MazeLevelTimer was found, so the timer and objective " +
                           "lesson cannot be shown.");
            return;
        }

        mazeTimer.BeginTutorialDemo(TutorialTimerSeconds, TutorialObjective, 0, TutorialObjectiveTotal);
    }

    void UpdateTutorialObjective()
    {
        if (mazeTimer == null) return;
        mazeTimer.SetTutorialProgress((buttonActivated ? 1 : 0) + (leverActivated ? 1 : 0));
    }

    void OnButton(EchoButtonInteractable button)
    {
        if ((step != Step.Button && step != Step.Lever) ||
            button == null || !button.transform.IsChildOf(levelRoot.transform)) return;

        buttonActivated = true;
        UpdateTutorialObjective();
        TryCompleteInteractionLesson();
    }

    void OnLever(LeverInteractable lever)
    {
        if ((step != Step.Button && step != Step.Lever) ||
            lever == null || !lever.transform.IsChildOf(levelRoot.transform)) return;

        leverActivated = true;
        UpdateTutorialObjective();
        TryCompleteInteractionLesson();
    }

    void TryCompleteInteractionLesson()
    {
        if (!buttonActivated || !leverActivated)
        {
            step = Step.Lever;
            return;
        }

        step = Step.Ending;
        StartCoroutine(EndAfterInteraction());
    }

    /// <summary>
    /// Finds the Auditor and strips its live behaviour. This is a museum piece, not a threat:
    /// the tutorial level has no baked NavMesh for the agent to walk, and being hunted at the end
    /// of a tutorial teaches nothing. TutorialDirector drives it manually instead.
    /// </summary>
    void BuildAuditorLesson()
    {
        Transform entity = levelRoot.transform.Find("Tutorial Entity");
        if (entity == null)
        {
            Debug.LogError("[Tutorial] Tutorial Entity is missing from the T-junction prefab, so the " +
                           "Auditor showcase will be skipped.");
            return;
        }

        auditor = entity;

        // Destroyed, not disabled. Unity runs Awake on a component even when the component itself
        // is disabled, and PingAttractedEntity.Awake touches its NavMeshAgent -- on a level with no
        // baked NavMesh that means console spam at best. Order matters: PingAttractedEntity carries
        // [RequireComponent(NavMeshAgent)], so removing the agent first would be refused.
        var hunter = entity.GetComponent<PingAttractedEntity>();
        if (hunter != null) Destroy(hunter);
        var agent = entity.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) Destroy(agent);

        entity.gameObject.SetActive(false);
    }

    IEnumerator EndAfterInteraction()
    {
        yield return SwapPrompt(WarningMessage);
        yield return new WaitForSecondsRealtime(WarningHoldDuration);
        yield return RevealAuditor();
        yield return PlayEnding();
    }

    IEnumerator RevealAuditor()
    {
        step = Step.Auditor;
        // The showcase is optional. If the prefab wiring is missing the tutorial must still end
        // cleanly rather than stranding the player in the dark.
        if (auditor == null) yield break;

        if (tutorialPromptPresented) yield return FadePrompt(0f, TextFadeDuration);
        yield return new WaitForSecondsRealtime(AuditorDarkBeatSeconds);

        FaceAuditorAtPlayer();
        ShowAuditorWallText();

        yield return SwapPrompt(AuditorTurnMessage);
        yield return new WaitForSecondsRealtime(AuditorTurnHoldSeconds);

        // Hand the sonar back before gating on it. The lever work and the shared lockout can leave
        // the next B press silently rejected -- no sound, no reveal -- and the beat reads as broken.
        if (pingEmitter != null) pingEmitter.ResetCooldown();
        auditorPingReceived = false;
        yield return SwapPrompt(AuditorPingMessage);

        float waited = 0f;
        while (!auditorPingReceived && waited < AuditorPingTimeoutSeconds)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        auditor.gameObject.SetActive(true);
        if (entityAudio != null && !entityAudio.isPlaying) entityAudio.Play();

        yield return SwapPrompt(AuditorBeatOne);
        yield return new WaitForSecondsRealtime(AuditorBeatSeconds);
        yield return SwapPrompt(AuditorBeatTwo);
        yield return new WaitForSecondsRealtime(AuditorBeatSeconds);
        yield return SwapPrompt(AuditorBeatThree);
        yield return new WaitForSecondsRealtime(AuditorBeatSeconds);

        HideAuditorWallText();
    }

    /// <summary>
    /// Turns the Auditor to face the player. Its position is authored in the level prefab, in the
    /// middle of the T-junction: the intersection is the one stage this room has, and the player is
    /// always at the button and lever wall when the reveal fires, so the junction is already behind
    /// them. Computing a spot at runtime only added ways for it to end up somewhere odd.
    /// </summary>
    void FaceAuditorAtPlayer()
    {
        ResolveHead();
        if (head == null || auditor == null) return;

        Vector3 toPlayer = Vector3.ProjectOnPlane(head.position - auditor.position, Vector3.up);
        if (toPlayer.sqrMagnitude < 0.0001f) return;
        auditor.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
    }

    /// <summary>
    /// Stencils the turn-around line onto whatever surface the player is facing. Self-lit, so it
    /// reads on a black wall. Purely atmospheric -- the prompt panel carries the same instruction,
    /// so nothing breaks if there is no wall to write on.
    /// </summary>
    void ShowAuditorWallText()
    {
        ResolveHead();
        if (head == null) return;
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit, 12f, ~0,
                             QueryTriggerInteraction.Ignore))
            return;

        if (auditorWallText == null)
        {
            var textObject = new GameObject("Auditor Wall Text");
            UITKOverlayLayer.Apply(textObject, false);
            auditorWallText = textObject.AddComponent<TextMesh>();
            auditorWallText.anchor = TextAnchor.MiddleCenter;
            auditorWallText.alignment = TextAlignment.Center;
            auditorWallText.fontSize = 96;
            auditorWallText.characterSize = 0.04f;
            auditorWallText.color = new Color(1f, 0.69f, 0.23f, 1f);
        }

        auditorWallText.text = AuditorWallText;
        auditorWallText.transform.position = hit.point + hit.normal * 0.02f +
                                             Vector3.up * 0.25f;
        auditorWallText.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
        auditorWallText.gameObject.SetActive(true);
    }

    void HideAuditorWallText()
    {
        if (auditorWallText != null) auditorWallText.gameObject.SetActive(false);
    }

    IEnumerator PlayEnding()
    {
        activeTutorial = false;
        if (mazeTimer != null) mazeTimer.EndTutorialDemo();
        if (tutorialPromptPresented)
            yield return FadePrompt(0f, WarningFadeDuration);

        TutorialProgress.Complete();
        Image fadeImage = CreateFadeOverlay();

        float entityTarget = entityAudio != null ? entityAudio.volume : 0f;
        float heartbeatTarget = heartbeatAudio != null ? heartbeatAudio.volume : 0f;
        if (entityAudio != null)
        {
            entityAudio.volume = 0f;
            entityAudio.Play();
        }
        if (heartbeatAudio != null)
        {
            heartbeatAudio.volume = 0f;
            heartbeatAudio.Play();
        }

        float elapsed = 0f;
        while (elapsed < ScreenFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / ScreenFadeDuration);
            if (fadeImage != null)
                fadeImage.color = new Color(0f, 0f, 0f, Mathf.SmoothStep(0f, 1f, t));

            if (entityAudio != null)
            {
                if (elapsed < EntitySoundFadeInDuration)
                    entityAudio.volume = Mathf.Lerp(0f, entityTarget, elapsed / EntitySoundFadeInDuration);
                else if (elapsed > ScreenFadeDuration - EntitySoundFadeOutDuration)
                    entityAudio.volume = Mathf.Lerp(entityTarget, 0f,
                        (elapsed - (ScreenFadeDuration - EntitySoundFadeOutDuration)) / EntitySoundFadeOutDuration);
                else
                    entityAudio.volume = entityTarget;
            }

            if (heartbeatAudio != null)
            {
                float heartbeatEnvelope = t < 0.2f ? t / 0.2f : (t > 0.8f ? (1f - t) / 0.2f : 1f);
                heartbeatAudio.volume = heartbeatTarget * Mathf.Clamp01(heartbeatEnvelope);
            }

            yield return null;
        }

        if (entityAudio != null) entityAudio.Stop();
        if (heartbeatAudio != null) heartbeatAudio.Stop();
        step = Step.Done;
        SceneManager.LoadScene("MainMenuScene");
    }

    Image CreateFadeOverlay()
    {
        Camera camera = UITKOverlayLayer.FindRenderingCamera();
        if (camera == null)
        {
            Debug.LogError("[Tutorial] UITK overlay camera is missing; the ending fade cannot be rendered safely in stereo.");
            return null;
        }

        GameObject canvasObject = new GameObject("Tutorial Ending Fade", typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = Mathf.Max(camera.nearClipPlane + 0.05f, 0.1f);
        canvas.sortingOrder = short.MaxValue;

        GameObject imageObject = new GameObject("Black Fade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = true;
        UITKOverlayLayer.Apply(canvasObject);
        return image;
    }

    static GameObject FindSceneObject(string name)
    {
        foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            if (candidate != null && candidate.scene.IsValid() && candidate.name == name) return candidate;
        return null;
    }
}

