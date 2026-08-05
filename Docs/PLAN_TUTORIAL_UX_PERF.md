# Plan — Tutorial finale, UI placement, one-controller mode, Quest 2 perf

Created 2026-07-24. Six work items agreed with Wamiq.

## STATUS

| Part | What | State |
|---|---|---|
| 6 | Quest 2 perf | **Done** — 6 config fixes + QuestPerformanceTuner. Needs on-device numbers. |
| 2 | `0 / 3` restyle | **Done** — cyan, 38px, spacing 5. |
| 4 | Prompt placement | **Done** — yaw-only frame + height-drift recentre. |
| 3 | Menu placement | **Done** — 0.6 m @ 0.7 m hand-anchored; overlay-camera render-through; ~134 lines deleted. |
| 1 | The Auditor | **Done** — reveal sequence built, orphan prefab wired in. |
| 5 | One-controller mode | **Done** — Handedness setting, HandedInput table, rig applier. |
| — | Tutorial teaches hold-to-pause | **Done** — plus all tutorial copy is now handedness-aware. |

**All six parts complete. Everything compiles clean.**

**Nothing is headset-verified.** Every judgement call is logged in `Docs/DECISIONS.md` (D-001…D-033) —
review that first if something feels wrong in play.

### Feel-test priority when you put the headset on
1. **Hold-B pause vs sonar ping in Both mode** (D-031) — a long press pings *and* pauses. Only
   judgeable in headset. Two escape hatches documented.
2. **Menu in both eyes?** (D-018) — the overlay camera was never exercised under XR. If the menu
   renders mono or at the wrong depth, that is why.
3. **Auditor smoke edges** (D-008) — the depth-texture removal is the biggest perf win and the most
   likely visual regression. Check it deliberately.
4. **Frame rate** — run with `adb logcat -s Unity` and read `[QuestPerf]`. It reports whether 90 Hz
   held or it stepped down to 72.
5. **One-stick move + snap turn** — comfort is unknown; losing strafe in the mazes is unknown.

### Known gaps, not bugs
- `MainMenuScene` has no `HandednessController` — the setting works there, the idle-hand ray filter
  does not.
- Pre-existing: XRI binds Move *and* Snap Turn to the same stick on **both** hands with
  `enableTurnAround: true`, so stick-down may walk backward *and* spin 180°. Left alone in Both mode
  because changing it silently alters the shipped control scheme. **Your call.**
- Pre-existing: the `Menu UI Ray` interactors have no line visual or reticle.

Recommended order: **Part 6 (perf) → Part 2 → Part 4 → Part 3 → Part 1 → Part 5.**
Rationale: 45 fps makes every other fix feel bad and the perf work is mostly config, so it is
the cheapest large win. One-controller mode goes last because it rewrites the panel anchoring
that Parts 1/3/4 establish.

---

## Part 1 — The Auditor reveal (tutorial final phase)

**Name: THE AUDITOR.** Facility-records vocabulary, and *audire* = "to hear". No canon name
existed (GDD §4.4 just says "stealth entity"; §184 lists naming as open).

### Copy — three beats

| Beat | Title | Body |
|---|---|---|
| 1 | `THE AUDITOR` | Facility records list it as a monitoring unit. Whatever it was built to listen for, it is still listening. |
| 2 | `IT HUNTS SOUND` | Every ping is a beacon. It walks to where your sound was born, and searches the dark around it. |
| 3 | `IF IT FINDS YOU` | It puts you back where you started. Stand still. Stay silent. It cannot see you. |

Beats 2 and 3 are literally true of `PingAttractedEntity` (`HandlePing` → moves to ping origin,
`pingDangerTimer = 8f` search, `CapturePlayer` → `ResetPlayerToSpawn`), so none of this becomes
a lie in Act 2.

### Flow — replaces the tail of `EndAfterInteraction()` (TutorialDirector.cs:611)

```
button + lever done
  → WARNING prompt, 2s hold                      (unchanged)
  → prompt fades, ~1.5s of dark and silence
  → "LOOK BEHIND YOU" fades up on the wall ahead ← new
  → "SONAR / Press B." prompt                    ← new gate
  → player pings
  → the Auditor resolves out of the dark BEHIND the player's start-facing
  → beat 1 → beat 2 → beat 3   (auto-advance ~5s, cross-faded)
  → it turns toward the player, audio + heartbeat spike
  → existing PlayEnding() fade to black → MainMenuScene   (unchanged)
```

The tutorial still ends on fear, not on a paragraph. The reveal is earned by the player's own
ping, which is the whole point of the game.

### Placement — decided

Player goes left, then works the button and lever on the right. **The Auditor stands behind
them.** To avoid them missing it, `LOOK BEHIND YOU` fades up on the wall they are facing,
using the existing `MazeWorldTextController` pattern (`TextMesh` + fade in/out) — a self-lit
TextMesh reads fine on a black wall.

### Work

1. `Step` enum — insert `Auditor` between `Ending` and `Done`.
2. `BuildAuditorLesson()` — mirrors `BuildEndingLesson()`: find `Tutorial Entity` under
   `levelRoot`, cache it, leave it `SetActive(false)`.
3. **Neuter it.** Disable `PingAttractedEntity` *and* `NavMeshAgent` on the tutorial instance.
   The tutorial level has no baked NavMesh, and this is what makes it a showcase rather than a
   threat — the "it shouldn't work like it works in a level" requirement.
4. TutorialDirector drives the smoke/eye-glow scale and audio manually so it cannot wander.
5. **Wire the prefab.** `Assets/_EchoRoom/Prefabs/Tutorial/Tutorial Entity.prefab` already
   exists and is referenced by **nothing** — a past session staged it and stopped. Add it to
   `T_Junction_Tutorial.prefab` as a child named `Tutorial Entity`, matching how
   `Tutorial Button` / `Tutorial Lever` / `Tutorial Ending Audio` are already found by name.
   That prefab also already contains `Entity wall` (9 m wall at 4.79, 1.08, 17.01) — check
   in-editor whether it was meant as the reveal backdrop.
6. New `auditor-state` class in `VRTutorialPromptStyles.uss` so the lore beats read as their
   own card, reusing the existing prompt panel rather than building new plumbing.

### Safeguards

- `pingEmitter.ResetCooldown()` before the PING gate (same precaution as TutorialDirector.cs:552)
  or the press is silently rejected and the tutorial reads as broken.
- **15 s timeout** — if no ping arrives, reveal anyway. A forced-input gate at the end of a
  tutorial with no escape hatch is a softlock.

### Open check

`Darken()` disables *directional* lights only, but `T_Junction_Tutorial` holds **20 spot
lights**. Confirm whether they are baked or realtime, and whether the reveal area is already
lit — if it is, the ping reveal has no punch.

---

## Part 2 — "0 / 3" restyle

`VRObjectivePanelStyles.uss:116`, `.obj-progress`:

| Property | Now | After |
|---|---|---|
| `color` | `rgb(220, 239, 255)` near-white | `rgb(45, 226, 230)` timer cyan |
| `font-size` | `46px` | `38px` |
| `letter-spacing` | `4px` | `5px` |

The mismatch was the colour *family*, not the size. The `.complete` amber override stays and
reads better against cyan than it did against white. ~3 lines.

---

## Part 3 — Pause menu placement (A + B)

Scope: **Pause / Captured / Settings-from-Pause only.** `ShouldUseFixedWorldPlacement`
(VRPauseMenu.cs:530) sends the *Start* menu to a hand-authored anchor in the safe start room —
that already works and is not being touched.

### Why not literally wrist-mount it

| | Objective panel | Pause menu |
|---|---|---|
| Layout | 560 × 300 px | 900 × 560 px |
| World scale | 0.00028 | 0.002 |
| **Physical size** | **16 cm × 8 cm** | **1.8 m × 1.12 m** |
| Interactive | No — everything is `picking-mode="Ignore"` | Yes — 6 screens, sliders |
| Dwell | ~3 s glance | 30 s+ |

The objective panel works on the wrist because it is a wristwatch you glance at and never
touch. An 11× larger interactive surface strapped to a moving hand, ray-targeted from the other
moving hand, is a different problem.

### A. Shrink, pull close, anchor at the hand, freeze

| | Now | After |
|---|---|---|
| Panel | 1.8 m × 1.12 m | **0.6 m × 0.37 m** |
| Distance | 2.1 m | **0.7 m** |
| Scale | 0.002 | **0.000667** |
| Angular size | ~46° | **~46°** |

`1.8/2.1` and `0.6/0.7` are the same ratio, so it looks **identical in size to the player** but
occupies a third of the space and fits any corridor. 0.7 m stays above the ~0.5 m
vergence-comfort floor.

Placement: add `[SerializeField] Transform menuHandAnchor` (rig already has `Left Controller`).
On open, place along the **head→hand direction, pitch-clamped to ±20°, at a fixed 0.7 m**,
facing the head, then **freeze in world space**. Pitch clamping matters — without it, opening
the menu with your hand at your hip puts the panel on the floor.

### B. Render-through

Primary: override the world-space panel material with a `ZTest Always` variant.
Fallback: URP overlay camera on a dedicated UI layer.
**Not promised before testing** — world-space UI Toolkit material override needs verifying.

### Then delete

With B proven: `GetOcclusionAdjustedPosition` (`BoxCastAll`), the candidate-position search
loop in `PlaceMenu`, `IsPanelPositionClear`, the clearance warnings, and the
`keepInFrontOfWalls` / `wallPadding` / `minimumDistanceFromCamera` / `wallMask` fields —
roughly 150 lines out of VRPauseMenu.cs. That is the real prize.

---

## Part 4 — Tutorial prompt sits too low (NEW)

### Root cause

`TryPlacePromptAtHeadPose()` (TutorialDirector.cs:405) builds the offset from
`head.right / head.up / head.forward` — the **full head orientation including pitch** — and is
called **once** per prompt, from `ShowPrompt` / `SwapPrompt`. The panel is then world-locked.

So if the player happens to be looking down (at the lever, at their hands) at the instant a
beat fires, the panel is placed down there and *stays* there. Standing players get it worst,
because `PromptOffsetFromView.y = -0.10` is already below eye level. Hence bowing to read it.

### Fix

1. **Yaw-only basis.** Flatten `head.forward` to horizontal and use world up. Head pitch then
   can never drag the panel up or down.
2. **Eye-height anchored.** Place at head height with a small world-space drop (≈ -0.05 m at
   0.9 m out ≈ 3° below eye line) instead of an orientation-relative offset.
3. **Lazy re-centre.** Soft-follow head yaw with a deadzone (re-centre only when the player
   turns more than ~35° away, over ~0.4 s) so a prompt is never stranded behind them. Keep it
   world-stable inside the deadzone so it does not swim — swimming UI causes nausea.
4. Apply the same treatment to the Auditor lore beats in Part 1, since they reuse this panel.

Also worth checking: the XR rig's height offset handling for standing vs seated players
(`HeightOffsetController.cs`) — if the rig assumes seated height, everything sits low.

---

## Part 5 — One-controller mode (NEW)

**Yes, it is possible.** It is also the largest item here, and there is one hard constraint.

### Current input inventory

| Input | Hand | Bound where |
|---|---|---|
| Move | Left stick | Rig: `Left Hand Move` |
| Turn / snap turn | Right stick | Rig: `Left Hand Turn`, `Left Hand Snap Turn` |
| Sonar ping (B) | Right secondary | `PingEmitter` / sonar control |
| Microphone ping (Y) | Left secondary | `MicPingTrigger.cs:10` (const binding string) |
| Objective panel (A) | Right primary | `MazeLevelTimer.cs:638-639` |
| Interact | Right trigger | `SimpleControllerInteraction.cs:109` (`XRNode.RightHand`) |
| Pause | Menu button | `VRPauseMenu.cs:209-210` (both hands already) |
| Haptics | Both | `EchoHaptics.cs` |

There is **no `.inputactions` asset** in `_EchoRoom` — bindings are code-built strings in three
places plus action-based providers configured on the rig prefab. Most of the work is on the rig.

### Proposed single-hand scheme

| Input | Function |
|---|---|
| Stick vertical | Move forward / back |
| Stick horizontal | Snap turn |
| Trigger | Interact |
| Grip (hold) | Microphone ping |
| A / X | Sonar ping |
| B / Y | Objective panel |
| Menu | Pause |

Stick vertical = move, horizontal = turn is a standard one-handed VR scheme and it fits.

### The hard constraint

**Quest Touch controllers only have a Menu button on the LEFT.** The right controller's
equivalent is the Oculus button, reserved by the system. `VRPauseMenu.cs:210` already binds
`<XRController>{RightHand}/menuButton`, which **does not exist on Touch** — so in right-hand-only
mode there is no pause button. Options: hold B/Y for ~0.75 s, or double-tap it. Needs a decision.

### Also required

- **Panels follow the active hand.** `MazeLevelTimer.rightController` and the new
  `VRPauseMenu.menuHandAnchor` must both resolve at runtime from the setting, not be
  hard-wired. Objective panel → active hand; pause menu → the same hand in one-controller mode.
- **Interactors move.** Ray/direct interactors must be enabled on the active hand and disabled
  on the other, or the ray still comes out of the dead controller.
- **Haptics target the active hand** (`EchoHaptics` currently pulses both).
- **Setting + persistence** in `EchoRoomSettings` / `VRSettingsPanelController`:
  `Handedness: Both | Left | Right`.

### Scope honesty

This touches locomotion providers, three code-built bindings, interactor setup, haptics, both
wrist panels, and the settings UI. It is the biggest item in this document. It should land
*after* Parts 1/3/4 settle where the panels live, or the panel work gets done twice.

---

## Part 6 — Quest 2 performance (NEW, do first)

Target: 72 fps floor, 90 fps goal. Currently 45 fps in a black tutorial room, which is the
tell — 45 is exactly **half of 90**, so the app is set to a 90 Hz display and missing the
11.1 ms budget, dropping to half rate.

A black room with one panel should be nearly free. That it is not means the cost is fixed
overhead, not scene complexity. Evidence found in the project files:

### Confirmed misconfigurations

| Setting | File | Now | Should be | Why |
|---|---|---|---|---|
| `m_IntermediateTextureMode` | `Default URP_Renderer.asset:79` | `1` (Always) | `0` (Auto) | Forces an offscreen render target + full-screen blit **every frame** instead of rendering straight to the backbuffer. Pure waste on a tiler. |
| `m_SupportsHDR` | `Default URP.asset:26` | `1` | `0` | FP16 buffers double bandwidth and break the ideal Quest tile format. |
| `m_UseNativeRenderPass` | `Default URP_Renderer.asset:52` | `0` | `1` | Without it URP cannot keep work in tile memory on Vulkan. |
| `m_ShadowDistance` | `Default URP.asset:57` | `50` | `~15` | The game is lightmapped; 50 m of realtime shadow range is paid for nothing. |
| Fixed Foveated Rendering | OpenXR settings | not enabled | level 2–3 | Typically 15–25 % GPU on Quest 2, free. |
| `m_MSAA` | `Default URP.asset:28` | `1` (off) | `4` | Quality, not perf — near-free on a tiler. Turn on *after* we are in budget. |

Good news already confirmed: `m_renderMode: 1` = **single-pass instanced is on**, SRP Batcher
is on, and there is **no post-processing volume in MainScene** (so bloom is not the culprit here).

### Also check

- `PingAttractedEntity.enableDebugLogs` defaults to **`true`** — `Debug.Log` on device is a real
  frame cost. Default it off.
- **Three separate `PanelSettings` instances** (MazeLevelTimer, TutorialDirector, VRPauseMenu),
  each a separate UI render target. Consolidate where possible.
- **Texture import sizes.** `rock_wall_10_nor_gl.jpg` (5 MB), `mossy_cobblestone_Diffuse.jpg`
  (4.4 MB) and friends have **no `maxTextureSize` override anywhere** — they import at native
  resolution. Cap maze textures at 1024–2048, confirm ASTC.
- The 20 spot lights in `T_Junction_Tutorial` — baked or realtime?
- `m_StoreActionsOptimization: 0` → Discard.

### Method — measure, do not guess

Before and after every batch, capture on-device **CPU vs GPU frame time** (OVR Metrics Tool, or
Unity Profiler over ADB). That one number says whether we are CPU- or GPU-bound and everything
else follows. I will not claim a fix works without a before/after number.

### Fallback

If 90 Hz stays out of reach, **target 72 Hz deliberately.** 13.9 ms of budget instead of
11.1 ms is a 25 % increase, and a locked 72 is dramatically better than a 90 that halves to 45.

---

## Decisions — resolved 2026-07-24

1. **Right-hand-only pause → hold B/Y (~0.75 s)**, and it gets taught in the tutorial.
   Implementation note: hold-to-pause is enabled in **all** handedness modes, not just
   right-only. Otherwise the tutorial copy has to branch on a setting, and a player who changes
   handedness later never gets taught. One binding, one lesson, always true.
   → adds a short pause beat to the tutorial (see Part 1 flow).
2. **Render-through** — Claude's call. Order is: prove the approach in-editor → confirm the menu
   draws through a wall → *only then* delete the wall-avoidance code. The safety net stays until
   the replacement is demonstrated working. If neither the material override nor the overlay
   camera pans out, Part 3A still stands on its own and the avoidance code simply stays.
3. **Tutorial lighting stays pitch black.** Confirmed by Wamiq. This resolves the risk in the
   good direction — the ping reveal will land. The 20 spot lights are therefore baked or
   inactive; confirm during Part 6 anyway, since dead realtime lights would still cost frames.
4. **CPU-bound possibility** — acknowledged. Profile first, then decide.
5. **Order: Part 6 first**, then 2 → 4 → 3 → 1 → 5. Claude's call, no objection raised.
6. **Refresh target: 90 Hz goal, 72 Hz acceptable fallback.** A locked 72 ships over a 90 that
   halves to 45.

## Standing risks

- **Perf may be CPU-bound.** Every fix in Part 6's table is GPU-side. If profiling says CPU, that
  table is irrelevant and the work becomes script and draw-call hunting instead.
- **Render-through may not be achievable** for world-space UI Toolkit. Fallback is Part 3A alone.
- **On-device verification is a hard dependency.** Frame timings, VR comfort, and whether the
  Auditor is actually frightening cannot be checked from the editor. See "Working agreement".

## Working agreement

Claude leads: owns implementation, editor work, subagent delegation, review, and all reversible
decisions. Wamiq is not consulted on routine choices.

Wamiq is needed for exactly two things:

1. **Anything requiring the headset.** Building to the Quest 2 and reporting CPU/GPU frame times
   (Part 6 is measurement-driven and cannot proceed on editor numbers), plus comfort checks:
   panel height while standing, 0.7 m menu distance, whether the Auditor reveal lands.
2. **Irreversible or taste calls** — anything that would be expensive to undo.

These get **batched into single check-in requests**, not drip-fed.
