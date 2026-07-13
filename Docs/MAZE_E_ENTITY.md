# Maze E Entity - Setup and Behavior Log

## Purpose

The Maze E Entity is a harmless patrol enemy until the player uses the echo ping. A ping temporarily attracts the Entity and creates a dangerous search area around the ping location.

## Scene Setup

The Entity GameObject contains:

- `PingAttractedEntity`
- `NavMeshAgent`
- `AudioSource`

It also needs:

- A baked `NavMeshSurface` on the maze
- The two patrol waypoint transforms
- The player's `PingEmitter`
- The `XR Origin (XR Rig)` as the player root
- An optional reset point
- An optional looping proximity sound clip

The main behavior is implemented in:

`Assets/_EchoRoom/Scripts/AI/PingAttractedEntity.cs`

## How It Works

1. **Patrol:** Before any ping, the Entity moves between its assigned waypoints and waits briefly at each one.
2. **Hear ping:** The Entity subscribes to `PingEmitter.OnPingEmitted`. The newest ping position becomes its destination and restarts the danger timer.
3. **Search:** The Entity travels to the ping location using its `NavMeshAgent`.
4. **Capture check:** When it reaches the ping radius, it checks whether the player's body or VR camera is still inside `captureRange` of the original ping position.
5. **Player found:** The current capture flow first tries to show the captured menu. If no captured menu handles it, the Entity asks `GameManager` to restart the level. If GameManager is unavailable, it moves the player to the configured reset point or cached starting position.
6. **Player escaped:** If the player is outside the radius when the Entity arrives, or the danger timer expires, the Entity returns to patrol.
7. **New ping:** A later ping immediately replaces the old target and restarts the timer.

## Adjustable Settings

- `Move Speed`: Entity movement speed.
- `Patrol Wait Time`: Pause at each waypoint.
- `Waypoint Reach Distance`: Distance considered close enough to a waypoint.
- `Capture Range`: Radius around the ping where the player can be found.
- `Ping Danger Timer`: How long the Entity investigates a ping.
- `Reset On Capture`: Enables the capture reset behavior.
- `Respawn Player With Game Manager`: Uses the level restart flow when possible.
- `Max Audio Distance`: Distance where the proximity sound begins increasing.
- `Min Volume` / `Max Volume`: Proximity sound volume limits.
- `Enable Debug Logs`: Prints Entity state changes in the Unity Console.

## Proximity Audio

The `AudioSource` volume increases as the Entity approaches the player. Assign a looping sound clip to the Entity's AudioSource; the script starts it automatically when a clip is present.

## Useful Debug Messages

- `Heard ping and created search radius.`
- `Ping search timer ended. Returning to patrol.`
- `Reached ping radius, player not found. Returning to patrol.`
- `Player captured.`

## Test Checklist

- Confirm the NavMesh is baked and the Entity starts on it.
- Confirm both patrol waypoints are assigned.
- Start the scene without pinging and verify patrol movement.
- Ping in another corridor and verify the Entity changes destination.
- Stay inside the ping radius and verify capture occurs.
- Leave the radius before arrival and verify the Entity returns to patrol.
- Ping twice and verify the second ping replaces the first target.
- Confirm the captured menu or level restart returns the player to the intended start state.
