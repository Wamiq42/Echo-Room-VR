using UnityEngine;

public interface IPlayerInputSource
{
    /// <summary>
    /// Returns the 2D movement input (joystick or gesture-based).
    /// </summary>
    Vector2 GetMoveInput();

    /// <summary>
    /// Returns true while sprint input is active.
    /// </summary>
    bool GetSprintInput();

    /// <summary>
    /// Returns true only on the frame ping input is triggered.
    /// </summary>
    bool GetPingInput();
}