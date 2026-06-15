# Echo Room VR — Codebase Analysis

> Snapshot of the project's own code (everything under `Assets/_EchoRoom/`).
> Third-party packages (Oculus, Meta XR, XR Toolkit samples, door/texture packs,
> SimpleSonarShader) are excluded except where the game depends on them.

---

## 1. What the game is

A **Meta Quest (Oculus / OpenXR) puzzle game** built in **Unity 2022.3.56f1 (URP)**
around an **echolocation** mechanic. The world is dark/obscured; the player "pings"
— by controller button **or by making noise into the headset mic** — and a sonar
pulse sweeps outward, briefly revealing geometry via a custom shader. The player
navigates by sound + light, finds buttons, and presses them to solve puzzles that
open doors and advance levels.

Status: active early development (branch `Development-Phase`). Core sensory loop
works; progression and content breadth are incomplete.

---

## 2. Tech stack

| Area | Choice |
|------|--------|
| Engine | Unity 2022.3.56f1 LTS |
| Render | Universal Render Pipeline 14.0.11 |
| VR / XR | Meta XR SDK 77, OpenXR 1.14, XR Interaction Toolkit 2.6.4, XR Hands 1.6 |
| Reveal FX | `SimpleSonarShader` (MadeByProfessorOakie) driven by `EchoPulseController` |
| Geometry | ProBuilder 5.2 (blocky primitives) |
| Environment art | Third-party door packs + stylized texture packs |
| Scenes | One scene `MainScene.unity`; levels are prefabs loaded at runtime |

---

## 3. Architecture overview

Code is well-organized under `Assets/_EchoRoom/Scripts/`:

```
Base/         BaseInteractable, PuzzleBase            (abstract base classes)
Controller/   PlayerController, EchoPulseController,  (runtime systems)
              EchoPuzzleController, EchoSoundController,
              DynamicSprintController
Interactables/ EchoButtonInteractable, HandPressCollider
Interface/    IPlayerInputSource, IEchoInteractable, IPuzzleElement
Managers/     GameManager (singleton), PlayerInputManager
ScriptableObj/ LevelData (data-driven level list)
+ PingEmitter, MicPingTrigger, Door, DoorPuzzleBinder, LevelSetup, ControllerInputSource
```

Good patterns in use: **interfaces**, **abstract bases**, **events**, a
**central input facade**, and a **ScriptableObject-driven level system**.

### Input flow (decoupled — easy to swap controller → hands)

```
ControllerInputSource (reads XR InputActions) --implements--> IPlayerInputSource
        |
        v
PlayerInputManager (central facade)
        |
        +--> PingEmitter        (ReadPing)
        +--> DynamicSprintController (ReadSprint)
        +--> EchoButtonInteractable  (ReadGrip)
```

### Ping / echo flow (the heart of the game)

```
Button press  /  Mic noise  /  SPACE (editor only)
        |
        v
PingEmitter.EmitPing()
   |-- OverlapSphere --> IEchoInteractable.OnPingHit()  (flash + audio on targets)
   |-- OnPingEmitted --> PlayerController --> EchoPulseController (sonar shader sweep)
   |-- EmitDirectionalEcho() --SphereCast--> delayed spatial echo audio
   |        (delay = distance / 343 m/s  -> acoustically correct)
   |-- FlashLight()  + cooldown until echo finishes
```

`MicPingTrigger` reading the real microphone to fire pings is the standout
creative mechanic ("shout to see").

### Puzzle / progression flow

```
EchoButtonInteractable --static OnAnyButtonPressed--> EchoPuzzleController (counts hits)
        | all targets hit
        v
PuzzleBase.MarkSolved --event--> DoorPuzzleBinder --> Door.OpenDoor()

GameManager (singleton) --> LevelData (SO) --> spawn level prefab --> LevelSetup.playerSpawnPoint
```

---

## 4. File-by-file summary

| File | Role |
|------|------|
| `Managers/GameManager.cs` | Singleton. Loads levels from `LevelData`, spawns prefab, moves player to spawn, fires `OnLevelLoaded`/`OnLevelCompleted`. |
| `Managers/PlayerInputManager.cs` | Facade over the active `IPlayerInputSource`. Exposes Read{Move,Sprint,Ping,Grip}. |
| `ControllerInputSource.cs` | Reads XR InputActions (move/sprint/ping/grip). Implements `IPlayerInputSource`. |
| `PingEmitter.cs` | Core ping: overlap detection, directional echo spherecast, spatial echo audio, flash light, cooldown. |
| `MicPingTrigger.cs` | Listens to the mic; loud noise -> requests a ping (respects cooldown). |
| `Controller/EchoPulseController.cs` | Drives the sonar shader: animates `_PingRadius` outward then fades, across cached renderers. |
| `Controller/EchoSoundController.cs` | Spatial 3D echo sound; self-destructs after lifetime. |
| `Controller/EchoPuzzleController.cs` | Counts button presses; solves when all targets hit. Extends `PuzzleBase`. |
| `Controller/PlayerController.cs` | Bridges `PingEmitter.OnPingEmitted` -> `EchoPulseController`; moves XR rig to spawn. |
| `Controller/DynamicSprintController.cs` | Toggles move speed on the XRI `DynamicMoveProvider` based on sprint input. |
| `Interactables/EchoButtonInteractable.cs` | Press-to-activate button; animates depress; fires UnityEvent + static event. |
| `Base/BaseInteractable.cs` | Shared idle/flash material + audio on ping hit. |
| `Base/PuzzleBase.cs` | Abstract puzzle: `IsSolved`, `MarkSolved`, `OnPuzzleSolved`, `ResetPuzzle`. |
| `Door.cs` / `DoorPuzzleBinder.cs` | Door open/close animation; binder opens door when its puzzle solves. |
| `ScriptableObject Scripts/LevelData.cs` | `LevelData` SO holding `Level[]` (name, prefab, preview, description). |
| `LevelSetup.cs` | Per-level component exposing the player spawn transform. |
| Interfaces | `IPlayerInputSource`, `IEchoInteractable`, `IPuzzleElement`. |

---

## 5. Bugs & issues found

Ordered by impact.

1. **No level progression wiring.** `GameManager.CompleteLevel()` / `LoadNextLevel()`
   are never called when a door opens. A solved puzzle opens the door, but nothing
   detects the player walking through, and nothing advances to the next level.
   *(Blocks an end-to-end playable loop.)*

2. **Button reset is a stub.** `EchoButtonInteractable._hasTriggered` latches `true`
   forever and `ResetElement()` is empty, so `EchoPuzzleController.ResetPuzzle()`
   doesn't actually reset buttons. Replaying a level leaves buttons stuck.

3. **`OnPingEmitted` fires inside the overlap loop** (`PingEmitter.cs`). With N nearby
   colliders the sonar pulse triggers N times per ping. Hoist it out of the `foreach`.

4. **Startup null-safety in `PingEmitter.Awake()`.** `echoSoundPrefab.TryGetComponent(...)`
   and `audioSource.clip.length` assume the prefab and its clip are assigned -> NRE if unset.

5. **Editor-only NRE risk.** `Keyboard.current.spaceKey` (`PingEmitter.Update`, behind
   `#if UNITY_EDITOR`) throws if no keyboard device is present. Guard with
   `Keyboard.current != null`.

6. **`Camera.main` every ping** (`EmitDirectionalEcho`). Does a tag search each call; cache it.

7. **`EchoPulseController` caches renderers only in `Awake`.** Renderers in level prefabs
   spawned later by `GameManager` won't pulse. Re-cache on level load.

8. **`EchoButtonInteractable.Awake()` hard-depends on `GameManager.Instance`.** NRE if a
   button ever exists without a GameManager (ordering is currently OK because buttons live
   in spawned level prefabs).

9. **Minor:** `HandPressCollider` is an empty marker; `DynamicSprintController` double
   null-checks `inputManager`; `Door` uses `Slerp` with a hard-coded `5 *` factor that
   ignores `smooth` partially.

---

## 6. Assessment

**Strong foundation, incomplete game.** Above-average code quality for a solo VR
project: consistent conventions, good abstraction, data-driven levels, clean input
layer. The hardest and most differentiating part — the echolocation sensory loop with
mic input and acoustically-correct echo delay — is built and mostly working.

The gaps are mostly **breadth and wiring**, not deep architecture:
progression (#1), puzzle reset (#2), more level/puzzle types, menus/UX, audio variety.

### Smallest path to a vertical slice
1. Wire `GameManager.LoadNextLevel()` to fire after the player exits a solved room.
2. Make button/puzzle reset actually reset (`ResetElement`).
3. Add one non-"logic room" layout (a maze reuses existing button/door scripts — see `Docs/`).
