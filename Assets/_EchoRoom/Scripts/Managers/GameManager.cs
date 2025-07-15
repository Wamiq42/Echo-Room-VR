using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool enableDebugging = false;  // ✅ debug toggle on top
    [SerializeField] private Transform levelRoot;           // Empty parent in scene where levels will spawn
    [SerializeField] private List<LevelData> levels;        // Assign LevelData assets here in Inspector

    private int _currentLevelIndex = -1;
    private GameObject _currentLevelInstance;

    // ✅ Singleton
    public static GameManager Instance { get; private set; }

    // Events
    public event Action<LevelData> OnLevelLoaded;
    public event Action<LevelData> OnLevelCompleted;

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

    /// <summary>
    /// Load a level by index.
    /// Destroys the previous level instance if it exists.
    /// </summary>
    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Count)
        {
            Log($"Invalid level index: {index}");
            return;
        }

        // Clean up old level
        if (_currentLevelInstance != null)
            Destroy(_currentLevelInstance);

        _currentLevelIndex = index;
        LevelData data = levels[_currentLevelIndex];

        // Spawn new level prefab
        if (data.levelPrefab != null)
        {
            _currentLevelInstance = Instantiate(data.levelPrefab, levelRoot);
            Log($"Loaded level: {data.levelName}");
        }
        else
        {
            Log($"LevelData at index {index} has no prefab assigned.");
        }

        OnLevelLoaded?.Invoke(data);
    }

    /// <summary>
    /// Call when current level is completed.
    /// </summary>
    public void CompleteLevel()
    {
        if (_currentLevelIndex < 0 || _currentLevelIndex >= levels.Count) return;
        LevelData data = levels[_currentLevelIndex];
        Log($"Level completed: {data.levelName}");
        OnLevelCompleted?.Invoke(data);
    }

    /// <summary>
    /// Load the next level in the list.
    /// </summary>
    public void LoadNextLevel()
    {
        int nextIndex = _currentLevelIndex + 1;
        if (nextIndex < levels.Count)
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
