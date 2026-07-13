using UnityEngine;

public class DoorPuzzleBinder : MonoBehaviour
{
    [SerializeField] private bool enableDebugging = false;   // debug toggle on top
    [SerializeField] private PuzzleBase puzzle;             // drag your puzzle controller here in Inspector

    private Door _door;

    void Awake()
    {
        _door = GetComponent<Door>();
        if (puzzle != null)
        {
            puzzle.OnPuzzleSolved += OnPuzzleSolved;
            Log($"Bound {puzzle.name} to {name}");
        }
        else
        {
            Log($"No puzzle assigned to {name}");
        }
    }

    private void Update()
    {
        if (puzzle != null && puzzle.IsSolved)
            OpenDoorIfNeeded();
    }

    private void OnDestroy()
    {
        if (puzzle != null)
            puzzle.OnPuzzleSolved -= OnPuzzleSolved;
    }

    private void OnPuzzleSolved(PuzzleBase p)
    {
        Log($"Puzzle solved! Opening {name}");
        OpenDoorIfNeeded();
    }

    private void OpenDoorIfNeeded()
    {
        if (_door != null && !_door.open)
            _door.Open();
    }

    // ---------- Debugging ----------
    private void Log(string message)
    {
        if (enableDebugging) Debug.Log($"[DoorPuzzleBinder] {message}");
    }
}
