using UnityEngine;

/// <summary>
/// Place this on a trigger collider just past a level's exit door. When the player walks
/// through it, the current level is marked complete and the next one loads.
///
/// If <see cref="requiredPuzzle"/> is assigned, the trigger only fires once that puzzle is
/// solved; otherwise it assumes the exit door already gates access (you can't reach the
/// trigger until the door has opened).
/// </summary>
[RequireComponent(typeof(Collider))]
public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private bool enableDebugging = false;

    [Tooltip("Optional. If set, the exit only triggers once this puzzle is solved.")]
    [SerializeField] private PuzzleBase requiredPuzzle;

    [Tooltip("Tag used to identify the player rig. 'Player' is a Unity built-in tag.")]
    [SerializeField] private string playerTag = "Player";

    private bool _fired;

    // Default the attached collider to a trigger when first added in the editor.
    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_fired) return;
        if (!IsPlayer(other)) return;

        if (requiredPuzzle != null && !requiredPuzzle.IsSolved)
        {
            Log("Player reached the exit, but the puzzle isn't solved yet.");
            return;
        }

        _fired = true;
        Log("Player exited the level. Advancing.");

        if (GameManager.Instance != null)
            GameManager.Instance.NotifyLevelExitReached();
        else
            Debug.LogWarning("[LevelExitTrigger] No GameManager.Instance found.");
    }

    private bool IsPlayer(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
            return true;

        // Fallback: XR rigs typically drive a CharacterController.
        return other.GetComponentInParent<CharacterController>() != null;
    }

    private void Log(string message)
    {
        if (enableDebugging) Debug.Log($"[LevelExitTrigger] {message}");
    }
}
