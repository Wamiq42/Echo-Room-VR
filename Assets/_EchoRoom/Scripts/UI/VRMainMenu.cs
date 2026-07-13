using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace EchoRoom.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(UIDocument), typeof(BoxCollider), typeof(XRUIToolkitManager))]
    public sealed class VRMainMenu : MonoBehaviour
    {
        [SerializeField] LevelData levelData;
        [SerializeField] VisualTreeAsset menuLayout;
        [SerializeField] StyleSheet menuStyles;
        [SerializeField] VRLoadingScreen loadingScreen;
        [SerializeField] Transform cameraTransform;
        [SerializeField] string gameplaySceneName = "MainScene";
        [SerializeField, Min(0.5f)] float distanceFromCamera = 2.1f;
        [SerializeField] float heightOffset = -0.05f;
        [SerializeField, Min(0.0001f)] float worldScale = 0.0016f;

        UIDocument document;
        BoxCollider documentCollider;
        VisualElement root;
        VisualElement startScreen;
        VisualElement levelScreen;
        VisualElement warningScreen;
        Button newGameButton;
        Button loadGameButton;
        Button tutorialButton;
        readonly List<Button> levelButtons = new List<Button>();
        PuzzleProgressData progress;

        void Awake()
        {
            document = GetComponent<UIDocument>();
            documentCollider = GetComponent<BoxCollider>();
            if (menuLayout == null) menuLayout = Resources.Load<VisualTreeAsset>("UI/VRMenu");
            if (menuStyles == null) menuStyles = Resources.Load<StyleSheet>("UI/VRMenu");
            if (loadingScreen == null) loadingScreen = FindObjectOfType<VRLoadingScreen>(true);

            if (menuLayout == null || levelData == null)
            {
                Debug.LogError("[VRMainMenu] Menu layout or LevelData is missing.", this);
                enabled = false;
                return;
            }

            document.visualTreeAsset = menuLayout;
            document.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
            document.worldSpaceSize = new Vector2(900f, 560f);
            document.pivot = Pivot.Center;
            document.position = Position.Absolute;
            document.sortingOrder = 100f;
            documentCollider.isTrigger = true;
            documentCollider.center = Vector3.zero;
            documentCollider.size = new Vector3(900f, 560f, 4f);
            transform.localScale = new Vector3(-worldScale, worldScale, worldScale);

            root = document.rootVisualElement;
            if (menuStyles != null && !root.styleSheets.Contains(menuStyles))
                root.styleSheets.Add(menuStyles);

            startScreen = root.Q<VisualElement>("start-screen");
            levelScreen = root.Q<VisualElement>("level-screen");
            warningScreen = root.Q<VisualElement>("warning-screen");
            newGameButton = root.Q<Button>("new-game-button");
            loadGameButton = root.Q<Button>("load-game-button");
            tutorialButton = root.Q<Button>("tutorial-replay-button");

            newGameButton.clicked += OnNewGamePressed;
            loadGameButton.clicked += ShowLevelPanel;
            if (tutorialButton != null) tutorialButton.clicked += StartTutorial;
            else Debug.LogError("[VRMainMenu] PLAY TUTORIAL button is missing from VRMenu.uxml.", this);
            root.Q<Button>("level-back-button").clicked += ShowStartPanel;
            root.Q<Button>("confirm-new-game-button").clicked += ConfirmNewGame;
            root.Q<Button>("cancel-new-game-button").clicked += ShowStartPanel;

            for (int i = 0; i < levelData.levels.Length; i++)
            {
                Button button = root.Q<Button>("level-" + (i + 1) + "-button");
                if (button == null) continue;
                int levelIndex = i;
                button.clicked += () => SelectLevel(levelIndex);
                levelButtons.Add(button);
            }

            SetControllerPointersVisible(true);
        }

        void Start()
        {
            ShowStartPanel();
            PlaceInFrontOfPlayer();
        }

        void LateUpdate()
        {
            PlaceInFrontOfPlayer();
        }

        public void ShowStartPanel()
        {
            bool hasSave = PuzzleProgressSaveSystem.HasSaveFile;
            loadGameButton.style.display = hasSave ? DisplayStyle.Flex : DisplayStyle.None;
            SetScreen(startScreen);
        }

        void ShowLevelPanel()
        {
            if (!PuzzleProgressSaveSystem.HasSaveFile)
            {
                ShowStartPanel();
                return;
            }

            progress = PuzzleProgressSaveSystem.Load(GetPuzzleId(0));
            RefreshLevelButtons();
            SetScreen(levelScreen);
        }

        void RefreshLevelButtons()
        {
            for (int i = 0; i < levelButtons.Count; i++)
            {
                Button button = levelButtons[i];
                bool exists = i < levelData.levels.Length;
                bool unlocked = exists && IsLevelUnlocked(i);
                button.style.display = exists ? DisplayStyle.Flex : DisplayStyle.None;
                button.SetEnabled(unlocked);

                string levelName = exists && levelData.levels[i] != null
                    ? levelData.levels[i].levelName.ToUpperInvariant()
                    : "LEVEL " + (i + 1);
                button.text = unlocked
                    ? "LEVEL " + (i + 1) + "  •  " + levelName
                    : "LEVEL " + (i + 1) + "  •  LOCKED";
            }
        }

        void OnNewGamePressed()
        {
            if (PuzzleProgressSaveSystem.HasSaveFile)
                SetScreen(warningScreen);
            else
                ConfirmNewGame();
        }

        void ConfirmNewGame()
        {
            PuzzleProgressSaveSystem.DeleteSave();
            progress = PuzzleProgressSaveSystem.CreateNew(GetPuzzleId(0));
            progress.puzzleToLoad = GetPuzzleId(0);
            PuzzleProgressSaveSystem.Save(progress);
            BeginGameplay(0);
        }

        void SelectLevel(int index)
        {
            if (index < 0 || index >= levelData.levels.Length) return;
            if (progress == null) progress = PuzzleProgressSaveSystem.Load(GetPuzzleId(0));
            if (!IsLevelUnlocked(index)) return;

            progress.puzzleToLoad = GetPuzzleId(index);
            PuzzleProgressSaveSystem.Save(progress);
            BeginGameplay(index);
        }

        bool IsLevelUnlocked(int index)
        {
            if (index == 0) return true;
            if (progress == null || progress.completedPuzzles == null) return false;
            return progress.completedPuzzles.Contains(GetPuzzleId(index - 1));
        }

        public void StartTutorial()
        {
            EnsureInitialProgress();
            TutorialProgress.RequestPlay();
            if (tutorialButton != null) tutorialButton.SetEnabled(false);
            SetControllerPointersVisible(false);

            if (loadingScreen != null)
                loadingScreen.LoadScene(gameplaySceneName, "ENTERING TUTORIAL");
            else
                SceneManager.LoadScene(gameplaySceneName);
        }

        void EnsureInitialProgress()
        {
            if (PuzzleProgressSaveSystem.HasSaveFile) return;
            progress = PuzzleProgressSaveSystem.CreateNew(GetPuzzleId(0));
            progress.puzzleToLoad = GetPuzzleId(0);
            PuzzleProgressSaveSystem.Save(progress);
        }

        void BeginGameplay(int index)
        {
            string levelName = levelData.levels[index] != null
                ? levelData.levels[index].levelName.ToUpperInvariant()
                : "LEVEL " + (index + 1);
            SetControllerPointersVisible(false);

            if (loadingScreen != null)
                loadingScreen.LoadScene(gameplaySceneName, "ENTERING " + levelName);
            else
                SceneManager.LoadScene(gameplaySceneName);
        }

        string GetPuzzleId(int index)
        {
            Level level = levelData.levels[index];
            return level != null ? level.GetStableId(index) : "Puzzle_" + (index + 1).ToString("00");
        }

        void SetScreen(VisualElement active)
        {
            SetVisible(startScreen, active == startScreen);
            SetVisible(levelScreen, active == levelScreen);
            SetVisible(warningScreen, active == warningScreen);
            SetVisible(root.Q("pause-screen"), false);
            SetVisible(root.Q("captured-screen"), false);
        }

        static void SetVisible(VisualElement element, bool value)
        {
            if (element != null) element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void PlaceInFrontOfPlayer()
        {
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            if (cameraTransform == null) return;
            transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera +
                                 cameraTransform.up * heightOffset;
            transform.rotation = cameraTransform.rotation * Quaternion.Euler(0f, 180f, 0f);
            transform.localScale = new Vector3(-worldScale, worldScale, worldScale);
        }

        static void SetControllerPointersVisible(bool value)
        {
            Transform[] transforms = FindObjectsOfType<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != null && transforms[i].name == "Menu UI Ray")
                    transforms[i].gameObject.SetActive(value);
        }
    }
}
