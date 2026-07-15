using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EchoRoom.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(UIDocument), typeof(BoxCollider))]
    public class VRPauseMenu : MonoBehaviour
    {
        public enum MenuState { Hidden, Start, Pause, Captured }

        [Header("UI Toolkit")]
        [SerializeField] VisualTreeAsset menuLayout;
        [SerializeField] StyleSheet menuStyles;
        [SerializeField] Transform cameraTransform;
        [SerializeField] GameObject[] vrPointerRoots;

        [Header("Behaviour")]
        [SerializeField] bool showOnStart = true;
        [SerializeField] bool pauseTimeWhileOpen = true;
        [SerializeField] bool pauseEnvironmentAudioWhileOpen = true;
        [SerializeField] bool useControllerMenuButton = true;
        [SerializeField, Min(0.25f)] float distanceFromCamera = 2.1f;
        [SerializeField] float heightOffset = -0.05f;
        [SerializeField] bool keepInFrontOfWalls = true;
        [SerializeField, Min(0.01f)] float wallPadding = 0.15f;
        [SerializeField, Min(0.25f)] float minimumDistanceFromCamera = 0.45f;
        [SerializeField] LayerMask wallMask = ~0;
        [SerializeField] KeyCode keyboardPauseKey = KeyCode.Escape;

        [Header("Return To Menu")]
        [SerializeField] UnityEvent onReturnToMenu;

        const int MaxPointerCount = 32;
        readonly List<UnityEngine.XR.InputDevice> controllers = new List<UnityEngine.XR.InputDevice>();
        readonly List<RendererOverlayState> playerOverlayStates = new List<RendererOverlayState>();
        UIDocument document;
        BoxCollider documentCollider;
        VisualElement menuRoot;
        VisualElement pauseScreen;
        VisualElement capturedScreen;
        Label capturedTitle;
        Label capturedSubtitle;
        Label capturedBody;
        VRFrontEndMenu frontEnd;
        bool controllerWasPressed;
        bool ownsTimePause;
        float previousTimeScale = 1f;
        bool ownsAudioPause;
        bool previousAudioPause;
        Coroutine enableInputRoutine;
#if ENABLE_INPUT_SYSTEM
        InputAction pauseAction;
#endif

        sealed class RendererOverlayState
        {
            public Renderer renderer;
            public Material[] originalMaterials;
            public Material[] overlayMaterials;
        }

        public static VRPauseMenu Instance { get; private set; }
        public MenuState State { get; private set; } = MenuState.Hidden;
        public bool IsOpen => State != MenuState.Hidden;
        internal VisualElement Root => menuRoot;
        public static event System.Action<MenuState> OnMenuStateChanged;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Debug.LogWarning("[VRPauseMenu] Duplicate menu found.", this);

            document = GetComponent<UIDocument>();
            documentCollider = GetComponent<BoxCollider>();
            ResolveAssets();
            ConfigureDocument();
            ResolveVisualElements();
            BindCoreButtons();
            ResolveCamera();
            ConfigurePauseInput();
        }

        void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            pauseAction?.Enable();
#endif
        }

        void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            pauseAction?.Disable();
#endif
            if (State != MenuState.Hidden)
                HideMenu(true);
            else
            {
                RestorePlayerVisualOverlay();
                RestoreTime();
                RestoreEnvironmentAudio();
            }
        }

        void Start()
        {
            if (showOnStart) ShowStartMenu();
            else HideMenu(false);
        }

        void Update()
        {
            if (!PausePressed() || State == MenuState.Captured || State == MenuState.Start) return;
            if (State == MenuState.Pause) ContinueGame();
            else ShowPauseMenu();
        }

        void LateUpdate()
        {
            if (IsOpen) ApplyPlayerVisualOverlay();
        }

        void ResolveAssets()
        {
            if (menuLayout == null) menuLayout = Resources.Load<VisualTreeAsset>("UI/VRMenu");
            if (menuStyles == null) menuStyles = Resources.Load<StyleSheet>("UI/VRMenu");
            if (menuLayout == null)
            {
                Debug.LogError("[VRPauseMenu] UI Toolkit layout Resources/UI/VRMenu.uxml was not found.", this);
                enabled = false;
                return;
            }
            if (document.visualTreeAsset != menuLayout) document.visualTreeAsset = menuLayout;
        }

        void ConfigureDocument()
        {
            document.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
            document.worldSpaceSize = new Vector2(900f, 560f);
            document.pivot = Pivot.Center;
            document.position = Position.Absolute;
            document.sortingOrder = 100f;
            if (documentCollider != null)
            {
                documentCollider.isTrigger = true;
                documentCollider.center = Vector3.zero;
                documentCollider.size = new Vector3(900f, 560f, 4f);
            }

            Vector3 panelScale = transform.localScale;
            panelScale.x = -Mathf.Abs(panelScale.x);
            panelScale.y = Mathf.Abs(panelScale.y);
            panelScale.z = Mathf.Abs(panelScale.z);
            transform.localScale = panelScale;
        }

        void ResolveVisualElements()
        {
            menuRoot = document.rootVisualElement;
            if (menuRoot == null) return;
            if (menuStyles != null && !menuRoot.styleSheets.Contains(menuStyles))
                menuRoot.styleSheets.Add(menuStyles);
            pauseScreen = menuRoot.Q<VisualElement>("pause-screen");
            capturedScreen = menuRoot.Q<VisualElement>("captured-screen");
            capturedTitle = capturedScreen?.Query<Label>(className: "title").First();
            capturedSubtitle = capturedScreen?.Query<Label>(className: "subtitle").First();
            capturedBody = capturedScreen?.Query<Label>(className: "body").First();
        }

        void BindCoreButtons()
        {
            BindButton("continue-button", ContinueGame);
            BindButton("restart-button", RestartGame);
            BindButton("return-button", ReturnToMenu);
            BindButton("pause-return-button", ReturnToMenu);
        }

        void ConfigurePauseInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (pauseAction != null) return;

            pauseAction = new InputAction("Pause", InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Keyboard>/p");
            pauseAction.AddBinding("<Gamepad>/start");
            pauseAction.AddBinding("<XRController>{LeftHand}/menuButton");
            pauseAction.AddBinding("<XRController>{RightHand}/menuButton");
            if (isActiveAndEnabled) pauseAction.Enable();
#endif
        }

        void BindButton(string name, System.Action action)
        {
            Button button = menuRoot?.Q<Button>(name);
            if (button == null)
            {
                Debug.LogError("[VRPauseMenu] Missing UI Toolkit button: " + name, this);
                return;
            }
            button.clicked += action;
        }

        internal void RegisterFrontEnd(VRFrontEndMenu value)
        {
            frontEnd = value;
            SetContent(State == MenuState.Start, State == MenuState.Pause, State == MenuState.Captured);
        }

        public void ToggleMenu()
        {
            if (State == MenuState.Captured || State == MenuState.Start) return;
            if (State == MenuState.Pause) ContinueGame();
            else ShowPauseMenu();
        }

        public void ShowStartMenu() { SetState(MenuState.Start); }
        public void ShowMenu() { ShowPauseMenu(); }
        public void ShowPauseMenu() { SetState(MenuState.Pause); }
        public void ShowCapturedMenu()
        {
            SetFailureCopy("SIGNAL LOST", "YOU HAVE BEEN CAPTURED",
                "The room has gone silent. Re-enter the signal or return to the beginning.");
            SetState(MenuState.Captured);
        }

        public void ShowTimeUpMenu()
        {
            SetFailureCopy("TIME EXPIRED", "YOUR TIME IS UP",
                "The signal window has closed. Restart the maze or return to the main menu.");
            SetState(MenuState.Captured);
        }

        void SetFailureCopy(string title, string subtitle, string body)
        {
            if (capturedTitle != null) capturedTitle.text = title;
            if (capturedSubtitle != null) capturedSubtitle.text = subtitle;
            if (capturedBody != null) capturedBody.text = body;
        }

        public static bool TryShowCapturedMenu()
        {
            VRPauseMenu menu = Instance != null ? Instance : FindObjectOfType<VRPauseMenu>(true);
            if (menu == null) return false;
            menu.ShowCapturedMenu();
            return true;
        }

        public static bool TryShowTimeUpMenu()
        {
            VRPauseMenu menu = Instance != null ? Instance : FindObjectOfType<VRPauseMenu>(true);
            if (menu == null) return false;
            menu.ShowTimeUpMenu();
            return true;
        }

        public void ContinueGame()
        {
            if (State != MenuState.Captured) HideMenu(true);
        }

        public void RestartGame()
        {
            HideMenu(true);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else if (!string.IsNullOrEmpty(scene.name)) SceneManager.LoadScene(scene.name);
            else Debug.LogError("[VRPauseMenu] Active scene cannot be restarted.", this);
        }

        public void ReturnToMenu()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
                return;
            }

            HideMenu(true);
            if (onReturnToMenu != null && onReturnToMenu.GetPersistentEventCount() > 0)
                onReturnToMenu.Invoke();
            else
                SceneManager.LoadScene("MainMenuScene");
        }

        public void HideMenu(bool resumeTime)
        {
            State = MenuState.Hidden;
            SetContent(false, false, false);
            SetVisible(false);
            if (resumeTime) RestoreTime();
            RestoreEnvironmentAudio();
            OnMenuStateChanged?.Invoke(State);
        }

        void SetState(MenuState state)
        {
            ResolveCamera();
            State = state;
            SetContent(state == MenuState.Start, state == MenuState.Pause, state == MenuState.Captured);
            SetVisible(true);
            PlaceMenuInFrontOfPlayer();
            PauseTime();
            PauseEnvironmentAudio();
            OnMenuStateChanged?.Invoke(State);
        }

        void SetContent(bool start, bool pause, bool captured)
        {
            frontEnd?.SetVisible(start);
            SetElementVisible(pauseScreen, pause);
            SetElementVisible(capturedScreen, captured);
        }

        static void SetElementVisible(VisualElement element, bool visible)
        {
            if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void SetVisible(bool visible)
        {
            StopPendingInputActivation();
            ReleaseMenuPointerState();

            if (!visible)
            {
                if (documentCollider != null) documentCollider.enabled = false;
                SetVRPointersVisible(false);
                SetElementVisible(menuRoot, false);
                SetMenuButtonsEnabled(true);
                RestorePlayerVisualOverlay();
                return;
            }

            if (document != null && !document.enabled) document.enabled = true;
            SetElementVisible(menuRoot, true);
            SetMenuButtonsEnabled(false);
            if (documentCollider != null) documentCollider.enabled = false;
            SetVRPointersVisible(false);
            ApplyPlayerVisualOverlay();
            enableInputRoutine = StartCoroutine(EnableInputAfterLayout());
        }

        IEnumerator EnableInputAfterLayout()
        {
            menuRoot?.MarkDirtyRepaint();

            // UI Toolkit world-space geometry is zero-sized on the frame a hidden screen is shown.
            // Wait for two panel updates before accepting mouse or controller presses.
            yield return null;
            yield return null;

            enableInputRoutine = null;
            if (!isActiveAndEnabled || !IsOpen || menuRoot == null)
                yield break;

            PlaceMenuInFrontOfPlayer();
            menuRoot.MarkDirtyRepaint();
            SetMenuButtonsEnabled(true);
            if (documentCollider != null) documentCollider.enabled = true;
            SetVRPointersVisible(true);
        }

        void StopPendingInputActivation()
        {
            if (enableInputRoutine == null) return;
            StopCoroutine(enableInputRoutine);
            enableInputRoutine = null;
        }

        void ReleaseMenuPointerState()
        {
            if (menuRoot == null) return;
            ReleasePointerCaptures(menuRoot);
            menuRoot.panel?.focusController?.focusedElement?.Blur();
        }

        static void ReleasePointerCaptures(VisualElement element)
        {
            if (element == null) return;

            for (int pointerId = 0; pointerId < MaxPointerCount; pointerId++)
                if (element.HasPointerCapture(pointerId))
                    element.ReleasePointer(pointerId);

            foreach (VisualElement child in element.Children())
                ReleasePointerCaptures(child);
        }

        void SetMenuButtonsEnabled(bool enabled)
        {
            if (menuRoot == null) return;
            SetButtonsEnabled(menuRoot, enabled);
        }

        static void SetButtonsEnabled(VisualElement element, bool enabled)
        {
            if (element is Button button)
                button.SetEnabled(enabled);

            foreach (VisualElement child in element.Children())
                SetButtonsEnabled(child, enabled);
        }

        void SetVRPointersVisible(bool visible)
        {
            if (vrPointerRoots == null) return;
            for (int i = 0; i < vrPointerRoots.Length; i++)
                if (vrPointerRoots[i] != null && vrPointerRoots[i].activeSelf != visible)
                    vrPointerRoots[i].SetActive(visible);
        }

        void PauseTime()
        {
            if (!pauseTimeWhileOpen || ownsTimePause) return;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            ownsTimePause = true;
        }

        void RestoreTime()
        {
            if (!ownsTimePause) return;
            Time.timeScale = previousTimeScale > 0.0001f ? previousTimeScale : 1f;
            ownsTimePause = false;
        }

        void PauseEnvironmentAudio()
        {
            if (!pauseEnvironmentAudioWhileOpen || ownsAudioPause) return;
            previousAudioPause = AudioListener.pause;
            AudioListener.pause = true;
            ownsAudioPause = true;
        }

        void RestoreEnvironmentAudio()
        {
            if (!ownsAudioPause) return;
            AudioListener.pause = previousAudioPause;
            ownsAudioPause = false;
        }

        bool PausePressed()
        {
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            pressed = pauseAction != null && pauseAction.WasPressedThisFrame();
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
                pressed |= keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            pressed |= Input.GetKeyDown(keyboardPauseKey) || Input.GetKeyDown(KeyCode.P);
#endif
            return pressed || ControllerPausePressed();
        }

        bool ControllerPausePressed()
        {
            if (!useControllerMenuButton) return false;
            bool pressed = false;
            controllers.Clear();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, controllers);
            for (int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i].TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool value) && value)
                {
                    pressed = true;
                    break;
                }
            }
            bool thisFrame = pressed && !controllerWasPressed;
            controllerWasPressed = pressed;
            return thisFrame;
        }

        void PlaceMenuInFrontOfPlayer()
        {
            if (!IsOpen) return;
            ResolveCamera();
            if (cameraTransform == null) return;

            float placementDistance = distanceFromCamera;
            if (keepInFrontOfWalls && Physics.Raycast(cameraTransform.position, cameraTransform.forward,
                    out RaycastHit hit, distanceFromCamera, wallMask, QueryTriggerInteraction.Ignore))
                placementDistance = Mathf.Max(minimumDistanceFromCamera, hit.distance - wallPadding);

            transform.position = cameraTransform.position + cameraTransform.forward * placementDistance +
                                 cameraTransform.up * heightOffset;
            transform.rotation = cameraTransform.rotation * Quaternion.Euler(0f, 180f, 0f);
        }

        void ResolveCamera()
        {
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        void ApplyPlayerVisualOverlay()
        {
            ResolveCamera();
            if (cameraTransform != null)
            {
                Renderer[] rigRenderers = cameraTransform.root.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rigRenderers.Length; i++) AddPlayerOverlayRenderer(rigRenderers[i]);
            }

            Behaviour[] behaviours = FindObjectsOfType<Behaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour item = behaviours[i];
                if (item == null || item.GetType().Namespace == null ||
                    !item.GetType().Namespace.StartsWith("UnityEngine.XR.Interaction.Toolkit")) continue;

                PropertyInfo selectedProperty = item.GetType().GetProperty("isSelected",
                    BindingFlags.Instance | BindingFlags.Public);
                if (selectedProperty == null || selectedProperty.PropertyType != typeof(bool) ||
                    !(bool)selectedProperty.GetValue(item, null)) continue;

                Renderer[] heldRenderers = item.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < heldRenderers.Length; r++) AddPlayerOverlayRenderer(heldRenderers[r]);
            }
        }

        void AddPlayerOverlayRenderer(Renderer targetRenderer)
        {
            if (targetRenderer == null || targetRenderer.transform.IsChildOf(transform)) return;
            for (int i = 0; i < playerOverlayStates.Count; i++)
                if (playerOverlayStates[i].renderer == targetRenderer) return;

            Material[] originals = targetRenderer.sharedMaterials;
            Material[] overlays = new Material[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                if (originals[i] == null) continue;
                overlays[i] = new Material(originals[i])
                {
                    name = originals[i].name + " (Player Menu Overlay)",
                    renderQueue = 5000
                };
            }

            playerOverlayStates.Add(new RendererOverlayState
            {
                renderer = targetRenderer,
                originalMaterials = originals,
                overlayMaterials = overlays
            });
            targetRenderer.sharedMaterials = overlays;
        }

        void RestorePlayerVisualOverlay()
        {
            for (int i = 0; i < playerOverlayStates.Count; i++)
            {
                RendererOverlayState state = playerOverlayStates[i];
                if (state.renderer != null) state.renderer.sharedMaterials = state.originalMaterials;
                if (state.overlayMaterials == null) continue;
                for (int m = 0; m < state.overlayMaterials.Length; m++)
                    if (state.overlayMaterials[m] != null) Destroy(state.overlayMaterials[m]);
            }
            playerOverlayStates.Clear();
        }

        void OnDestroy()
        {
            StopPendingInputActivation();
            ReleaseMenuPointerState();
            RestorePlayerVisualOverlay();
            RestoreTime();
            RestoreEnvironmentAudio();
#if ENABLE_INPUT_SYSTEM
            pauseAction?.Dispose();
            pauseAction = null;
#endif
            if (Instance == this) Instance = null;
        }

        void OnValidate()
        {
            distanceFromCamera = Mathf.Max(0.25f, distanceFromCamera);
            minimumDistanceFromCamera = Mathf.Clamp(minimumDistanceFromCamera, 0.25f, distanceFromCamera);
            wallPadding = Mathf.Max(0.01f, wallPadding);
        }
    }
}
