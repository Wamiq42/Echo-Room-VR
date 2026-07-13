using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public sealed class TutorialRuntimeObserver : MonoBehaviour
{
    TutorialDirector director;
    GameObject levelRoot;
    Transform player;
    Transform interactionWall;
    Transform warningWall;
    AudioSource entityAudio;
    AudioSource heartbeatAudio;
    PingEmitter pingEmitter;

    FieldInfo stepField;
    FieldInfo readTimerField;
    FieldInfo buttonActivatedField;
    FieldInfo leverActivatedField;

    string lastStep;
    string lastSonarText;
    string lastInteractionText;
    string lastWarningText;
    bool? lastEntityAudioPlaying;
    bool? lastHeartbeatPlaying;
    float nextHeartbeat;
    bool attached;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (FindObjectOfType<TutorialRuntimeObserver>() != null) return;
        GameObject observerObject = new GameObject("Tutorial Runtime Observer");
        DontDestroyOnLoad(observerObject);
        observerObject.AddComponent<TutorialRuntimeObserver>();
    }

    IEnumerator Start()
    {
        while (true)
        {
            if (attached && director == null)
            {
                TutorialRuntimeLogger.Event("DETACH", "Tutorial scene/director was destroyed.");
                Detach();
            }

            if (!attached)
            {
                TutorialDirector candidate = FindObjectOfType<TutorialDirector>();
                if (candidate != null && candidate.enabled)
                {
                    System.Type type = typeof(TutorialDirector);
                    FieldInfo rootField = type.GetField("levelRoot", BindingFlags.Instance | BindingFlags.NonPublic);
                    FieldInfo playerField = type.GetField("player", BindingFlags.Instance | BindingFlags.NonPublic);
                    bool ready = rootField != null && rootField.GetValue(candidate) != null &&
                                 playerField != null && playerField.GetValue(candidate) != null;
                    if (ready) Attach(candidate);
                }
            }

            yield return null;
        }
    }

    void Attach(TutorialDirector target)
    {
        attached = true;
        director = target;
        System.Type type = typeof(TutorialDirector);
        stepField = type.GetField("step", BindingFlags.Instance | BindingFlags.NonPublic);
        readTimerField = type.GetField("warningReadTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        buttonActivatedField = type.GetField("buttonActivated", BindingFlags.Instance | BindingFlags.NonPublic);
        leverActivatedField = type.GetField("leverActivated", BindingFlags.Instance | BindingFlags.NonPublic);

        levelRoot = ReadField<GameObject>(type, "levelRoot");
        player = ReadField<Transform>(type, "player");
        interactionWall = ReadField<Transform>(type, "interactionWall");
        warningWall = ReadField<Transform>(type, "warningWall");
        entityAudio = ReadField<AudioSource>(type, "entityAudio");
        heartbeatAudio = ReadField<AudioSource>(type, "heartbeatAudio");

        pingEmitter = FindObjectOfType<PingEmitter>();
        if (pingEmitter != null) pingEmitter.OnPingEmitted += OnPing;
        EchoButtonInteractable.OnAnyButtonPressed += OnButton;
        LeverInteractable.OnAnyLeverTurnedOn += OnLever;

        TutorialRuntimeLogger.Begin("Automatic TutorialRuntimeObserver");
        TutorialRuntimeLogger.Event("ATTACH",
            "director=" + director.name +
            " levelRoot=" + NameOf(levelRoot) +
            " player=" + NameOf(player) +
            " interactionWall=" + NameOf(interactionWall) +
            " warningWall=" + NameOf(warningWall) +
            " entityAudio=" + NameOf(entityAudio) +
            " heartbeatAudio=" + NameOf(heartbeatAudio));

        LogReferenceHealth();
        CaptureChanges(true);
        LogHeartbeat();
    }

    T ReadField<T>(System.Type type, string name) where T : class
    {
        FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        return field != null ? field.GetValue(director) as T : null;
    }

    void Update()
    {
        if (!attached || director == null) return;

        CaptureChanges(false);
        if (Time.unscaledTime >= nextHeartbeat)
        {
            nextHeartbeat = Time.unscaledTime + 1f;
            LogHeartbeat();
        }
    }

    void CaptureChanges(bool force)
    {
        string step = stepField != null ? System.Convert.ToString(stepField.GetValue(director)) : "UNKNOWN";
        Track("STEP", ref lastStep, step, force);

        TextMeshPro sonar = levelRoot != null ? FindText(levelRoot.transform, "Sonar Wall Tip") : null;
        TextMeshPro interaction = levelRoot != null ? FindText(levelRoot.transform, "Interaction Wall Tip") : null;
        TextMeshPro warningTip = levelRoot != null ? FindText(levelRoot.transform, "Entity Wall Tip") : null;

        string sonarState = TextState(sonar);
        string interactionState = TextState(interaction);
        string warningState = TextState(warningTip);
        Track("TEXT_SONAR", ref lastSonarText, sonarState, force);
        Track("TEXT_INTERACTION", ref lastInteractionText, interactionState, force);
        Track("TEXT_WARNING", ref lastWarningText, warningState, force);

        Track("ENTITY_AUDIO_PLAYING", ref lastEntityAudioPlaying, entityAudio != null && entityAudio.isPlaying, force);
        Track("HEARTBEAT_PLAYING", ref lastHeartbeatPlaying, heartbeatAudio != null && heartbeatAudio.isPlaying, force);
    }

    void LogHeartbeat()
    {
        float readTimer = readTimerField != null ? (float)readTimerField.GetValue(director) : -1f;
        string step = stepField != null ? System.Convert.ToString(stepField.GetValue(director)) : "UNKNOWN";
        bool buttonDone = buttonActivatedField != null && (bool)buttonActivatedField.GetValue(director);
        bool leverDone = leverActivatedField != null && (bool)leverActivatedField.GetValue(director);

        TutorialRuntimeLogger.Event("HEARTBEAT",
            "step=" + step +
            " buttonActivated=" + buttonDone +
            " leverActivated=" + leverDone +
            " player=" + PositionOf(player) +
            " interactionDistance=" + DistanceTo(interactionWall) +
            " warningWallDistance=" + DistanceTo(warningWall) +
            " warningReadTimer=" + readTimer.ToString("F2") +
            " entityAudioPlaying=" + (entityAudio != null && entityAudio.isPlaying) +
            " entityVolume=" + (entityAudio != null ? entityAudio.volume.ToString("F2") : "NA") +
            " heartbeatPlaying=" + (heartbeatAudio != null && heartbeatAudio.isPlaying) +
            " heartbeatVolume=" + (heartbeatAudio != null ? heartbeatAudio.volume.ToString("F2") : "NA"));
    }

    void LogReferenceHealth()
    {
        Check("levelRoot", levelRoot);
        Check("player", player);
        Check("interactionWall", interactionWall);
        Check("warningWall", warningWall);
        Check("entityAudio", entityAudio);
        Check("heartbeatAudio", heartbeatAudio);
        Check("pingEmitter", pingEmitter);
        Check("stepField", stepField);
        Check("readTimerField", readTimerField);
        Check("buttonActivatedField", buttonActivatedField);
        Check("leverActivatedField", leverActivatedField);
    }

    void Check(string label, object value)
    {
        if (value == null) TutorialRuntimeLogger.Error("REFERENCE", label + " is NULL.");
        else TutorialRuntimeLogger.Event("REFERENCE", label + " OK.");
    }

    void OnPing(Vector3 origin)
    {
        TutorialRuntimeLogger.Event("PING",
            "origin=" + origin.ToString("F2") +
            " stepBeforeDirectorCallbackMayDependOnSubscriptionOrder=" +
            (stepField != null ? System.Convert.ToString(stepField.GetValue(director)) : "UNKNOWN"));
    }

    void OnButton(EchoButtonInteractable button)
    {
        TutorialRuntimeLogger.Event("BUTTON",
            "object=" + NameOf(button) +
            " isOn=" + (button != null && button.IsOn) +
            " belongsToTutorial=" + BelongsToLevel(button != null ? button.transform : null) +
            " step=" + CurrentStep());
    }

    void OnLever(LeverInteractable lever)
    {
        TutorialRuntimeLogger.Event("LEVER",
            "object=" + NameOf(lever) +
            " isOn=" + (lever != null && lever.IsOn) +
            " belongsToTutorial=" + BelongsToLevel(lever != null ? lever.transform : null) +
            " step=" + CurrentStep());
    }

    string CurrentStep()
    {
        return stepField != null ? System.Convert.ToString(stepField.GetValue(director)) : "UNKNOWN";
    }

    bool BelongsToLevel(Transform target)
    {
        return target != null && levelRoot != null && target.IsChildOf(levelRoot.transform);
    }

    string DistanceTo(Transform target)
    {
        if (player == null || target == null) return "NA";
        Vector3 a = player.position;
        Vector3 b = target.position;
        a.y = b.y = 0f;
        return Vector3.Distance(a, b).ToString("F2");
    }

    static TextMeshPro FindText(Transform root, string name)
    {
        Transform target = root.Find(name);
        return target != null ? target.GetComponent<TextMeshPro>() : null;
    }

    static GameObject FindChild(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child.gameObject;
        return null;
    }

    static string TextState(TextMeshPro text)
    {
        if (text == null) return "NULL";
        return "active=" + text.gameObject.activeSelf +
               " alpha=" + text.alpha.ToString("F1") +
               " text=" + (text.text ?? string.Empty).Replace("\n", " / ");
    }

    static void Track(string category, ref string previous, string current, bool force)
    {
        if (!force && previous == current) return;
        TutorialRuntimeLogger.Event(category, previous + " -> " + current);
        previous = current;
    }

    static void Track(string category, ref bool? previous, bool current, bool force)
    {
        if (!force && previous.HasValue && previous.Value == current) return;
        TutorialRuntimeLogger.Event(category, (previous.HasValue ? previous.Value.ToString() : "UNSET") + " -> " + current);
        previous = current;
    }

    static string PositionOf(Transform value)
    {
        return value != null ? value.position.ToString("F2") : "NULL";
    }

    static string NameOf(Object value)
    {
        return value != null ? value.name : "NULL";
    }

    void Detach()
    {
        if (pingEmitter != null) pingEmitter.OnPingEmitted -= OnPing;
        EchoButtonInteractable.OnAnyButtonPressed -= OnButton;
        LeverInteractable.OnAnyLeverTurnedOn -= OnLever;

        attached = false;
        director = null;
        levelRoot = null;
        player = null;
        interactionWall = null;
        warningWall = null;
        entityAudio = null;
        heartbeatAudio = null;
        pingEmitter = null;
    }

    void OnDestroy()
    {
        if (attached) Detach();
    }
}
