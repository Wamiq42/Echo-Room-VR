using UnityEngine;

/// <summary>
/// Central access point for player input. Can switch between different input sources.
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugging = false;

    [Header("Input Sources")]
    [Tooltip("Drag in a component implementing IPlayerInputSource (e.g., ControllerInputSource).")]
    [SerializeField] private MonoBehaviour currentInputSourceComponent;

    private IPlayerInputSource _currentSource;

    private void Awake()
    {
        if (currentInputSourceComponent != null)
            _currentSource = currentInputSourceComponent as IPlayerInputSource;

        if (_currentSource == null)
            Debug.LogError("[PlayerInputManager] No valid input source assigned!");
    }

    public Vector2 ReadMove() => _currentSource != null ? _currentSource.GetMoveInput() : Vector2.zero;
    public bool ReadSprint() => _currentSource != null && _currentSource.GetSprintInput();
    public bool ReadPing() => _currentSource != null && _currentSource.GetPingInput();
    public bool ReadGrip()
    {
        // For now, reuse your Sprint action if that’s mapped to grip,
        // or assign a specific grip InputActionProperty in ControllerInputSource and expose it.
        return _currentSource != null && _currentSource.GetGripInput();
    }

    private void Update()
    {
        // Example: if you want to debug inputs centrally
        if (!enableDebugging || _currentSource == null) return;

        var move = _currentSource.GetMoveInput();
        bool sprint = _currentSource.GetSprintInput();
        bool ping = _currentSource.GetPingInput();

        if (move != Vector2.zero) Debug.Log($"[PlayerInputManager] Move: {move}");
        if (sprint) Debug.Log("[PlayerInputManager] Sprint active");
        if (ping) Debug.Log("[PlayerInputManager] Ping triggered");
    }
}