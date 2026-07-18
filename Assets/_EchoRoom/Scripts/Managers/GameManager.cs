using System;
using System.Collections;
using EchoRoom.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool enableDebugging = false;
    [SerializeField] private Transform levelRoot;
    [SerializeField] private LevelData levelData;
    [SerializeField] private bool useSceneLevelInstance = false;
    [SerializeField] private GameObject sceneLevelInstance;
    [SerializeField] private Transform scenePlayerSpawnPoint;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInputManager playerInputManager;

    [Header("Puzzle Progress")]
    [SerializeField] private bool loadSavedProgress = true;
    [SerializeField] private VRScreenFade screenFade;

    [Header("Scene Flow")]
    [SerializeField] private VRLoadingScreen loadingScreen;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialLevelPrefab;

    private int _currentLevelIndex = -1;
    private GameObject _currentLevelInstance;
    private bool _currentLevelIsSceneInstance;
    private bool _isTransitioning;
    private PuzzleProgressData _progress;
    private bool _tutorialLevelActive;

    public static GameManager Instance { get; private set; }
    public PlayerInputManager PlayerInputManager => playerInputManager;
    public string ProgressFilePath => PuzzleProgressSaveSystem.SavePath;
    public bool HasSaveFile => PuzzleProgressSaveSystem.HasSaveFile;
    public int LevelCount => HasValidLevelData() ? levelData.levels.Length : 0;
    public bool IsTutorialLevelActive => _tutorialLevelActive;
    public GameObject CurrentLevelInstance => _currentLevelInstance;
    public GameObject TutorialLevelPrefab => tutorialLevelPrefab;

    public event Action<Level> OnLevelLoaded;
    public event Action<Level> OnLevelCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PrefabLightmapRuntime.InitializeSceneLighting();
        if (screenFade == null) screenFade = GetComponent<VRScreenFade>();
        if (screenFade == null) screenFade = gameObject.AddComponent<VRScreenFade>();
        if (loadingScreen == null) loadingScreen = FindObjectOfType<VRLoadingScreen>(true);
    }

    private void Start()
    {
        if (!HasValidLevelData())
        {
            Debug.LogError("[GameManager] No puzzles are configured in LevelData.");
            return;
        }

        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        if (TutorialProgress.IsTutorialRequested)
        {
            if (tutorialLevelPrefab == null)
            {
                Debug.LogError("[GameManager] Tutorial was requested but no tutorial prefab is assigned.");
                yield break;
            }

            if (screenFade != null) screenFade.SetOpacity(0f);
            if (loadingScreen != null)
                yield return loadingScreen.CoverPrefabSwap("ENTERING TUTORIAL", LoadTutorialLevel);
            else
                LoadTutorialLevel();
            yield break;
        }

        string firstPuzzleId = GetPuzzleId(0);
        _progress = loadSavedProgress
            ? PuzzleProgressSaveSystem.Load(firstPuzzleId)
            : PuzzleProgressSaveSystem.CreateNew(firstPuzzleId);

        int levelIndex = ResolveSavedLevelIndex();
        if (screenFade != null) screenFade.SetOpacity(0f);

        if (loadingScreen != null)
            yield return loadingScreen.CoverPrefabSwap("ENTERING " + GetLevelDisplayName(levelIndex).ToUpperInvariant(),
                () => LoadLevel(levelIndex));
        else
            LoadLevel(levelIndex);
    }

    private void LoadTutorialLevel()
    {
        PrefabLightmapRuntime.RestoreSceneLighting();
        if (_currentLevelInstance != null && !_currentLevelIsSceneInstance)
            Destroy(_currentLevelInstance);

        _currentLevelIndex = -1;
        _tutorialLevelActive = true;
        _currentLevelIsSceneInstance = false;
        _currentLevelInstance = levelRoot != null
            ? Instantiate(tutorialLevelPrefab, levelRoot)
            : Instantiate(tutorialLevelPrefab);
        _currentLevelInstance.name = tutorialLevelPrefab.name;
        _currentLevelInstance.SetActive(true);

        if (levelData != null && levelData.tutorialBakedLighting != null)
            PrefabLightmapRuntime.Apply(_currentLevelInstance, levelData.tutorialBakedLighting);

        MovePlayerToSpawn(ResolveSpawnPoint(_currentLevelInstance));
        Log("Loaded tutorial through GameManager.");
        GetComponent<EchoFogController>()?.ApplyTutorialFog();
    }

    public void LoadLevel(int index)
    {
        if (!HasValidLevelData() || index < 0 || index >= levelData.levels.Length)
        {
            Log("Invalid level index: " + index);
            return;
        }

        PrefabLightmapRuntime.RestoreSceneLighting();
        if (_currentLevelInstance != null && !_currentLevelIsSceneInstance)
            Destroy(_currentLevelInstance);

        _tutorialLevelActive = false;
        _currentLevelIndex = index;
        Level data = levelData.levels[_currentLevelIndex];
        _currentLevelInstance = null;
        _currentLevelIsSceneInstance = false;

        if (useSceneLevelInstance && sceneLevelInstance != null)
        {
            _currentLevelInstance = sceneLevelInstance;
            _currentLevelIsSceneInstance = true;
            _currentLevelInstance.SetActive(true);
            Log("Loaded scene level: " + data.levelName);
        }
        else if (data.levelPrefab != null)
        {
            _currentLevelInstance = levelRoot != null
                ? Instantiate(data.levelPrefab, levelRoot)
                : Instantiate(data.levelPrefab);

            _currentLevelInstance.SetActive(true);
            Log("Loaded puzzle: " + GetPuzzleId(index));
        }
        else
        {
            Debug.LogError("[GameManager] Puzzle '" + GetPuzzleId(index) + "' has no prefab assigned.");
            return;
        }

        if (data.bakedLighting != null)
            PrefabLightmapRuntime.Apply(_currentLevelInstance, data.bakedLighting);

        MovePlayerToSpawn(ResolveSpawnPoint(_currentLevelInstance));
        OnLevelLoaded?.Invoke(data);
    }

    public void LoadFirstLevelAfterTutorial()
    {
        if (!_tutorialLevelActive || _isTransitioning) return;
        TutorialProgress.Complete();
        StartCoroutine(LoadFirstLevelAfterTutorialRoutine());
    }

    private IEnumerator LoadFirstLevelAfterTutorialRoutine()
    {
        _isTransitioning = true;
        try
        {
            if (loadingScreen != null)
                yield return loadingScreen.CoverPrefabSwap("ENTERING " + GetLevelDisplayName(0).ToUpperInvariant(), () => LoadLevel(0));
            else
                LoadLevel(0);
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    public void NotifyLevelExitReached()
    {
        if (_isTransitioning || _currentLevelIndex < 0) return;
        StartCoroutine(TransitionToNextPuzzle());
    }

    private IEnumerator TransitionToNextPuzzle()
    {
        _isTransitioning = true;
        try
        {
            int completedIndex = _currentLevelIndex;
            int nextIndex = completedIndex + 1;
            bool hasNextPuzzle = HasValidLevelData() && nextIndex < levelData.levels.Length;

            CompleteLevel();
            if (_progress == null) _progress = PuzzleProgressSaveSystem.CreateNew(GetPuzzleId(0));

            string completedPuzzleId = GetPuzzleId(completedIndex);
            string nextPuzzleId = hasNextPuzzle ? GetPuzzleId(nextIndex) : string.Empty;
            PuzzleProgressSaveSystem.MarkCompleted(_progress, completedPuzzleId, nextPuzzleId);
            PuzzleProgressSaveSystem.Save(_progress);

            if (hasNextPuzzle)
            {
                if (loadingScreen != null)
                    yield return loadingScreen.CoverPrefabSwap(
                        "TUNING " + GetLevelDisplayName(nextIndex).ToUpperInvariant(),
                        () => LoadLevel(nextIndex));
                else
                {
                    yield return null;
                    LoadLevel(nextIndex);
                }

                yield break;
            }

            Log("All configured puzzles are complete.");
            if (loadingScreen != null)
            {
                yield return loadingScreen.ShowThankYouThenLoad(mainMenuSceneName);
                yield break;
            }

            yield return new WaitForSecondsRealtime(5f);
            SceneManager.LoadScene(mainMenuSceneName);
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    public void CompleteLevel()
    {
        if (!HasValidLevelData() || _currentLevelIndex < 0 || _currentLevelIndex >= levelData.levels.Length)
            return;

        Level data = levelData.levels[_currentLevelIndex];
        Log("Puzzle completed: " + GetPuzzleId(_currentLevelIndex));
        OnLevelCompleted?.Invoke(data);
    }

    public void LoadNextLevel()
    {
        if (!_isTransitioning) NotifyLevelExitReached();
    }

    public void RestartLevel()
    {
        if (_isTransitioning) return;
        if (_tutorialLevelActive)
        {
            StartCoroutine(RestartTutorialRoutine());
            return;
        }
        if (_currentLevelIndex < 0) return;
        if (useSceneLevelInstance && sceneLevelInstance != null)
        {
            ReloadActiveScene();
            return;
        }

        StartCoroutine(RestartLevelRoutine());
    }

    private IEnumerator RestartTutorialRoutine()
    {
        _isTransitioning = true;
        try
        {
            if (loadingScreen != null)
                yield return loadingScreen.CoverPrefabSwap("RESTARTING TUTORIAL", LoadTutorialLevel);
            else
                LoadTutorialLevel();
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private IEnumerator RestartLevelRoutine()
    {
        _isTransitioning = true;
        int index = _currentLevelIndex;
        try
        {
            if (loadingScreen != null)
                yield return loadingScreen.CoverPrefabSwap(
                    "RESTARTING " + GetLevelDisplayName(index).ToUpperInvariant(),
                    () => LoadLevel(index));
            else
                LoadLevel(index);
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    public void ReturnToMainMenu()
    {
        if (_isTransitioning) return;
        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        _isTransitioning = true;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (loadingScreen != null)
        {
            yield return loadingScreen.LoadSceneRoutine(mainMenuSceneName, "RETURNING TO MAIN MENU");
            yield break;
        }

        if (screenFade != null)
            yield return screenFade.FadeOut();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void StartNewGame()
    {
        if (!HasValidLevelData()) return;
        PuzzleProgressSaveSystem.DeleteSave();
        _progress = PuzzleProgressSaveSystem.CreateNew(GetPuzzleId(0));
        PuzzleProgressSaveSystem.Save(_progress);
        StartCoroutine(LoadSelectedLevelRoutine(0));
    }

    public string GetLevelDisplayName(int index)
    {
        if (!HasValidLevelData() || index < 0 || index >= levelData.levels.Length)
            return "LEVEL " + (index + 1);

        Level level = levelData.levels[index];
        return level != null && !string.IsNullOrWhiteSpace(level.levelName)
            ? level.levelName
            : "LEVEL " + (index + 1);
    }

    public bool IsLevelUnlocked(int index)
    {
        if (!HasValidLevelData() || index < 0 || index >= levelData.levels.Length) return false;
        if (index == 0) return true;
        if (_progress == null) _progress = PuzzleProgressSaveSystem.Load(GetPuzzleId(0));
        return _progress.completedPuzzles != null &&
               _progress.completedPuzzles.Contains(GetPuzzleId(index - 1));
    }

    public bool TryLoadUnlockedLevel(int index)
    {
        if (!HasSaveFile || !IsLevelUnlocked(index) || _isTransitioning) return false;
        _progress.puzzleToLoad = GetPuzzleId(index);
        PuzzleProgressSaveSystem.Save(_progress);
        StartCoroutine(LoadSelectedLevelRoutine(index));
        return true;
    }

    private IEnumerator LoadSelectedLevelRoutine(int index)
    {
        _isTransitioning = true;
        try
        {
            if (loadingScreen != null)
                yield return loadingScreen.CoverPrefabSwap(
                    "ENTERING " + GetLevelDisplayName(index).ToUpperInvariant(),
                    () => LoadLevel(index));
            else
                LoadLevel(index);
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    public void RespawnPlayerAtSpawn()
    {
        Transform spawnPoint = scenePlayerSpawnPoint;
        if (spawnPoint == null)
        {
            GameObject levelObject = _currentLevelInstance != null ? _currentLevelInstance : sceneLevelInstance;
            spawnPoint = ResolveSpawnPoint(levelObject);
        }

        if (spawnPoint != null)
        {
            MovePlayerToSpawn(spawnPoint);
            Log("Player respawned at puzzle spawn.");
        }
        else Log("No player spawn point is configured.");
    }

    private int ResolveSavedLevelIndex()
    {
        if (_progress == null) return 0;
        if (string.IsNullOrWhiteSpace(_progress.puzzleToLoad))
        {
            if (_progress.completedPuzzles != null && _progress.completedPuzzles.Count > 0)
                return Mathf.Max(0, levelData.levels.Length - 1);
            return 0;
        }

        for (int i = 0; i < levelData.levels.Length; i++)
            if (string.Equals(GetPuzzleId(i), _progress.puzzleToLoad, StringComparison.Ordinal))
                return i;

        Debug.LogWarning("[GameManager] Saved puzzle ID '" + _progress.puzzleToLoad +
                         "' is not configured. Loading the first puzzle.");
        _progress.puzzleToLoad = GetPuzzleId(0);
        return 0;
    }

    private string GetPuzzleId(int index)
    {
        if (!HasValidLevelData() || index < 0 || index >= levelData.levels.Length) return string.Empty;
        Level level = levelData.levels[index];
        return level != null ? level.GetStableId(index) : "Puzzle_" + (index + 1).ToString("00");
    }

    private bool HasValidLevelData()
    {
        return levelData != null && levelData.levels != null && levelData.levels.Length > 0;
    }

    private Transform ResolveSpawnPoint(GameObject levelObject)
    {
        if (scenePlayerSpawnPoint != null && useSceneLevelInstance) return scenePlayerSpawnPoint;
        LevelSetup setup = levelObject != null ? levelObject.GetComponent<LevelSetup>() : null;
        return setup != null ? setup.playerSpawnPoint : null;
    }

    private void MovePlayerToSpawn(Transform spawnPoint)
    {
        if (playerController != null && spawnPoint != null)
            playerController.MovePlayerToSpawn(spawnPoint.position, spawnPoint.rotation);
        else if (spawnPoint == null)
            Debug.LogWarning("[GameManager] The loaded puzzle has no StartCheckpoint.");
    }

    private void ReloadActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex >= 0) SceneManager.LoadScene(activeScene.buildIndex);
        else if (!string.IsNullOrEmpty(activeScene.name)) SceneManager.LoadScene(activeScene.name);
        else Log("Cannot restart scene because it is not saved or added to Build Settings.");
    }

    private void OnDestroy()
    {
        PrefabLightmapRuntime.RestoreSceneLighting();
        if (Instance == this) Instance = null;
    }

    private void Log(string message)
    {
        if (enableDebugging) Debug.Log("[GameManager] " + message);
    }
}
