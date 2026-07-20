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
        [System.Serializable]
        public struct SceneAnchor
        {
            public string sceneName;
            public Vector3 position;
            public Vector3 eulerAngles;
        }

        [SerializeField] VisualTreeAsset loadingLayout;
        [SerializeField] StyleSheet loadingStyles;
        [SerializeField] Transform cameraTransform;
        [SerializeField, Min(0.5f)] float distanceFromCamera = 1.15f;
        [SerializeField] float heightOffset;
        [SerializeField] bool useFixedWorldPlacement = true;
        [SerializeField] Vector3 fixedWorldPosition = new(-3.04f, 0.95f, -1.342f);
        [SerializeField] Vector3 fixedWorldEulerAngles = new(0f, 89.752f, 0f);
        [SerializeField] SceneAnchor[] sceneAnchors =
        {
            new SceneAnchor
            {
                sceneName = "MainMenuScene",
                position = new Vector3(0f, 1.15f, -8.75f),
                eulerAngles = new Vector3(0f, 180f, 0f)
            },
            new SceneAnchor
            {
                sceneName = "MainScene",
                position = new Vector3(-3.04f, 0.95f, -1.342f),
                eulerAngles = new Vector3(0f, 89.752f, 0f)
            }
        };
        [SerializeField, Min(0.1f)] float minimumDisplayTime = 4f;
        [SerializeField, Min(0.1f)] float thankYouDuration = 5f;
        [SerializeField, Min(0.0001f)] float worldScale = 0.0016f;
        [SerializeField, Min(1f)] float transitionWatchdogSeconds = 25f;
        [SerializeField] string uiPointerFallbackName = "Menu UI Ray";

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
        string activeSceneName;
        bool visible;
        bool busy;
        bool transitioning;
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
            activeSceneName = SceneManager.GetActiveScene().name;

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
            if (visible) PlaceInFrontOfPlayer();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            activeSceneName = SceneManager.GetActiveScene().name;

            // Camera.main from the previous scene is gone; never trust the cached transform across a load.
            cameraTransform = null;
            ResolveCamera();

            if (!transitioning) return;

            // Anything captured before the load belonged to the unloaded scene.
            suppressedPointers.Clear();
            SuppressUiPointers();
            SuppressPauseMenu();
            if (visible) PlaceInFrontOfPlayer();
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

        public void ShowThankYouThenLoadScene(string mainMenuSceneName)
        {
            if (!busy) StartCoroutine(ShowThankYouThenLoad(mainMenuSceneName));
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

        public IEnumerator ShowThankYouThenLoad(string mainMenuSceneName)
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
                SetProgress(0.2f);
                swapAction?.Invoke();
                SetProgress(0.85f);
                yield return null;
                SetProgress(1f);

                float remaining = minimumDisplayTime - (Time.realtimeSinceStartup - shownAt);
                if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);
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
            XRRayInteractor[] rays = FindObjectsByType<XRRayInteractor>(FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < rays.Length; i++)
            {
                XRRayInteractor ray = rays[i];
                if (ray == null || !ray.enableUIInteraction) continue;
                SuppressPointerObject(ray.gameObject);
            }

            if (string.IsNullOrEmpty(uiPointerFallbackName)) return;
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != null && transforms[i].name == uiPointerFallbackName)
                    SuppressPointerObject(transforms[i].gameObject);
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
            if (useFixedWorldPlacement)
            {
                ResolveSceneAnchor(out Vector3 anchorPosition, out Vector3 anchorEulerAngles);
                transform.SetPositionAndRotation(anchorPosition, Quaternion.Euler(anchorEulerAngles));
                transform.localScale = new Vector3(-worldScale, worldScale, worldScale);
                return;
            }

            ResolveCamera();
            if (cameraTransform == null) return;
            transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera +
                                 cameraTransform.up * heightOffset;
            transform.rotation = cameraTransform.rotation * Quaternion.Euler(0f, 180f, 0f);
            transform.localScale = new Vector3(-worldScale, worldScale, worldScale);
        }

        void ResolveSceneAnchor(out Vector3 position, out Vector3 eulerAngles)
        {
            if (sceneAnchors != null)
            {
                for (int i = 0; i < sceneAnchors.Length; i++)
                {
                    if (!string.Equals(sceneAnchors[i].sceneName, activeSceneName, System.StringComparison.Ordinal))
                        continue;
                    position = sceneAnchors[i].position;
                    eulerAngles = sceneAnchors[i].eulerAngles;
                    return;
                }
            }

            position = fixedWorldPosition;
            eulerAngles = fixedWorldEulerAngles;
        }

        void ResolveCamera()
        {
            // A cached transform survives Destroy only as a fake-null, but a camera that is merely
            // deactivated by a scene swap still compares non-null. Re-resolve in both cases.
            if (cameraTransform != null && cameraTransform.gameObject.activeInHierarchy) return;
            cameraTransform = null;
            Camera main = Camera.main;
            if (main != null) cameraTransform = main.transform;
        }
    }
}
