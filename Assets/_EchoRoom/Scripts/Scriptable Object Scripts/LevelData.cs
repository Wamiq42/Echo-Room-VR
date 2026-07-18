using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Tutorial Baked Lighting")]
    [Tooltip("Captured lightmaps and renderer bindings for the tutorial prefab.")]
    public LevelLightingData tutorialBakedLighting;

    public Level[] levels;
}

[Serializable]
public class Level
{
    [Header("Level Settings")]
    [Tooltip("Stable ID written to the external progress file. Do not change it after release.")]
    public string puzzleId = "Puzzle_01";
    public string levelName = "New Level";
    public GameObject levelPrefab;

    [Header("Baked Lighting")]
    [Tooltip("Captured lightmaps and renderer bindings for this prefab level.")]
    public LevelLightingData bakedLighting;

    [Header("Optional")]
    public Sprite previewImage;
    public string description;

    public string GetStableId(int fallbackIndex)
    {
        if (!string.IsNullOrWhiteSpace(puzzleId))
            return puzzleId;

        if (!string.IsNullOrWhiteSpace(levelName))
            return levelName;

        return "Puzzle_" + (fallbackIndex + 1).ToString("00");
    }
}
