using UnityEngine;

public abstract class PuzzleBase : MonoBehaviour
{
    [SerializeField] protected bool enableDebugging = false;   // debug toggle always on top

    public bool IsSolved { get; protected set; }

    public delegate void PuzzleSolvedEvent(PuzzleBase puzzle);
    public event PuzzleSolvedEvent OnPuzzleSolved;

    /// <summary>
    /// Call this when the puzzle conditions are met.
    /// </summary>
    protected void MarkSolved()
    {
        if (IsSolved) return;
        IsSolved = true;
        if (enableDebugging) Debug.Log($"[{GetType().Name}] Puzzle solved!");
        OnPuzzleSolved?.Invoke(this);
    }

    /// <summary>
    /// Reset puzzle state and elements.
    /// </summary>
    public abstract void ResetPuzzle();
}