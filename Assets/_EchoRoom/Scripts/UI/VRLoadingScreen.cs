using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
        [SerializeField, Min(0.1f)] float minimumDisplayTime = 1f;
        [SerializeField, Min(0.1f)] float thankYouDuration = 5f;
        [SerializeField, Min(0.0001f)] float worldScale = 0.0016f;

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
        bool visible;
        bool busy;
        float spin;

        public bool IsVisible => visible;
        public bool IsBusy => busy;

        void Awake()
        {
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

        void Update()
        {
            if (!visible) return;
            spin = Mathf.Repeat(spin + 95f * Time.unscaledDeltaTime, 360f);
            if (ringOne != null) ringOne.style.rotate = new Rotate(new Angle(spin, AngleUnit.Degree));
            if (ringTwo != null) ringTwo.style.rotate = new Rotate(new Angle(-spin * 1.35f, AngleUnit.Degree));
        }

        void LateUpdate()
        {
            if (visible) PlaceInFrontOfPlayer();
        }

        public void ShowLoading(string message)
        {
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
        }

        public void LoadScene(string sceneName, string message)
        {
            if (!busy) StartCoroutine(LoadSceneRoutine(sceneName, message));
        }

        public IEnumerator LoadSceneRoutine(string sceneName, string message)
        {
            busy = true;
            ShowLoading(message);
            float shownAt = Time.realtimeSinceStartup;
            yield return null;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError("[VRLoadingScreen] Could not load scene '" + sceneName + "'.", this);
                Hide();
                yield break;
            }

            while (!operation.isDone)
            {
                SetProgress(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            float remaining = minimumDisplayTime - (Time.realtimeSinceStartup - shownAt);
            if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);
        }

        public IEnumerator ShowThankYouThenLoad(string mainMenuSceneName)
        {
            busy = true;
            visible = true;
            root.style.display = DisplayStyle.Flex;
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
                Hide();
                yield break;
            }

            while (!operation.isDone)
            {
                SetProgress(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }
        }

        public IEnumerator CoverPrefabSwap(string message, System.Action swapAction)
        {
            busy = true;
            ShowLoading(message);
            float shownAt = Time.realtimeSinceStartup;
            try
            {
                yield return null;
                SetProgress(0.2f);
                swapAction?.Invoke();
                SetProgress(0.85f);
                yield return null;
                SetProgress(1f);

                float effectiveMinimum = Mathf.Max(1f, minimumDisplayTime);
                float remaining = effectiveMinimum - (Time.realtimeSinceStartup - shownAt);
                if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);
            }
            finally
            {
                Hide();
            }
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
            if (cameraTransform == null) return;
            transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera +
                                 cameraTransform.up * heightOffset;
            transform.rotation = cameraTransform.rotation * Quaternion.Euler(0f, 180f, 0f);
            transform.localScale = new Vector3(-worldScale, worldScale, worldScale);
        }

        void ResolveCamera()
        {
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }
    }
}
