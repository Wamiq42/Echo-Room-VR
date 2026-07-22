# Echo Room VR — UI Fix Work Plan

Execution plan for the issues catalogued in [`UI_ISSUES_2026-07-20.md`](UI_ISSUES_2026-07-20.md).
Investigation lives in that document; **this** document is the plan of record and the running log.

**Owner:** Claude (lead). Work is delegated to subagents where it parallelises safely; every change
is reviewed by the lead before commit.

## Ground rules agreed with Wamiq (2026-07-20)

| Decision | Value |
| --- | --- |
| Issue 8 architecture | **A+** — persistent `GameManager` + loading screen + screen fade. No additive scenes. |
| Git | **One commit per issue**, directly on `Development-Phase`. No branches. No push. |
| Conflicts | Resolved before any work. ✅ Done — commit `9886acd`. |
| Ambiguity | **Decide and document.** Record rationale; skip only genuinely irreversible actions. |
| Unity | Stays open. **Power cuts are a real risk** → commit per issue, save scenes eagerly, never leave an unsaved multi-step edit. |
| QA | Screenshot before/after where visual; otherwise a written check. Recorded below. |
| Out of scope | Issues **12** and **14** (in-game HUD / wrist panel) and **7a** (panel resolution reflow). |

### Screenshot tooling

`node .claude/tools/unity-shot.js <out.png> [tool] [inputJson]` — wraps `unity-mcp-cli`, extracts the
base64 payload and writes a PNG. Verified working. Captures land in `Docs/QA/`.

```
node .claude/tools/unity-shot.js Docs/QA/issue-04-after.png                    # game view
node .claude/tools/unity-shot.js Docs/QA/issue-04-after.png screenshot-scene-view
```

### Verification ceiling — read this before trusting any "done"

I can verify: compilation, serialized values via MCP readback, Editor Play Mode behaviour, and Game
View screenshots. **I cannot wear the headset.** Anything about comfort, stereo convergence, apparent
distance, or physical reachability is marked **`PENDING-HEADSET`** and is explicitly *not* claimed as
done. Those need your eyes.

---

## Execution order

Ordered by dependency, not by issue number. Each row is one commit.

| # | Work | Issues | Depends on | Status |
| --- | --- | --- | --- | --- |
| W0 | Resolve merge conflicts | — | — | ✅ Done (`9886acd`) |
| W1 | Ray→UI input fix (trigger interaction) | 2 | — | ✅ Done (`052bb7c`) |
| W2 | Transition ownership: persist loading screen + fade | 8, 5, 6 | — | ✅ Done (`b78075f`) |
| W3 | Consolidate the hard-coded world anchor | 13 | W2 | ✅ Done |
| W4 | Menu placement: wall occlusion + distance | 4, 7b | W1 | ✅ Done |
| W5 | Editor authoring parity | 1 | W1 | ⬜ |
| W6 | Tutorial prompt: anchoring + styling | 3, 11 | — | ⬜ |
| W7 | Timer fairness: warnings + tutorial teaching | 10 | W2 | ⬜ |
| W8 | Main menu: block simulator WASD | 9 | — | ✅ Done (`8bcecdc`) |
| W9 | Interaction-layer reconciliation + safe cleanup | adjacent | W1 | ⬜ |

**Parallel-safe groups.** W1, W6 and W8 touch disjoint files and can run concurrently. W2 and W3 are
strictly sequential (same files). W4 and W5 both wait on W1's outcome.

**File contention map** — no two concurrent workers may hold the same file:

| File | Claimed by |
| --- | --- |
| `VRPauseMenu.cs` | W4 |
| `VRMainMenu.cs` | W2 → W3 → W5 |
| `VRLoadingScreen.cs` | W2 → W3 |
| `GameManager.cs` | W2, then W7 |
| `TutorialDirector.cs` | W6, then W7 |
| `MazeLevelTimer.cs` | W7 |
| `UnifiedLocomotionBridge.cs` | W8 |
| `MainMenuScene.unity` / `MainScene.unity` | W1, W2, W4, W8 — **serialise these, never concurrent** |

Scenes are the bottleneck. Only one worker may hold a scene at a time, and the lead serialises all
scene writes through MCP rather than editing YAML on disk.

---

## W1 — Ray→UI input

**Root cause (confirmed).** All four `XRRayInteractor`s have `m_RaycastTriggerInteraction: 1`.
Unity's `QueryTriggerInteraction` is `UseGlobal=0, Ignore=1, Collide=2` — so they are set to
**Ignore triggers**. The UI Toolkit panel's only collider *is* a trigger
(`VRMenuPanelSettings.asset:20`, `m_ColliderIsTrigger: 1`). Rays pass straight through.

> Corrects my earlier claim that `1` meant Collide. It does not. This, not the mirrored transform,
> is why rays did nothing. Confirmed by Wamiq: rays pass through, collider is correctly sized, and
> removing the 180° rotation changed nothing.

**Approach.** Prefer changing the *panel*, not the interactors: set `m_ColliderIsTrigger: 0` so the
panel presents a solid collider. Setting interactors to `Collide` would make rays snag on every
gameplay trigger volume in the maze — a wider blast radius for the same result.

**Risk to check:** a solid panel collider could physically block the player or interact with physics.
Mitigate by confirming the panel sits on a layer excluded from player/physics collision. If that
proves messy, fall back to interactor-side `Collide` **plus** a narrowed raycast mask.

**Acceptance:** ray hover highlights a menu button; trigger press activates it; no new console
errors; player cannot be physically blocked by a panel. Screenshot before/after.

**QA:** ⬜ pending

---

## W2 — Transition ownership (the structural one)

**Root cause (confirmed).** `GameManager` and `VRLoadingScreen` are both **scene-local**. Two copies
of each exist (`MainMenuScene.unity:2515` / `MainScene.unity:7907`). The menu-side loading screen is
destroyed mid-coroutine at `allowSceneActivation = true`, and MainScene's separate copy then shows
*after* the maze is visible.

**Approach (option A+).**

1. Introduce a persistent transition owner (`DontDestroyOnLoad`) holding the loading screen, screen
   fade, and transition state, with a single `IsTransitioning` flag.
2. Make `GameManager` persistent so the "two instances, one per scene" pattern is gone at the root.
3. While `IsTransitioning`: disable UI ray pointers and suppress other UI (fixes 5 and 6).
4. Reorder `GameManager.InitializeGame()` — the current code calls `screenFade.SetOpacity(0f)` on the
   line *before* showing the loading screen (`GameManager.cs:98`), clearing the only thing masking
   the seam. Fade must stay up until the destination is ready.
5. Gate `MazeWorldTextController`'s intro on "transition complete" rather than `OnEnable`, so the
   "LEVEL 1 / Find the exit." banner is actually seen. It currently never is.
6. Retune total loading time — presently ~10 s split across the boundary (5 s each side).

**Care required.** Making the XR Rig's neighbours persistent while the rig itself is per-scene means
camera references must be re-resolved after each load. `VRLoadingScreen.ResolveCamera()` already
handles a null camera; verify it re-resolves rather than caching a destroyed transform.

**Acceptance:** loading screen appears *before* the maze is visible, stays through activation, and
hides only once the level is ready. Pause menu is hidden and rays are dead for the whole transition.
Level intro banner visible after the screen clears. `PENDING-HEADSET` for comfort of the transition.

**QA:** ⬜ pending

---

## W3 — Loading screen: head-relative placement (Issue 13)

**Scope reduced by W2.** W2 added `sceneAnchors` so the persistent loading screen at least uses the
correct *per-scene* anchor. But the MainScene anchor is still `(-3.04, 0.95, -1.342)` — the main-menu
hallway coordinate — while the player is teleported to the maze spawn. So the loading panel can still
render where the player isn't. W3 finishes the job.

**Approach.** The loading screen is a full-screen cover; it should follow the head, not sit at a world
point. `VRLoadingScreen.PlaceInFrontOfPlayer()` already has a non-fixed branch
(`cameraTransform.position + forward * distanceFromCamera`) — switch the loading screen to use it by
default (`useFixedWorldPlacement = false`), verify the camera re-resolution from W2 keeps it correct
after the scene swap, and confirm it tracks smoothly rather than jitters. The `sceneAnchors` mechanism
becomes dead weight once this works and should be removed to avoid two placement systems.

**Do NOT** change the *menu* anchors (main menu / pause) to head-relative here — those are handled by
W4 (distance) and are a different UX decision. W3 is loading-screen-only.

**Acceptance:** enter a maze via NEW GAME; the loading screen is centred in front of the player the
whole time, including immediately after `MovePlayerToSpawn`. Screenshot from inside a maze.

**QA:** ⬜ pending

---

## W4 — Menu placement: occlusion and distance

**Occlusion (Issue 4).** `VRPauseMenu.cs:547-553` casts a single centre ray against a ~1.8 m-wide
panel, so corners punch through walls. Three defects: single ray, `wallMask = ~0` (hits solid props,
interactables, and player colliders), and the fixed-anchor path skips the check entirely. Trigger
volumes were already excluded by `QueryTriggerInteraction.Ignore`.

**Approach.** `BoxCast` sized to the panel, on an explicit geometry layer mask, applied on **both**
the dynamic and fixed paths — treating the fixed anchor as a preferred position that gets pulled in
when blocked.

**Distance (Issue 7b).** Panel is 1.44 m wide; main menu uses a fixed anchor. Target ~1.5 m viewing
distance (comfort band is 1.2–2.0 m; below ~1 m causes vergence strain).

**Acceptance:** menu never intersects wall geometry from any approach angle — verified from several
positions by screenshot. Distance change is `PENDING-HEADSET`; I'll set a defensible starting value
and you adjust to taste.

**QA:** ✅ Editor Play Mode and physics-volume checks complete. `PENDING-HEADSET` for main-menu
distance/readability and emergency scale-to-fit comfort.

---

## W5 — Editor authoring parity

**Root cause.** Stylesheets are attached in `Awake()` (`VRLoadingScreen.cs:61-62`), screens are
hidden in `Awake()` (`:82`), and document sizing is assigned in `Awake()` (`:53-58`). Edit mode never
runs `Awake()`, so every screen renders at once, unstyled — see
`Docs/QA/issue-01-before-editmode-stacking.png`.

**Approach (option A).** Move authoring data into the assets: `<Style src="..."/>` in each UXML,
`display: none` as the USS default for non-active screens, and serialized `UIDocument` fields
matching what `Awake()` assigns. Code then only *changes* state, never *establishes* it. Deliberately
**not** `[ExecuteAlways]`, which would run `Awake`/`Update` in the Editor and risk scene-dirtying.

**Sequenced after W1** so we don't bake the current mirrored-transform hack into serialized assets.

**Acceptance:** Scene/Game view outside Play Mode shows a single correctly-styled screen. Runtime
behaviour unchanged. Before/after screenshots.

**QA:** ⬜ pending

---

## W6 — Tutorial prompt: anchoring and styling

**Anchoring (Issue 3).** `TutorialDirector.cs:345` re-drives the prompt from
`rightController.position` every frame. Wamiq asked for "best possible UX" → **head-relative,
world-locked on show**: placed once in front of the player when it appears, then frozen. Always
visible on arrival, never chases the player (chasing UI is a leading cause of VR discomfort), and
consistent with how the pause menu already behaves.

**Styling (Issue 11).** The prompt is the only UI outside the UI Toolkit stack — a runtime-built uGUI
`Canvas` with a bare `TextMeshPro` and hard-coded font values (`:325-331`). It cannot inherit
`VRMenu.uss`. Port to UI Toolkit and reuse the menu stylesheet so it matches for free.

**Also fix here:** `TutorialDirector.cs:528` falls back to `RenderMode.ScreenSpaceOverlay` when
`Camera.main` is null — the only such case in the project, and it will not composite correctly in
stereo. Should fail loudly instead.

**Acceptance:** prompt appears in front of the player, stays put, matches menu styling. Screenshots
of each tutorial prompt state. `PENDING-HEADSET` for readability.

**QA:** ⬜ pending

---

## W7 — Timer fairness

**Root cause.** 180 s per maze; display is controller-parented and alpha-0 unless the A button is
pressed. The only unprompted warning is a 4-flash at level start — which currently fires *behind the
loading screen*, so it is never seen. Hence "no indication of time anywhere."

**Approach — options A + C, keeping the manual reveal (as agreed).**

1. Escalating auto-reveals at 60 s / 30 s / 10 s remaining, with audio. Prefer a signal-decay or
   heartbeat cue over a beep — it fits the fiction and the game is audio-first.
2. Teach the reveal button in the tutorial. `TutorialDirector` already has a prompt-slot pattern.
3. Keep A / keyboard `T` manual reveal exactly as-is.

**Depends on W2** — the level-start flash only becomes visible once the loading screen stops covering
it.

**Open question I will decide and document:** `GetDuration` returns `-1` for unmatched level names,
so **Maze E currently has no time limit**. I'll leave that behaviour alone (changing it is a design
call, not a bug fix) and flag it for you.

**Tutorial copy** will be drafted to match the existing voice (`SonarMessage` et al., `:130-134`) and
flagged for your approval — it's player-facing text and it should sound like you wrote it.

**Acceptance:** warnings fire at the right thresholds and are visible; tutorial teaches the reveal;
manual reveal still works. `PENDING-HEADSET` for whether the warnings read clearly in-headset.

**QA:** ⬜ pending

---

## W8 — Main menu: block simulator WASD

**Root cause (confirmed, and self-inflicted).** The WASD movement is Unity's **XR Device Simulator**,
not the game's locomotion. The component that suppresses simulator translation —
`UnifiedLocomotionBridge`, via `suppressSimulatorTranslation` in `OnEnable` (`:22, 36-38`) — is the
same component that was **disabled** in `MainMenuScene` to turn locomotion off. Disabling locomotion
is precisely what *enabled* the WASD movement.

**Scope note.** This is `#if UNITY_EDITOR` only and never ships to Quest. It's a playtest-fidelity
problem, not player-facing — but it matters because it makes Editor sessions misrepresent the build.

**Approach.** Move simulator suppression somewhere that does not depend on locomotion being enabled.

**Acceptance:** WASD does not translate the player in `MainMenuScene`; mouse look and simulated
controller interaction still work (needed for testing the menu). Gameplay locomotion unaffected.

**QA:** ⬜ pending

---

## W9 — Interaction layers and safe cleanup

1. **Reconcile `PanelInputConfiguration.m_InteractionLayers`** — `4294967291` in `MainMenuScene.unity:1122-1131`
   vs `1075` in `MainScene.unity:4113-4116`. Drift, not design. Align to one value.
2. **Replace name-string lookups.** Three places match GameObjects by literal name and silently break
   on rename: `VRMainMenu.cs:262-268` (`"Menu UI Ray"`, plus a full-scene `FindObjectsOfType` scan),
   `MazeLevelTimer.cs:273-285` (`"Right Controller"`), and the tutorial prompt. Replace with
   serialized references.

**Deliberately NOT doing without your say-so** (recorded, not actioned):

- **Deleting `VRFrontEndMenu.cs`** — duplicates `VRMainMenu.cs` against the same UXML IDs, and only
  `VRMainMenu` appears wired into a scene. Deleting a script is irreversible enough that I want your
  confirmation, and "appears unused" is not proof.
- **Deleting `Resources/UI/VRGameplayMenu.uxml`** — zero references, but it may be groundwork for the
  Issue 14 wrist panel. Leaving it.
- **`EchoPuzzleController`'s per-frame `GetComponentsInChildren`** (`:98`, called from `Update` `:20`)
  — a real performance smell on Quest, but it's gameplay code outside this UI pass.

**QA:** ⬜ pending

---

## Running log

Appended as work completes — what changed, what was verified, what is still `PENDING-HEADSET`, and
every judgment call made while you were asleep.

### W0 — Merge conflict resolution ✅

- **Commit:** `9886acd`
- **`Docs/PROJECT_MEMORY.md`** — real conflict. Both sides were *distinct append-only journal
  entries*, not competing versions of the same entry: upstream held
  `MAINMENU-LONGWALL2-LIGHTMAP-001` plus tutorial entries, stashed held `MCP-MEMORY-001`. Resolved as
  a union (removed the three marker lines only). **No content discarded.**
- **`Packages/manifest.json`** — had **no conflict markers**; already resolved in the working tree and
  merely unstaged. Validated as parseable JSON, then staged.
- **Verified:** zero conflict markers remain in the repo; both paths report clean.

### W1 — Ray→UI input ✅

- **Commit:** `052bb7c`
- **Change:** `m_RaycastTriggerInteraction` `1` → `2` (Collide) on the four `Menu UI Ray`
  interactors, both scenes. `Teleport Interactor`s deliberately left at `Ignore` — they have
  `enableUIInteraction=false` and are not UI rays.

**Live audit before the change** (read back from the running Editor):

```
RAY Teleport Interactor | trigger=Ignore  | mask=-5 | enableUI=False | active=False
RAY Menu UI Ray         | trigger=Ignore  | mask=1  | enableUI=True  | active=True
RAY Teleport Interactor | trigger=Ignore  | mask=-5 | enableUI=False | active=False
RAY Menu UI Ray         | trigger=Ignore  | mask=1  | enableUI=True  | active=True
DOC Main Menu | localScale=-0.00184,0.00184,0.00184 | collider size=(900,560,4) isTrigger=True
PanelSettings: colliderIsTrigger=True renderMode=1
```

**Proof the fix is causal** — same ray, same mask, only the trigger setting varied:

```
QueryTriggerInteraction.Ignore  -> hit=False   (NOTHING - this was the bug)
QueryTriggerInteraction.Collide -> hit=True on Main Menu
VERDICT: CONFIRMED - trigger setting was the blocker.
```

**QA status:** ✅ physics layer proven in-Editor. ⚠️ **`PENDING-HEADSET`/play-test for the last
step** — I proved the ray now *reaches* the panel; I have not proven `XRUIInputModule` then converts
that hit into a hover/click, because driving XR controller aim in Play Mode isn't something I can do
reliably. **This is the first thing to check when you wake up:** point a controller at a menu button
and confirm it highlights and activates.

**Three things found while doing this, worth your attention:**

1. **Unity is warning about the negative scale.** Console:
   `"BoxCollider does not support negative scale or size. The effective box size has been forced
   positive and is likely to give unexpected collision geometry. Scene hierarchy path 'Main Menu'"`
   The collider's world bounds do come out correct (1.66 × 1.03) because the box is centred and
   symmetric, so this did **not** cause Issue 2. But if clicks land on the *wrong button* once rays
   work, this mirroring is the reason. Flagged for W5.
2. **The main menu is 2.5 m from the player** — measured from the actual interactor origin
   `(0, 0.82, -11.25)` to panel centre `(0, 1.15, -8.75)`. That confirms your "it is very far"
   complaint quantitatively; comfortable reading is 1.2–2.0 m. Feeds W4/Issue 7b.
   Also note the panel's real position is **not** the `(-3.04, 0.95, -1.342)` script default — the
   scene serializes a different value. W3 must read live values, not the source defaults.
3. **Unity normalized a dangling override on save.** `MainMenuScene` dropped an `m_AddedComponents`
   entry for a `UniversalAdditionalLightData` on prefab GameObject `4953029835817543307` — an ID
   that **no longer exists** in `T_Junction_Tutorial.prefab`. I checked before accepting it: that
   prefab has 20 lights and 20 matching light-data components, so nothing was lost. It was a stale
   reference Unity garbage-collected. **Expect this diff to reappear whenever anyone saves that
   scene** — it is not caused by this work.

### W8 — Simulator WASD ✅

- **Commit:** `8bcecdc`
- **New:** `Assets/_EchoRoom/Scripts/Controller/SimulatorTranslationSuppressor.cs` (entirely inside
  `#if UNITY_EDITOR`). Bootstraps via `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` onto a hidden
  `DontDestroyOnLoad` host and re-applies on `sceneLoaded`, so suppression no longer depends on
  `UnifiedLocomotionBridge` being enabled.
- **Modified:** `UnifiedLocomotionBridge.cs` — its own suppression path removed so there is exactly
  one owner and the two cannot fight. Keeps the serialized `suppressSimulatorTranslation` flag
  (**no scene data changed** — both scenes already serialize `1`) and exposes it read-only.
- Only translate inputs / keyboard translate speeds are zeroed, so **mouse look and simulated
  controller interaction still work** — you need those to test menus.

**QA status:** ✅ Unity recompiled clean — verified the only `error CS` entries in the console were
from my own earlier throwaway audit script, none from these files; `.meta` generated.
⚠️ **`PENDING-PLAYTEST`** — I could not press WASD in Play Mode to confirm the behaviour end to end.
**Check on waking:** enter Play Mode in MainMenuScene, press WASD → you should not translate; mouse
look should still work.

**Judgment calls made** (you can overrule any of these):
- Chose a separate bootstrap component over `[ExecuteAlways]` or re-enabling the bridge, because
  re-enabling the bridge would have re-enabled locomotion — the thing you asked to stay off.
- Widened the simulator lookup to `FindObjectsInactive.Include`. Intentional (an inactive simulator
  should still be suppressed) but it *is* a behaviour change from the previous exclude-inactive
  lookup.
- `IsSuppressionRequested()` reads the **first** `UnifiedLocomotionBridge` found. Correct today (one
  per scene); would be wrong if a scene ever had two with different flags.

**Unrelated noise spotted:** the console is full of `[PlayerInputManager] Move: (-0.71, 0.71)` debug
logs every frame. Left-over debug logging — worth silencing, but it's outside this UI pass so I have
not touched it.

### W2 — Transition ownership ✅

- **Commit:** `b78075f`
- **Design correction made during the work.** The agreed A+ plan said "make `GameManager`
  persistent." That turned out to be **wrong**: `GameManager` exists *only* in MainScene (zero
  occurrences in `MainMenuScene.unity`), so it does not exist when the menu→game transition starts
  and could never own it. The persistent owner had to be `VRLoadingScreen` itself. Same outcome,
  correct mechanism.
- `VRLoadingScreen` is now a `DontDestroyOnLoad` singleton owning `IsTransitioning`; re-resolves
  `Camera.main` on `sceneLoaded` (treating a cached-but-deactivated camera as stale, not just a
  destroyed one); suppresses UI ray pointers and keeps `VRPauseMenu` hidden while transitioning →
  **this is what fixes Issues 5 and 6**.
- `GameManager.InitializeGame` now detects an in-flight transition and completes it instead of
  starting a second sequence. The fade is no longer cleared before the loading screen is up.
- Loading time: one 4 s window instead of two 5 s windows.
- Added `sceneAnchors` so the persistent panel uses the right per-scene anchor.
- 25 s watchdog logs an error and hides if nothing completes a transition.

**QA status:**
- ✅ Compiles clean — verified the new API is genuinely present in the **rebuilt** `Assembly-CSharp`
  (timestamp checked, 70 s old), and every pre-existing public method survives. This matters because
  a *failed* compile also leaves `IsCompiling: false` with stale assemblies loaded.
- ✅ Play Mode smoke test in MainMenuScene: no exceptions, menu renders correctly —
  `Docs/QA/w2-playmode-mainmenu.png`.
- ✅ **Menu→maze transition verified in Editor Play Mode (2026-07-23).** A runtime observer sampled
  the transition from `MainMenuScene` through `MainScene`: the same loading-screen instance remained
  in `DontDestroyOnLoad`, visible and transitioning for about 5.2 seconds, reached 100% before scene
  activation, remained visible through activation, then hid only after the destination initialized.
  Both UI rays stayed inactive and the pause menu stayed closed throughout. The test invoked the
  menu's internal level-start routine to avoid deleting or changing the user's save data.

**Subagent claims I checked rather than trusted:**
- It assumed `VRPauseMenu.Instance`, `.IsOpen`, `.HideMenu(bool)` were public — verified, all three
  exist (`VRPauseMenu.cs:78, 80, 324`).
- It flagged `sceneAnchors` deserialization as its "biggest guess" — verified in the live Editor,
  populated correctly (size 2, right values) in both scenes.
- It asked whether MainScene's `VRPauseMenu.showOnStart` was true, which would have made its
  every-frame re-hide a behaviour regression — verified `showOnStart=False`, so the concern is moot.

**Known limitation carried forward:** the MainScene anchor is still `(-3.04, 0.95, -1.342)`, the
main-menu hallway coordinate from Issue 13. W2 preserves that deliberately; **W3 fixes it** by making
the loading screen head-relative, at which point the anchors become redundant.

### W3 — Loading screen head-relative ✅

- **Commit:** the W3 commit containing this log entry.
- **Changed:** `VRLoadingScreen` no longer exposes or executes fixed loading-screen placement.
  Removed `SceneAnchor`, `sceneAnchors`, the fixed position/rotation fields, the fixed-mode switch,
  and the active-scene anchor resolver. `PlaceInFrontOfPlayer()` now has one authoritative path:
  `camera position + camera forward * 1.15 m + camera up * heightOffset`, with the existing
  camera-relative rotation and world scale preserved.
- **Why the switch was removed instead of defaulted off:** both scene instances serialized
  `useFixedWorldPlacement=true`, which overrides a C# field initializer. Changing only the default
  would have left Issue 13 unfixed. Removing the obsolete schema also prevents the two placement
  systems from drifting back apart.
- **Scope:** script-only runtime behavior. `VRMainMenu` and `VRPauseMenu` fixed placement were not
  changed. Scene assets were inspected live and left clean/unsaved; their old unrecognized YAML keys
  are ignored by Unity and can be normalized by a future intentional scene save.

**QA status:**
- ✅ Fresh Unity compilation: `scriptCompilationFailed=false`; rebuilt `Assembly-CSharp.dll` was
  29 seconds old at verification. Reflection and `SerializedObject` readback confirmed every
  obsolete loading-screen placement field is absent and the placement method remains present.
- ✅ Cross-scene Play Mode measurement: the same persistent instance stayed exactly `1.1500 m` from
  `Camera.main` before and after activation/player spawn, with `0.00000 m` position error and
  `0.0000°` rotation error at every recorded sample. UI rays stayed suppressed and the pause menu
  stayed closed until W2 completed the transition.
- ✅ In-maze Game View evidence: `Docs/QA/w3-head-relative-main-scene.png`.
- ✅ Final five-minute Unity Console query returned zero Errors and zero Exceptions; both inspected
  scenes were clean after the test.
- ⚠️ **`PENDING-HEADSET`:** continuous head tracking, peripheral coverage, and any perceived
  late-update jitter still require a physical Quest comfort check.

### W4 — Menu occlusion and viewing distance ✅

- **Commit:** the W4 commit containing this log entry.
- **Occlusion:** `VRPauseMenu` now routes fixed Start/Start→Settings and dynamic
  Pause/Captured/Time Up/Pause→Settings placement through one volume-aware path. It casts the
  oriented world-space `BoxCollider` footprint instead of one centre ray, takes the nearest hit,
  backs off by the existing 0.15 m padding, and validates the final pose with an overlap check.
- **Mask:** `wallMask` is now explicit `Default | Echoable` (`1025`) instead of `~0`. This matches
  the live maze geometry layers, excludes the XR body on `Ignore Raycast`, and continues to ignore
  triggers through `QueryTriggerInteraction.Ignore`.
- **No-fit fallback:** if the 1.8 m × 1.12 m pause panel cannot fit anywhere along the preferred
  line, placement retries from 90% down to 10% scale, selecting the largest scale that reaches the
  0.45 m comfort minimum when possible. The authored full scale is restored and recalculated every
  time the menu opens. If even 10% has no clear pose, the previous pose is retained and an Error
  tells the player/tester to move away from the wall and reopen the menu.
- **Distance:** the main-menu fixed anchor moved from `(0, 1.15, -8.75)` to
  `(0, 1.15, -9.75)`. Runtime measurement changed from 2.522 m to **1.536 m centre distance**
  (1.500 m forward depth). The dormant camera-relative fallback is also 1.5 m. Gameplay pause-menu
  distance deliberately remains 2.1 m because its panel is wider (1.8 m versus 1.44 m).
- **Scene normalization:** saving both scenes intentionally removed W3's obsolete serialized
  loading-screen anchor keys. Those fields no longer exist in `VRLoadingScreen`; this is schema
  cleanup only, not a W4 loading-screen behavior change.

**QA status:**
- ✅ Fresh Unity compilation with `scriptCompilationFailed=false`; reflection found the new
  placement/overlap helpers in a freshly rebuilt `Assembly-CSharp.dll`.
- ✅ Deterministic physics cases passed for clear space, centred obstruction, side/corner-only
  obstruction missed by the old ray, fixed-anchor obstruction, close wall, ignored trigger, and an
  excluded-layer collider. Final overlap count was zero in every case.
- ✅ A 24-pose maze sweep across three player positions and eight yaw angles produced 24/24 clear
  final panel volumes. Scale-to-fit was required in the deliberately narrow cross-corridor cases.
  Final targeted regression checks also confirmed avoidance-on pulls the panel to 1.345 m while
  avoidance-off preserves the exact preferred pose and scale.
- ✅ Normal gameplay pause pose remained full-size at 2.101 m with no geometry overlap. Evidence:
  `Docs/QA/w4-pause-clear-main-scene.png`. Side-only obstruction evidence:
  `Docs/QA/w4-pause-side-wall-main-scene.png`.
- ✅ Main-menu runtime readback measured 1.500 m forward / 1.536 m centre distance at full authored
  runtime scale. Evidence: `Docs/QA/w4-main-menu-distance.png`.
- ✅ Final one-minute Unity Console query returned zero Errors; both scenes read back clean and
  `MainMenuScene` was restored as the active scene.
- ⚠️ **`PENDING-HEADSET`:** confirm the closer main menu is readable/comfortable and controller rays
  still feel natural. Also test the emergency scale-to-fit behavior near a wall; one deliberately
  extreme sweep pose remained at 0.43 m because no scale could meet the 0.45 m minimum, although it
  was geometry-clear.

**Known limitations:** the explicit mask is practical, not semantically geometry-only—solid props
on `Default` can still shorten placement. Placement remains world-locked after opening and is not
continuously rechecked against moving geometry. The main menu itself has no runtime occlusion cast;
its static authored anchor was verified clear and locomotion remains disabled.

### Reversal — W6 styling approach (was: port to UI Toolkit)

Detailed scoping turned up a blocker I did not know about when I recommended the port, so I am
**changing my recommendation** and will not port unless you overrule me:

- `TutorialRuntimeObserver.cs:78` reads TutorialDirector's private field **by reflection**:
  `ReadField<TMP_Text>(type, "tutorialPromptText")`. A UI Toolkit `Label` **is not** a `TMP_Text`, so
  the port makes this return null, and `:153` then emits a hard `Debug.LogError` every tutorial run.
  The diagnostic channel at `:124, 213-219` goes permanently dead too.
- **TMP features with no USS equivalent:** text outline (`outlineWidth 0.22`) and auto-sizing
  (`fontSizeMin 10.5` / `fontSizeMax 15`). Auto-sizing matters — `InteractionMessage` is 76
  characters and *will* clip at a fixed font size.
- The world-space transform math would need re-deriving, since UI Toolkit here needs the negative-X
  mirror and a 180° flip that the uGUI prompt does not.

**Revised plan for W6:** keep the prompt as uGUI, and restyle it by hand to match the menu's visual
language (panel frame, corner brackets, `--echo-cyan`/`--echo-white` palette, `.signal-line`
divider). You lose single-source-of-truth styling, which is a real cost — but you keep working
diagnostics, text auto-fit, and the existing pose math. **The anchoring fix (Issue 3) is unaffected
and proceeds as planned.** Say the word if you'd rather take the port and its costs.
