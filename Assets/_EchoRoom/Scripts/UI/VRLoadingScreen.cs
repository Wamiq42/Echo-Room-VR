using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace EchoRoom.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(UIDocument))]
    public sealed class VRLoadingScreen : MonoBehaviour
    {
        [SerializeField] VisualTreeAsset loadingLayout;
        [SerializeField] StyleSheet loadingStyles;
        [SerializeField] Transform cameraTransform;
        [SerializeField, Min(0.5f)] float distanceFromCamera = 1.15f;
        [SerializeField] float heightOffset;
        [SerializeField, Min(0.1f)] float minimumDisplayTime = 4f;
        [SerializeField, Min(0.1f)] float thankYouDuration = 5f;
        [SerializeField, Min(0.0001f)] float worldScale = 0.0016f;
        [SerializeField, Min(1f)] float transitionWatchdogSeconds = 25f;

        // A levelled panel seen from far above or below is edge-on, so stop tracking pitch there.
        const float MaxTargetPitchRadians = 55f * Mathf.Deg2Rad;
        const float MinimumPrefabSwapDisplaySeconds = 5f;

        readonly List<GameObject> suppressedPointers = new List<GameObject>();
        UIDocument document;
        VisualElement root;
        VisualElement loadingScreen;
        VisualElement thankYouScreen;
        VisualElement progressFill;
        VisualElement ringOne;
        VisualElement ringTwo;
        Label loadingMessage;
        Label progressLabel;
        Label returnCountdown;
        Coroutine watchdogRoutine;
        bool visible;
        bool busy;
        bool transitioning;
        bool placementDirty;
        float spin;

        public static VRLoadingScreen Instance { get; private set; }
        public bool IsVisible => visible;
        public bool IsBusy => busy;
        public bool IsTransitioning => transitioning;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null) transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            document = GetComponent<UIDocument>();
            if (loadingLayout == null) loadingLayout = Resources.Load<VisualTreeAsset>("UI/VRLoadingScreen");
            if (loadingStyles == null) loadingStyles = Resources.Load<StyleSheet>("UI/VRLoadingScreen");

            if (loadingLayout == null)
            {
                Debug.LogError("[VRLoadingScreen] Resources/UI/VRLoadingScreen.uxml is missing.", this);
                enabled = false;
                return;
            }

            document.visualTreeAsset = loadingLayout;
            document.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
            document.worldSpaceSize = new Vector2(900f, 560f);
            document.pivot = Pivot.Center;
            document.position = Position.Absolute;
            document.sortingOrder = 1000f;

            VisualElement documentRoot = document.rootVisualElement;
            if (loadingStyles != null && !documentRoot.styleSheets.Contains(loadingStyles))
                documentRoot.styleSheets.Add(loadingStyles);
            root = documentRoot.Q<VisualElement>("loading-root");
            if (root == null)
            {
                Debug.LogError("[VRLoadingScreen] loading-root is missing from the layout.", this);
                enabled = false;
                return;
            }

            loadingScreen = root.Q<VisualElement>("loading-screen");
            thankYouScreen = root.Q<VisualElement>("thank-you-screen");
            progressFill = root.Q<VisualElement>("progress-fill");
            ringOne = root.Q<VisualElement>(className: "ring-one");
            ringTwo = root.Q<VisualElement>(className: "ring-two");
            loadingMessage = root.Q<Label>("loading-message");
            progressLabel = root.Q<Label>("progress-label");
            returnCountdown = root.Q<Label>("return-countdown");

            transform.localScale = new Vector3(-worldScale, worldScale, worldScale);
            ResolveCamera();
            HideImmediate();
        }

        void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }

        void Update()
        {
            if (!visible) return;
            spin = Mathf.Repeat(spin + 95f * Time.unscaledDeltaTime, 360f);
            if (ringOne != null) ringOne.style.rotate = new Rotate(new Angle(spin, AngleUnit.Degree));
            if (ringTwo != null) ringTwo.style.rotate = new Rotate(new Angle(-spin * 1.35f, AngleUnit.Degree));
        }

        void LateUpdate()
        {
            if (transitioning) SuppressPauseMenu();
            if (visible) UpdatePlacement();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Camera.main from the previous scene is gone; never trust the cached transform across a load.
            cameraTransform = null;
            ResolveCamera();

            if (!transitioning) return;

            // Anything captured before the load belonged to the unloaded scene.
            suppressedPointers.Clear();
            SuppressUiPointers();
            SuppressPauseMenu();
            if (visible) PlaceInFrontOfPlayer();

            // The new rig's tracked pose has not been applied yet, so snap again next frame
            // instead of easing away from a placement made against the untracked pose.
            placementDirty = true;
        }

        public void ShowLoading(string message)
        {
            if (root == null) return;
            visible = true;
            root.style.display = DisplayStyle.Flex;
            loadingScreen.style.display = DisplayStyle.Flex;
            thankYouScreen.style.display = DisplayStyle.None;
            if (loadingMessage != null) loadingMessage.text = string.IsNullOrWhiteSpace(message) ? "TUNING SIGNAL" : message;
            SetProgress(0f);
            PlaceInFrontOfPlayer();
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (progressFill != null) progressFill.style.width = Length.Percent(progress * 100f);
            if (progressLabel != null) progressLabel.text = Mathf.RoundToInt(progress * 100f) + "%";
        }

        public void Hide()
        {
            visible = false;
            busy = false;
            if (root != null) root.style.display = DisplayStyle.None;
            EndTransitionState();
        }

        public void BeginTransition(string message)
        {
            if (!transitioning)
            {
                transitioning = true;
                SuppressUiPointers();
                if (watchdogRoutine != null) StopCoroutine(watchdogRoutine);
                watchdogRoutine = StartCoroutine(TransitionWatchdogRoutine());
            }

            SuppressPauseMenu();
            ShowLoading(message);
        }

        public void CompleteTransition()
        {
            Hide();
        }

        public void LoadScene(string sceneName, string message)
        {
            if (!busy) StartCoroutine(LoadSceneRoutine(sceneName, message));
        }

        public void ShowLevelCompletedThenLoadScene(string mainMenuSceneName)
        {
            if (!busy) StartCoroutine(ShowLevelCompletedThenLoad(mainMenuSceneName));
        }

        public IEnumerator LoadSceneRoutine(string sceneName, string message)
        {
            busy = true;
            BeginTransition(message);
            float shownAt = Time.realtimeSinceStartup;
            yield return null;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError("[VRLoadingScreen] Could not load scene '" + sceneName + "'.", this);
                CompleteTransition();
                yield break;
            }

            operation.allowSceneActivation = false;
            while (operation.progress < 0.9f || Time.realtimeSinceStartup - shownAt < minimumDisplayTime)
            {
                float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
                float timeProgress = Mathf.Clamp01((Time.realtimeSinceStartup - shownAt) / minimumDisplayTime);
                SetProgress(Mathf.Min(loadProgress, timeProgress));
                yield return null;
            }

            SetProgress(1f);
            yield return null;

            // The screen stays up past activation. The destination owner calls CompleteTransition().
            operation.allowSceneActivation = true;
        }

        public IEnumerator ShowLevelCompletedThenLoad(string mainMenuSceneName)
        {
            busy = true;
            BeginTransition("RETURNING TO MAIN MENU");
            if (root == null)
            {
                CompleteTransition();
                yield break;
            }

            loadingScreen.style.display = DisplayStyle.None;
            thankYouScreen.style.display = DisplayStyle.Flex;
            PlaceInFrontOfPlayer();

            float endTime = Time.realtimeSinceStartup + thankYouDuration;
            while (Time.realtimeSinceStartup < endTime)
            {
                if (returnCountdown != null)
                {
                    int seconds = Mathf.Max(1, Mathf.CeilToInt(endTime - Time.realtimeSinceStartup));
                    returnCountdown.text = "RETURNING TO MAIN MENU IN " + seconds;
                }
                yield return null;
            }

            loadingScreen.style.display = DisplayStyle.Flex;
            thankYouScreen.style.display = DisplayStyle.None;
            if (loadingMessage != null) loadingMessage.text = "RETURNING TO MAIN MENU";
            yield return null;

            AsyncOperation operation = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError("[VRLoadingScreen] Could not load main menu scene '" + mainMenuSceneName + "'.", this);
                CompleteTransition();
                yield break;
            }

            operation.allowSceneActivation = false;
            float loadingShownAt = Time.realtimeSinceStartup;
            while (operation.progress < 0.9f || Time.realtimeSinceStartup - loadingShownAt < minimumDisplayTime)
            {
                float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
                float timeProgress = Mathf.Clamp01((Time.realtimeSinceStartup - loadingShownAt) / minimumDisplayTime);
                SetProgress(Mathf.Min(loadProgress, timeProgress));
                yield return null;
            }

            SetProgress(1f);
            yield return null;
            operation.allowSceneActivation = true;
        }

        public IEnumerator CoverPrefabSwap(string message, System.Action swapAction)
        {
            busy = true;
            BeginTransition(message);
            float shownAt = Time.realtimeSinceStartup;
            try
            {
                yield return null;
                float requiredDisplayTime = Mathf.Max(MinimumPrefabSwapDisplaySeconds, minimumDisplayTime);
                while (Time.realtimeSinceStartup - shownAt < requiredDisplayTime)
                {
                    float elapsed = Time.realtimeSinceStartup - shownAt;
                    SetProgress(Mathf.Min(0.95f, elapsed / requiredDisplayTime));
                    yield return null;
                }

                SetProgress(1f);
                yield return null;

                // Hide before the synchronous prefab swap. The next level's intro UI is created
                // inside swapAction, so leaving this visible for even one rendered frame makes the
                // loading panel and level title overlap through the shared overlay camera.
                CompleteTransition();
                swapAction?.Invoke();
            }
            finally
            {
                CompleteTransition();
            }
        }

        IEnumerator TransitionWatchdogRoutine()
        {
            yield return new WaitForSecondsRealtime(transitionWatchdogSeconds);
            watchdogRoutine = null;
            if (!transitioning) yield break;

            Debug.LogError("[VRLoadingScreen] No owner completed the transition within " +
                           transitionWatchdogSeconds + "s. Hiding the loading screen.", this);
            Hide();
        }

        void EndTransitionState()
        {
            if (watchdogRoutine != null)
            {
                StopCoroutine(watchdogRoutine);
                watchdogRoutine = null;
            }

            if (!transitioning) return;
            transitioning = false;
            RestoreUiPointers();
        }

        void SuppressUiPointers()
        {
            NearFarInteractor[] toolkitPointers = FindObjectsByType<NearFarInteractor>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < toolkitPointers.Length; i++)
            {
                NearFarInteractor pointer = toolkitPointers[i];
                if (pointer == null || !pointer.enableUIInteraction) continue;
                SuppressPointerObject(pointer.gameObject);
            }

            XRRayInteractor[] rays = FindObjectsByType<XRRayInteractor>(FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < rays.Length; i++)
            {
                XRRayInteractor ray = rays[i];
                if (ray == null || !ray.enableUIInteraction) continue;
                SuppressPointerObject(ray.gameObject);
            }
        }

        void SuppressPointerObject(GameObject pointer)
        {
            if (pointer == null || !pointer.activeSelf) return;
            if (suppressedPointers.Contains(pointer)) return;
            suppressedPointers.Add(pointer);
            pointer.SetActive(false);
        }

        void RestoreUiPointers()
        {
            for (int i = 0; i < suppressedPointers.Count; i++)
                if (suppressedPointers[i] != null) suppressedPointers[i].SetActive(true);
            suppressedPointers.Clear();
        }

        static void SuppressPauseMenu()
        {
            VRPauseMenu pauseMenu = VRPauseMenu.Instance;
            if (pauseMenu != null && pauseMenu.IsOpen) pauseMenu.HideMenu(true);
        }

        void HideImmediate()
        {
            visible = false;
            busy = false;
            if (root != null) root.style.display = DisplayStyle.None;
        }

        void PlaceInFrontOfPlayer()
        {
            ResolveCamera();
            if (cameraTransform == null)
            {
                // Mid-load there may be no camera yet; place on the first frame one exists.
                placementDirty = true;
                return;
            }

            ComputeTargetPose(out Vector3 position, out Quaternion rotation);
            ApplyPose(position, rotation);
            placementDirty = false;
        }

        void UpdatePlacement()
        {
            ResolveCamera();
            if (cameraTransform == null) return;

            // World-locked, not head-locked: the panel is placed once in front of the player and
            // then holds its world pose, so it never drifts with the gaze. It is re-placed only
            // when the placement is explicitly invalidated -- the first frame a camera exists, a
            // rig/camera swap, or a scene load -- so a fresh spawn still gets a panel in front of
            // the new pose instead of one stranded at the old world location.
            if (!placementDirty) return;

            ComputeTargetPose(out Vector3 targetPosition, out Quaternion targetRotation);
            ApplyPose(targetPosition, targetRotation);
            placementDirty = false;
        }

        void ComputeTargetPose(out Vector3 position, out Quaternion rotation)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
            // Looking straight up or down leaves no heading, so keep the one already in use.
            if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.ProjectOnPlane(-transform.forward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.forward;
            flatForward.Normalize();

            // The panel centre picks up head pitch so its one-time placement lands on the gaze
            // rather than leaving the view uncovered, but only its position does.
            float pitch = Mathf.Clamp(Mathf.Asin(Mathf.Clamp(cameraTransform.forward.y, -1f, 1f)),
                -MaxTargetPitchRadians, MaxTargetPitchRadians);
            Vector3 heading = flatForward * Mathf.Cos(pitch) + Vector3.up * Mathf.Sin(pitch);

            position = cameraTransform.position + heading * distanceFromCamera + Vector3.up * heightOffset;
            // Yaw only. Inheriting head pitch or roll on a panel this large is a nausea trigger.
            rotation = Quaternion.LookRotation(flatForward, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);
        }

        void ApplyPose(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            // The negative X mirrors the panel so world-space UI Toolkit renders it the right way round.
            transform.localScale = new Vector3(-worldScale, worldScale, worldScale);
        }

        void ResolveCamera()
        {
            // A cached transform survives Destroy only as a fake-null, but a camera that is merely
            // deactivated by a scene swap still compares non-null. Re-resolve in both cases.
            if (cameraTransform != null && cameraTransform.gameObject.activeInHierarchy) return;
            Transform previous = cameraTransform;
            cameraTransform = null;
            Camera main = Camera.main;
            if (main != null) cameraTransform = main.transform;
            // A different rig means the anchored pose belongs to a dead scene; never ease across that.
            if (cameraTransform != previous) placementDirty = true;
        }
    }
}
