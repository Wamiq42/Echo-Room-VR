using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public sealed class PuzzleProgressData
{
    public List<string> completedPuzzles = new List<string>();
    public string puzzleToLoad = string.Empty;
}

public static class PuzzleProgressSaveSystem
{
    private const string SaveFileName = "puzzle-progress.json";

    public static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
    }

    public static bool HasSaveFile
    {
        get { return File.Exists(SavePath); }
    }

    public static PuzzleProgressData CreateNew(string firstPuzzleId)
    {
        return new PuzzleProgressData
        {
            completedPuzzles = new List<string>(),
            puzzleToLoad = firstPuzzleId ?? string.Empty
        };
    }

    public static PuzzleProgressData Load(string firstPuzzleId)
    {
        if (!File.Exists(SavePath))
            return CreateNew(firstPuzzleId);

        try
        {
            string json = File.ReadAllText(SavePath);
            PuzzleProgressData data = JsonUtility.FromJson<PuzzleProgressData>(json);
            if (data == null)
                return CreateNew(firstPuzzleId);

            if (data.completedPuzzles == null)
                data.completedPuzzles = new List<string>();

            if (data.puzzleToLoad == null)
                data.puzzleToLoad = string.Empty;

            return data;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[PuzzleProgressSaveSystem] Could not read the save file. Starting from the first puzzle. " + exception.Message);
            return CreateNew(firstPuzzleId);
        }
    }

    public static bool Save(PuzzleProgressData data)
    {
        if (data == null)
            return false;

        try
        {
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string temporaryPath = SavePath + ".tmp";
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(temporaryPath, json);
            File.Copy(temporaryPath, SavePath, true);
            File.Delete(temporaryPath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("[PuzzleProgressSaveSystem] Could not save puzzle progress. " + exception.Message);
            return false;
        }
    }

    public static void MarkCompleted(PuzzleProgressData data, string completedPuzzleId, string nextPuzzleId)
    {
        if (data == null)
            return;

        if (!string.IsNullOrWhiteSpace(completedPuzzleId) && !data.completedPuzzles.Contains(completedPuzzleId))
            data.completedPuzzles.Add(completedPuzzleId);

        data.puzzleToLoad = nextPuzzleId ?? string.Empty;
    }

    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);

            string temporaryPath = SavePath + ".tmp";
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
        catch (Exception exception)
        {
            Debug.LogError("[PuzzleProgressSaveSystem] Could not delete puzzle progress. " + exception.Message);
        }
    }
}
