# Echo Room VR — GDD vs. Implementation Progress

> Feature-by-feature audit of the Game Design Document against what is actually
> built in code. Legend: ✅ done · ⚠️ partial · ❌ not started.

**Headline: ~30–35% of the GDD is implemented** — but the hardest, most
differentiating part (the echolocation sensory loop) is done. What remains is
mostly *breadth* (more rooms, menus, audio variety) and *wiring*, which is more
numerous but individually easier than what's already solved.

---

## Controls

| GDD control | Status | Notes |
|---|:--:|---|
| Trigger: emit ping | ⚠️ | Works via a generic ping action + **mic** + editor SPACE. "Snap/shout" realized through the mic trigger. |
| Grip: interact with switches | ✅ | `GetGripInput()` → `EchoButtonInteractable`. |
| Joystick: smooth locomotion / teleport | ⚠️ | Smooth locomotion + sprint done. **Teleport (accessibility) not implemented.** |
| Button A/B: toggle ping mode (hi/low freq) | ❌ | No ping-mode/frequency system. |

## Core mechanics — Echolocation

| GDD spec | Status | Notes |
|---|:--:|---|
| Ping reflects off geometry | ✅ | `EmitDirectionalEcho` spherecast. |
| **Echo delay = distance** | ✅ | `delay = distance / 343f` — exactly as specified. |
| Pitch shift = material | ❌ | Echo clip is fixed; no per-material pitch. |
| Echo volume = object size/surface | ❌ | No volume scaling by object. |
| Echo-absorbing surfaces | ❌ | Only LayerMask filtering; no "absorbing" concept. |

## Replayability

| GDD spec | Status |
|---|:--:|
| Procedural room layouts (seed-based) | ❌ |
| Randomized sound puzzles | ❌ |
| Timed / endless mode | ❌ |

> Note: the blueprint generator in `Docs/` / `Assets/_EchoRoom/Design/` is
> seed-based and could later feed runtime procedural generation.

## Advanced abilities

| GDD spec | Status | Notes |
|---|:--:|---|
| Directional ping | ⚠️ | Exists internally as the echo spherecast, but not a player-selectable ability. |
| Multi-ping | ❌ | |
| Noise filter | ❌ | |

## Art direction

| GDD spec | Status | Notes |
|---|:--:|---|
| Dark / foggy world | ⚠️ | Depends on scene setup (see `GAME_OVERVIEW.md` for target look). |
| **Wireframe/shader outline on echo hit** | ✅ | `SimpleSonarShader` + `EchoPulseController`. |
| Blocky / primitive geometry | ✅ | ProBuilder available and used. |
| VR floating radial settings menu | ❌ | No UI/menu code exists. |
| Haptics / cooldown feedback | ⚠️ | Cooldown logic exists; feedback is a flash light. **No haptics.** |

## Audio design

| GDD spec | Status | Notes |
|---|:--:|---|
| Echo sounds | ✅ | `EchoSoundController`, spatial 3D audio. |
| Background ambient (drips/wind/hum) | ❌ | Not in code. |
| Puzzle sounds (tone/buzz/chime) | ⚠️ | Buttons have an AudioSource; no success/error system. |
| Audio Mixer sound zones / reverb | ❌ | Meta XR Acoustics present, but no mixer logic. |

## Technical

| GDD spec | Status |
|---|:--:|
| Unity + OpenXR | ✅ |
| Meta Quest support | ✅ |
| SteamVR support | ❌ |
| Spatial audio bouncing | ✅ |
| Navmesh / grid layout | ❌ |
| Echo-triggered outline shader | ✅ |

## Level types (the actual game content)

| Type | Status | Notes |
|---|:--:|---|
| **Logic Rooms** (echo correct panels → door) | ✅ | The one built: `EchoPuzzleController` + buttons + `DoorPuzzleBinder`. |
| Maze Rooms | ❌→🟡 | Not built, but **blueprints designed** (see `Assets/_EchoRoom/Design/Maze_5x5_*.svg`). Reuses existing button/door scripts. |
| Pattern Rooms (memorize/mimic sequences) | ❌ | |
| Stealth Rooms (avoid moving entities) | ❌ | |

---

## Progress against the GDD development plan

| Phase (GDD) | Target | Est. done | Notes |
|---|---|:--:|---|
| Prototype (Wk 1–2) | Ping + spatial reflection | **~90%** | Strongest part. Ping, echo, sonar shader, mic input all work. |
| Core Gameplay (Wk 3–4) | 3 puzzle types, first level | **~25%** | 1 of 3 types (Logic). Level framework exists but **progression not wired**; reset is a stub. |
| Polish & UX (Wk 5) | Shaders, haptics, menus | **~5%** | Sonar shader done; no menus, no haptics, no ping-mode toggle. |
| Sound Tuning (Wk 6) | Echo feedback + puzzles | **~10%** | Core echo done; no material pitch, ambient, absorbing surfaces, mixer zones. |
| Playtest / Launch (Wk 7–8) | Store setup, trailer | **0%** | |

---

## What blocks a playable "first level" (vertical slice)

1. **Wire level progression** — `GameManager.LoadNextLevel()` is never called when a
   room is solved/exited. (See `CODEBASE_ANALYSIS.md` issue #1.)
2. **Make puzzle/button reset work** — currently a stub; replays leave buttons stuck. (Issue #2.)
3. **Add one non-logic layout** — the **maze blueprints** are ready and reuse existing
   scripts, so this is mostly geometry + button/door placement, not new code.

Closing these three turns the existing pieces into an actual playable loop.
