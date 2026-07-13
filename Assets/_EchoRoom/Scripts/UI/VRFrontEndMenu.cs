using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoRoom.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(VRPauseMenu), typeof(UIDocument))]
    public sealed class VRFrontEndMenu : MonoBehaviour
    {
        VRPauseMenu pauseMenu;
        VisualElement startScreen;
        VisualElement levelScreen;
        VisualElement warningScreen;
        Button newGameButton;
        Button loadGameButton;
        Button levelBackButton;
        Button confirmNewGameButton;
        Button cancelNewGameButton;
        readonly List<Button> levelButtons = new List<Button>();

        void Awake()
        {
            pauseMenu = GetComponent<VRPauseMenu>();
            VisualElement root = pauseMenu.Root;
            if (root == null)
            {
                Debug.LogError("[VRFrontEndMenu] UI Toolkit root is unavailable.", this);
                enabled = false;
                return;
            }

            startScreen = root.Q<VisualElement>("start-screen");
            levelScreen = root.Q<VisualElement>("level-screen");
            warningScreen = root.Q<VisualElement>("warning-screen");
            newGameButton = root.Q<Button>("new-game-button");
            loadGameButton = root.Q<Button>("load-game-button");
            levelBackButton = root.Q<Button>("level-back-button");
            confirmNewGameButton = root.Q<Button>("confirm-new-game-button");
            cancelNewGameButton = root.Q<Button>("cancel-new-game-button");

            if (!ValidateElements())
            {
                enabled = false;
                return;
            }

            newGameButton.clicked += OnNewGamePressed;
            loadGameButton.clicked += ShowLevelPanel;
            levelBackButton.clicked += ShowStartPanel;
            confirmNewGameButton.clicked += ConfirmNewGame;
            cancelNewGameButton.clicked += ShowStartPanel;

            for (int i = 0; i < 5; i++)
            {
                Button button = root.Q<Button>("level-" + (i + 1) + "-button");
                if (button == null)
                {
                    Debug.LogError("[VRFrontEndMenu] Missing level button " + (i + 1) + ".", this);
                    enabled = false;
                    return;
                }

                int levelIndex = i;
                button.clicked += () => LoadLevel(levelIndex);
                levelButtons.Add(button);
            }

            pauseMenu.RegisterFrontEnd(this);
        }

        void Start()
        {
            ShowStartPanel();
        }

        bool ValidateElements()
        {
            bool valid = startScreen != null && levelScreen != null && warningScreen != null &&
                         newGameButton != null && loadGameButton != null && levelBackButton != null &&
                         confirmNewGameButton != null && cancelNewGameButton != null;
            if (!valid) Debug.LogError("[VRFrontEndMenu] One or more required UI Toolkit elements are missing.", this);
            return valid;
        }

        internal void SetVisible(bool visible)
        {
            if (!visible)
            {
                SetElementVisible(startScreen, false);
                SetElementVisible(levelScreen, false);
                SetElementVisible(warningScreen, false);
                return;
            }

            if (!IsVisible(startScreen) && !IsVisible(levelScreen) && !IsVisible(warningScreen))
                SetScreen(startScreen);
        }

        public void ShowStartPanel()
        {
            if (startScreen == null) return;

            bool hasSave = GameManager.Instance != null
                ? GameManager.Instance.HasSaveFile
                : PuzzleProgressSaveSystem.HasSaveFile;

            loadGameButton.style.display = hasSave ? DisplayStyle.Flex : DisplayStyle.None;
            SetScreen(startScreen);
            pauseMenu.ShowStartMenu();
        }

        void ShowLevelPanel()
        {
            if (GameManager.Instance == null || !GameManager.Instance.HasSaveFile)
            {
                ShowStartPanel();
                return;
            }

            RefreshLevelButtons();
            SetScreen(levelScreen);
        }

        void RefreshLevelButtons()
        {
            GameManager manager = GameManager.Instance;
            for (int i = 0; i < levelButtons.Count; i++)
            {
                Button button = levelButtons[i];
                bool exists = manager != null && i < manager.LevelCount;
                bool unlocked = exists && manager.IsLevelUnlocked(i);
                button.style.display = exists ? DisplayStyle.Flex : DisplayStyle.None;
                button.SetEnabled(unlocked);

                string levelName = exists ? manager.GetLevelDisplayName(i).ToUpperInvariant() : string.Empty;
                button.text = unlocked
                    ? "LEVEL " + (i + 1) + "  •  " + levelName
                    : "LEVEL " + (i + 1) + "  •  LOCKED";
            }
        }

        void OnNewGamePressed()
        {
            if (PuzzleProgressSaveSystem.HasSaveFile)
            {
                SetScreen(warningScreen);
                return;
            }

            ConfirmNewGame();
        }

        void ConfirmNewGame()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.StartNewGame();
            pauseMenu.HideMenu(true);
        }

        void LoadLevel(int index)
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.TryLoadUnlockedLevel(index))
                pauseMenu.HideMenu(true);
            else
                RefreshLevelButtons();
        }

        void SetScreen(VisualElement active)
        {
            SetElementVisible(startScreen, active == startScreen);
            SetElementVisible(levelScreen, active == levelScreen);
            SetElementVisible(warningScreen, active == warningScreen);
        }

        static void SetElementVisible(VisualElement element, bool visible)
        {
            if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static bool IsVisible(VisualElement element)
        {
            return element != null && element.resolvedStyle.display != DisplayStyle.None;
        }
    }
}
