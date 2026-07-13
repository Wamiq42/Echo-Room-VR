using System.Collections;
using TMPro;
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
    enum Step { Ping, SecondReveal, Move, Button, Lever, ReadWarning, Ending, Done }
    const float WarningReadDistance = 3f;
    const float WarningReadDuration = 2f;
    const float TextFadeDuration = 0.65f;
    const float WarningFadeDuration = 2.5f;
    const float ScreenFadeDuration = 5f;
    const float EntitySoundFadeInDuration = 1.5f;
    const float EntitySoundFadeOutDuration = 1.75f;

    Step step;
    GameObject levelRoot;
    Transform player, startCheckpoint, interactionWall, warningWall;
    PingEmitter pingEmitter;
    AudioSource entityAudio, heartbeatAudio;
    TextMeshPro sonarText, microphoneText, directionText, interactionText, warningText;
    float warningReadTimer;
    bool buttonActivated, leverActivated;
    bool activeTutorial;

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
    }

    void Update()
    {
        if (!activeTutorial || player == null) return;

        if (step == Step.Move && interactionWall != null && HorizontalDistance(player.position, interactionWall.position) <= 5f)
        {
            step = Step.Button;
            buttonActivated = false;
            leverActivated = false;
            StartCoroutine(SwapWallText(directionText, interactionText));
        }
        else if (step == Step.ReadWarning)
        {
            UpdateWarningReading();
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
        BuildWallInstructions();
        Darken();

        step = Step.Ping;
        ShowWallText(sonarText);
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

    void BuildWallInstructions()
    {
        interactionWall = levelRoot.transform.Find("Interaction Wall");
        warningWall = levelRoot.transform.Find("Entity wall");

        sonarText = FindPrefabText("Sonar Wall Tip");
        microphoneText = FindPrefabText("Microphone Wall Tip");
        directionText = FindPrefabText("Direction Wall Tip");
        interactionText = FindPrefabText("Interaction Wall Tip");
        warningText = FindPrefabText("Entity Wall Tip");

        SetTextVisible(sonarText, false);
        SetTextVisible(microphoneText, false);
        SetTextVisible(directionText, false);
        SetTextVisible(interactionText, false);
        SetTextVisible(warningText, false);
    }

    TextMeshPro FindPrefabText(string objectName)
    {
        Transform target = levelRoot.transform.Find(objectName);
        if (target == null)
        {
            Debug.LogError("[Tutorial] Prefab text is missing: " + objectName);
            return null;
        }

        TextMeshPro text = target.GetComponent<TextMeshPro>();
        if (text == null)
            Debug.LogError("[Tutorial] " + objectName + " has no TextMeshPro component.");
        return text;
    }

    static void ShowWallText(TextMeshPro text)
    {
        if (text == null) return;
        text.alpha = 1f;
        text.gameObject.SetActive(true);
    }

    static void SetTextVisible(TextMeshPro text, bool visible)
    {
        if (text == null) return;
        text.alpha = visible ? 1f : 0f;
        text.gameObject.SetActive(visible);
    }

    IEnumerator FadeOut(TextMeshPro text, float duration = TextFadeDuration)
    {
        if (text == null) yield break;

        float startAlpha = text.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        text.alpha = 0f;
        text.gameObject.SetActive(false);
    }

    IEnumerator SwapWallText(TextMeshPro outgoing, TextMeshPro incoming)
    {
        if (outgoing != null)
            yield return FadeOut(outgoing);

        if (incoming == null) yield break;

        incoming.alpha = 0f;
        incoming.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < TextFadeDuration)
        {
            elapsed += Time.deltaTime;
            incoming.alpha = Mathf.Lerp(0f, 1f, elapsed / TextFadeDuration);
            yield return null;
        }

        incoming.alpha = 1f;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void UpdateWarningReading()
    {
        if (warningWall == null) return;

        bool isReading = HorizontalDistance(player.position, warningWall.position) <= WarningReadDistance;
        warningReadTimer = isReading ? warningReadTimer + Time.deltaTime : 0f;
        if (warningReadTimer < WarningReadDuration) return;

        step = Step.Ending;
        StartCoroutine(PlayEnding());
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
            StartCoroutine(SwapWallText(sonarText, microphoneText));
        }
        else if (step == Step.SecondReveal)
        {
            step = Step.Move;
            StartCoroutine(SwapWallText(microphoneText, directionText));
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

        step = Step.ReadWarning;
        warningReadTimer = 0f;
        StartCoroutine(SwapWallText(interactionText, warningText));
    }

    IEnumerator PlayEnding()
    {
        activeTutorial = false;
        if (warningText != null)
            yield return FadeOut(warningText, WarningFadeDuration);

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

    static Image CreateFadeOverlay()
    {
        GameObject canvasObject = new GameObject("Tutorial Ending Fade", typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        Camera camera = Camera.main;
        if (camera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = Mathf.Max(camera.nearClipPlane + 0.05f, 0.1f);
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
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

