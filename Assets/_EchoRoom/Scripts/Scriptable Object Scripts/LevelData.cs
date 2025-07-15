using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public Level[] levels;
}

[Serializable]
public class Level
{
    [Header("Level Settings")]
    public string levelName = "New Level";
    public GameObject levelPrefab;     // The prefab that represents this level

    [Header("Optional")]
    public Sprite previewImage;        // For UI or menus later
    public string description;         // Notes, difficulty, etc.
}