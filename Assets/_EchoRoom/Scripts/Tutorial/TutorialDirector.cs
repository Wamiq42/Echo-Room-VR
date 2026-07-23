using System.Collections;
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
            new GameObject("Tutorial First Run Prompt").AddComponent<TutorialMenuPrompt>();
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

    static Button MakeButton(string label, System.Action action)
    {
        var button = new Button(action) { text = label };
        button.style.width = 390; button.style.height = 66; button.style.marginTop = 9;
        button.style.fontSize = 27; button.style.color = Color.white;
        button.style.backgroundColor = new Color(0.08f, 0.28f, 0.36f, 1f);
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

    enum Step { Ping, SecondReveal, Move, Button, Lever, Ending, Done }
    const float WarningHoldDuration = 2f;
    const float TextFadeDuration = 0.65f;
    const float WarningFadeDuration = 2.5f;
    const float ScreenFadeDuration = 5f;
    const float EntitySoundFadeInDuration = 1.5f;
    const float EntitySoundFadeOutDuration = 1.75f;

    const string SonarMessage = "SONAR\nPress the SONAR button to reveal the corridor.";
    const string MicrophoneMessage = "MICROPHONE\nHold Y and speak for a stronger microphone ping.";
    const string DirectionMessage = "MOVE\nIn timed mazes, press A to reveal the timer.\nGo straight, then turn left.";
    const string InteractionMessage = "INTERACTION\nMove close, aim at the button or lever, and press RIGHT TRIGGER.";
    const string WarningMessage = "WARNING\nSonar can attract unwanted attention.\nSomething dangerous may be listening.";

    static readonly Vector3 PromptOffsetFromView = new Vector3(0f, -0.10f, 0.85f);
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
        StopAllCoroutines();
        SetPromptPresentation(false, 0f);
    }

    void Update()
    {
        if (!activeTutorial || player == null) return;

        if (step == Step.Move && interactionWall != null && HorizontalDistance(player.position, interactionWall.position) <= 5f)
        {
            step = Step.Button;
            buttonActivated = false;
            leverActivated = false;
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

        Transform prompt = tutorialPromptDocument.transform;
        Vector3 offset = head.right * PromptOffsetFromView.x +
                         head.up * PromptOffsetFromView.y +
                         head.forward * PromptOffsetFromView.z;
        prompt.position = head.position + offset;
        prompt.rotation = Quaternion.LookRotation(prompt.position - head.position, head.up) *
                          Quaternion.Euler(0f, 180f, 0f);
        prompt.localScale = new Vector3(-PromptWorldScale, PromptWorldScale, PromptWorldScale);
        return true;
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
        if (step == Step.Ping)
        {
            if (pingEmitter != null && pingEmitter.LastPingInputSource != PingEmitter.PingInputSource.SonarControl)
                return;

            step = Step.SecondReveal;
            StartCoroutine(SwapPrompt(MicrophoneMessage));
        }
        else if (step == Step.SecondReveal)
        {
            step = Step.Move;
            StartCoroutine(SwapPrompt(DirectionMessage));
        }

    }

    void OnButton(EchoButtonInteractable button)
    {
        if ((step != Step.Button && step != Step.Lever) ||
            button == null || !button.transform.IsChildOf(levelRoot.transform)) return;

        buttonActivated = true;
        TryCompleteInteractionLesson();
    }

    void OnLever(LeverInteractable lever)
    {
        if ((step != Step.Button && step != Step.Lever) ||
            lever == null || !lever.transform.IsChildOf(levelRoot.transform)) return;

        leverActivated = true;
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

    IEnumerator EndAfterInteraction()
    {
        yield return SwapPrompt(WarningMessage);
        yield return new WaitForSecondsRealtime(WarningHoldDuration);
        yield return PlayEnding();
    }

    IEnumerator PlayEnding()
    {
        activeTutorial = false;
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
        ResolveHead();
        Camera camera = head != null ? head.GetComponent<Camera>() : null;
        if (camera == null)
        {
            Debug.LogError("[Tutorial] Main Camera is missing; the ending fade cannot be rendered safely in stereo.");
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
        return image;
    }

    static GameObject FindSceneObject(string name)
    {
        foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            if (candidate != null && candidate.scene.IsValid() && candidate.name == name) return candidate;
        return null;
    }
}

