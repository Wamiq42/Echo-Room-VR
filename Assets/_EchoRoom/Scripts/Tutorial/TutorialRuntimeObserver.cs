using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public sealed class TutorialRuntimeObserver : MonoBehaviour
{
    TutorialDirector director;
    GameObject levelRoot;
    Transform player;
    Transform interactionWall;
    AudioSource entityAudio;
    AudioSource heartbeatAudio;
    PingEmitter pingEmitter;

    string lastStep;
    string lastPromptText;
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
                if (candidate != null && candidate.enabled &&
                    candidate.TryGetRuntimeDiagnosticState(out TutorialDirector.RuntimeDiagnosticState state))
                    Attach(candidate, state);
            }

            yield return null;
        }
    }

    void Attach(TutorialDirector target, TutorialDirector.RuntimeDiagnosticState state)
    {
        attached = true;
        director = target;
        RefreshReferences(state);

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
            " tutorialPrompt=" + NameOf(state.Prompt.PromptObject) +
            " promptElement=" + state.Prompt.ElementName +
            " entityAudio=" + NameOf(entityAudio) +
            " heartbeatAudio=" + NameOf(heartbeatAudio));

        LogReferenceHealth(state);
        CaptureChanges(state, true);
        LogHeartbeat(state);
    }

    void Update()
    {
        if (!attached || director == null) return;
        director.TryGetRuntimeDiagnosticState(out TutorialDirector.RuntimeDiagnosticState state);
        RefreshReferences(state);

        CaptureChanges(state, false);
        if (Time.unscaledTime >= nextHeartbeat)
        {
            nextHeartbeat = Time.unscaledTime + 1f;
            LogHeartbeat(state);
        }
    }

    void CaptureChanges(TutorialDirector.RuntimeDiagnosticState state, bool force)
    {
        Track("STEP", ref lastStep, state.Step, force);
        Track("TEXT_PROMPT", ref lastPromptText, PromptState(state.Prompt), force);

        Track("ENTITY_AUDIO_PLAYING", ref lastEntityAudioPlaying, entityAudio != null && entityAudio.isPlaying, force);
        Track("HEARTBEAT_PLAYING", ref lastHeartbeatPlaying, heartbeatAudio != null && heartbeatAudio.isPlaying, force);
    }

    void LogHeartbeat(TutorialDirector.RuntimeDiagnosticState state)
    {
        TutorialRuntimeLogger.Event("HEARTBEAT",
            "step=" + state.Step +
            " buttonActivated=" + state.ButtonActivated +
            " leverActivated=" + state.LeverActivated +
            " player=" + PositionOf(player) +
            " interactionDistance=" + DistanceTo(interactionWall) +
            " entityAudioPlaying=" + (entityAudio != null && entityAudio.isPlaying) +
            " entityVolume=" + (entityAudio != null ? entityAudio.volume.ToString("F2") : "NA") +
            " heartbeatPlaying=" + (heartbeatAudio != null && heartbeatAudio.isPlaying) +
            " heartbeatVolume=" + (heartbeatAudio != null ? heartbeatAudio.volume.ToString("F2") : "NA"));
    }

    void LogReferenceHealth(TutorialDirector.RuntimeDiagnosticState state)
    {
        Check("levelRoot", levelRoot);
        Check("player", player);
        Check("interactionWall", interactionWall);
        Check("tutorialPromptDocument", state.Prompt.DocumentReady ? state.Prompt.PromptObject : null);
        Check("tutorialPromptRoot", state.Prompt.RootReady ? state.Prompt.PromptObject : null);
        Check("tutorialPromptText", state.Prompt.TextReady ? state.Prompt.PromptObject : null);
        Check("entityAudio", entityAudio);
        Check("heartbeatAudio", heartbeatAudio);
        Check("pingEmitter", pingEmitter);
        TutorialRuntimeLogger.Event("REFERENCE", "step OK (typed diagnostic contract).");
        TutorialRuntimeLogger.Event("REFERENCE", "buttonActivated OK (typed diagnostic contract).");
        TutorialRuntimeLogger.Event("REFERENCE", "leverActivated OK (typed diagnostic contract).");
        TutorialRuntimeLogger.Event("PROMPT_PANEL", "attached=" + state.Prompt.Attached);
    }

    void Check(string label, object value)
    {
        if (value == null) TutorialRuntimeLogger.Error("REFERENCE", label + " is NULL.");
        else TutorialRuntimeLogger.Event("REFERENCE", label + " OK.");
    }

    void OnPing(Vector3 origin)
    {
        director.TryGetRuntimeDiagnosticState(out TutorialDirector.RuntimeDiagnosticState state);
        TutorialRuntimeLogger.Event("PING",
            "origin=" + origin.ToString("F2") +
            " stepBeforeDirectorCallbackMayDependOnSubscriptionOrder=" +
            state.Step);
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
        if (director == null) return "UNKNOWN";
        director.TryGetRuntimeDiagnosticState(out TutorialDirector.RuntimeDiagnosticState state);
        return state.Step;
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

    static string PromptState(TutorialDirector.PromptDiagnosticState prompt)
    {
        if (!prompt.Created) return "NULL";
        return "active=" + prompt.Active +
               " alpha=" + prompt.Opacity.ToString("F1") +
               " text=" + (prompt.Text ?? string.Empty).Replace("\n", " / ");
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

    void RefreshReferences(TutorialDirector.RuntimeDiagnosticState state)
    {
        levelRoot = state.LevelRoot;
        player = state.Player;
        interactionWall = state.InteractionWall;
        entityAudio = state.EntityAudio;
        heartbeatAudio = state.HeartbeatAudio;
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
        entityAudio = null;
        heartbeatAudio = null;
        pingEmitter = null;
    }

    void OnDestroy()
    {
        if (attached) Detach();
    }
}
