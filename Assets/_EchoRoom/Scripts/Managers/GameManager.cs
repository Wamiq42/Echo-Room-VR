using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool enableDebugging = false;   // ✅ debug toggle on top
    [SerializeField] private Transform levelRoot;            // Empty parent in scene where levels will spawn
    [SerializeField] private LevelData levelData;            // ✅ single asset that holds all level info
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInputManager playerInputManager;
  
    private int _currentLevelIndex = -1;
    private GameObject _currentLevelInstance;

    // ✅ Singleton
    public static GameManager Instance { get; private set; }
    
    //properties.
    public PlayerInputManager PlayerInputManager => playerInputManager;
    
    // Events
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
        DontDestroyOnLoad(gameObject); // stays alive through scene transitions if needed
    }

    private void Start()
    {
        LoadLevel(0);
    }

    /// <summary>
    /// Load a level by index.
    /// Destroys the previous level instance if it exists.
    /// </summary>
    public void LoadLevel(int index)
    {
        if (levelData == null || levelData.levels == null || index < 0 || index >= levelData.levels.Length)
        {
            Log($"Invalid level index: {index}");
            return;
        }

        // Clean up old level
        if (_currentLevelInstance != null)
            Destroy(_currentLevelInstance);

        _currentLevelIndex = index;
        Level data = levelData.levels[_currentLevelIndex];

        // Spawn new level prefab
        if (data.levelPrefab != null)
        {
            // After spawning the level
            _currentLevelInstance = Instantiate(data.levelPrefab, levelRoot);
            Log($"Loaded level: {data.levelName}");

            // Find LevelSetup on this instance
            LevelSetup setup = _currentLevelInstance.GetComponent<LevelSetup>();
            if (setup != null && setup.playerSpawnPoint != null)
            {
                playerController.MovePlayerToSpawn(setup.playerSpawnPoint.position, setup.playerSpawnPoint.rotation);
            }
        }
        else
        {
            Log($"Level at index {index} has no prefab assigned.");
        }

        OnLevelLoaded?.Invoke(data);
    }

    /// <summary>
    /// Call when current level is completed.
    /// </summary>
    public void CompleteLevel()
    {
        if (_currentLevelIndex < 0 || _currentLevelIndex >= levelData.levels.Length) return;
        Level data = levelData.levels[_currentLevelIndex];
        Log($"Level completed: {data.levelName}");
        OnLevelCompleted?.Invoke(data);
    }

    /// <summary>
    /// Load the next level in the list.
    /// </summary>
    public void LoadNextLevel()
    {
        int nextIndex = _currentLevelIndex + 1;
        if (levelData != null && nextIndex < levelData.levels.Length)
        {
            LoadLevel(nextIndex);
        }
        else
        {
            Log("No more levels available!");
            // TODO: Handle end of game or loop
        }
    }

    public void RestartLevel()
    {
        LoadLevel(_currentLevelIndex);
    }

    // ---------- Debugging ----------
    private void Log(string message)
    {
        if (enableDebugging)
            Debug.Log($"[GameManager] {message}");
    }
}
