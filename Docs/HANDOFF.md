# Echo Room VR — UI Fix Pass · Session Handoff

**Purpose.** Everything a fresh Claude Code session — or a different AI model — needs to continue this
UI-fix pass without re-discovering what's already known. Read this top to bottom before touching code.

**Last updated:** after W9 completion and full in-scope plan QA (2026-07-23). Branch:
`Development-Phase`. No pushes made.

---

## 0. The two documents that matter

1. **[`Docs/UI_ISSUES_2026-07-20.md`](UI_ISSUES_2026-07-20.md)** — the investigation. 14 issues,
   each with confirmed root cause, file/line evidence, and options discussed with the user. Read the
   issue before you fix it.
2. **[`Docs/UI_FIX_PLAN.md`](UI_FIX_PLAN.md)** — the plan of record and running log. Work items
   `W0`–`W9`, dependency order, file-contention map, per-item acceptance criteria, and a completed-work
   log with QA evidence. **This is the source of truth for status.** Update it as you go.

This handoff summarises both but does not replace them.

---

## 1. What this project is

- **Echo Room** — a VR horror/puzzle game. Premise: *"play blind, see with sound"* — a blind survivor
  in a dead research facility navigates by echolocation (a sonar "ping" reveals geometry briefly).
- **Unity 6000.3.8f1**, **URP 17.3.0**, **XR Interaction Toolkit 3.3.1**, target is **Meta Quest**.
- **UI is UI Toolkit** (`UIDocument` + UXML/USS, world-space panels). The tutorial ending fade remains
  a runtime-built uGUI canvas because it is a camera-bound stereo screen fade, not a menu/prompt.
- Two scenes only: `MainMenuScene` (build index 0) and `MainScene`. **Levels are prefabs instantiated
  into MainScene**, not separate scenes. This matters a lot — see §5.
- Key scripts live under `Assets/_EchoRoom/Scripts/`. UI is in `.../Scripts/UI/`, managers in
  `.../Scripts/Managers/`, tutorial in `.../Scripts/Tutorial/`.

## 2. The working arrangement (important)

The user (Wamiq) works as a **single point of contact**. He talks only to the lead session; the lead
delegates to subagents, **independently verifies their output** (compile it, read the diff, check
their claims — never relay self-assessment as fact), reviews, commits, and reports status. He does not
want to review raw subagent output or coordinate parallel work himself. If you are picking this up as
the lead, you inherit that role.

His stated preferences (also in Claude's cross-project memory):
- **Brutal honesty.** No yes-man. Flag guesses, report what's *not* done and what's unverified.
- **One commit per issue**, directly on `Development-Phase`. No branches. No pushing.
- **Decide-and-document** on ambiguity — make the call, record the rationale, skip only genuinely
  irreversible actions (deleting scripts/assets) without asking.
- **No `Co-Authored-By` trailer** on commits.

## 3. Environment — how to actually make changes

Unity is **open and connected** via the Ivan Murzak Unity-MCP bridge (`http://localhost:26566`). It
must stay open. **Power cuts are a real risk on his machine** — commit at every task boundary so work
is resumable.

**Driving Unity from the shell** — the CLI wrapper is `npx unity-mcp-cli`. Examples that work:

```bash
# readiness / editor state
npx unity-mcp-cli run-system-tool ping --input '{}'
npx unity-mcp-cli run-tool editor-application-get-state --input '{}'

# run arbitrary C# in the editor (best for reliable, verifiable scene/asset edits).
# Pass a JSON file with {"isMethodBody": true, "csharpCode": "..."}; have the C# WRITE
# RESULTS TO A FILE and cat it — console-get-logs is too noisy to parse.
npx unity-mcp-cli run-tool script-execute --input-file args.json
```

**Screenshots for QA** — a helper turns the 1MB base64 blob into a PNG:

```bash
node .claude/tools/unity-shot.js Docs/QA/name.png                    # game view
node .claude/tools/unity-shot.js Docs/QA/name.png screenshot-scene-view
```

Then `Read` the PNG to view it. Screenshots land in `Docs/QA/`.

**MCP quirks learned the hard way:**
- `gameObjectRef` wants an object, not a string: `{"gameObjectRef":{"name":"Main Menu"}}`.
- `scene-open` via the CLI tool returns HTTP 500 with "not valid or not found." **Open scenes with a
  `script-execute` calling `EditorSceneManager.OpenScene("Assets/.../X.unity", ...Single)` instead.**
- The `Write` tool cannot create a file that a Bash heredoc already made — write the JSON with the
  `Write` tool from the start.
- `npx.cmd` cannot be `spawn`ed directly on Windows (`EINVAL`); `unity-shot.js` uses `execSync` with
  `shell:true`.

## 4. ⚠️ Gotchas that will bite you if you don't know them

1. **A failed compile still shows `IsCompiling: false` with stale assemblies loaded.** "No console
   errors" is NOT proof a change compiled. To actually verify, `script-execute` a reflection check
   that the new type/member exists AND compare the `Assembly-CSharp` file's last-write time to now
   (see the W2 log in the plan for the exact pattern). Do this for every script change.
2. **`console-get-logs` is extremely noisy** (MetaXR, toolbar, per-frame `[PlayerInputManager] Move`
   debug spam). Filter hard, or better, have `script-execute` write results to a file and read that.
   When counting `error CS`, check they aren't from *your own* earlier throwaway scripts.
3. **World-space UI panels use a negative-X scale hack** (`localScale = (-0.0016, 0.0016, 0.0016)`)
   plus a 180° rotation, so panels are geometrically back-facing/mirrored. Unity even warns:
   *"BoxCollider does not support negative scale."* This is why Scene view shows mirrored text
   ("ЈАИDIƧ"). It did NOT cause the ray bug (that was the trigger enum, W1 — fixed), but if clicks
   ever land on the wrong button, this is why. W5 normalized the authored scale magnitude to the
   existing runtime value but deliberately retained this orientation convention; untangling it now
   requires a separate collider/orientation/controller-targeting pass with headset verification.
4. **`QueryTriggerInteraction` enum is `UseGlobal=0, Ignore=1, Collide=2`.** Easy to get backwards.
5. **W9 removed the remaining UI/timer object-name scans.** `VRMainMenu` owns exact serialized
   pointer refs, `MazeLevelTimer` owns exact controller/head refs, and the persistent loader discovers
   only active `XRRayInteractor`s with UI interaction enabled after each scene load. Do not replace
   the loader's type-safe current-scene scan with fixed scene refs; it survives Single-mode loads.
6. **`GameManager` exists only in MainScene** (zero in MainMenuScene). Anything that must survive the
   menu→game transition cannot live on it — this invalidated the original W2 plan mid-flight.
7. **Line-ending warnings** (`LF will be replaced by CRLF`) on every commit are cosmetic; ignore.
8. **Uncommitted churn not ours:** `.mcp.json`, `Packages/manifest.json`, `Packages/packages-lock.json`,
   `ProjectSettings/QualitySettings.asset`, some `Assets/Plugins/NuGet/*.dll`, and a batch of
   `.claude/skills/navigation-*` folders show as modified/untracked. These are from tooling/plugins,
   **not this UI work** — leave them alone unless the user says otherwise.

## 5. Current state — done vs remaining

| # | Work | Issues | Status |
|---|---|---|---|
| W0 | Merge conflicts resolved | — | ✅ `9886acd` |
| — | Plan + issue log + QA tooling | — | ✅ `1b36f42` |
| W1 | Ray→UI input (trigger enum) | 2 | ✅ `052bb7c` |
| W8 | Main-menu simulator WASD | 9 | ✅ `8bcecdc` |
| W2 | Persistent transition owner | 8, 5, 6 | ✅ `b78075f` |
| W3 | Loading screen head-relative | 13 | ✅ completed and QA-verified |
| W4 | Menu occlusion + distance | 4, 7b | ✅ completed and QA-verified |
| W5 | Editor authoring parity | 1 | ✅ completed and QA-verified |
| **W6a** | Tutorial prompt world-lock | 3 | ✅ completed; headset check pending |
| **W6b** | Tutorial prompt styling | 11 | ✅ completed; headset check pending |
| **W7** | Timer fairness | 10 | ✅ `92715c4`; headset check pending |
| **W9** | Interaction layers + safe cleanup | adjacent | ✅ completed; headset check pending |

**Out of scope this pass (user's call):** Issue 12 / Issue 14 (in-game HUD / wrist objective panel)
and Issue 7a (panel resolution reflow to 1920×1080). Don't start these without the user.

**W4 result worth carrying forward:** `VRPauseMenu` now uses an oriented panel-volume cast plus final
overlap validation on `Default | Echoable` (`1025`) for both fixed and dynamic paths. If no full-size
pose fits, it temporarily scales down in 10% steps; it restores the authored full scale on the next
open. The normal pause pose remains 2.1 m. Main-menu placement is still a static authored anchor,
now 1.500 m forward / 1.536 m centre distance and verified clear with locomotion disabled. The mask
is practical rather than semantic—solid `Default` props can affect placement—and the menu remains
world-locked after opening. Saving both scenes normalized W3's obsolete loading-anchor YAML keys.
QA images: `Docs/QA/w4-main-menu-distance.png`, `Docs/QA/w4-pause-clear-main-scene.png`, and
`Docs/QA/w4-pause-side-wall-main-scene.png`.

**W5 result worth carrying forward:** `VRMenu.uxml` and `VRLoadingScreen.uxml` now link their
matching stylesheets directly, so Edit Mode no longer depends on `Awake()` for styling or hidden
screen defaults. Both loading UIDocuments serialize Absolute/Fixed 900×560 layouts; MainScene's
pause UIDocument now serializes `VRMenu.uxml`; and the MainMenuScene `Main Menu` authored scale now
matches runtime at `(-0.0016, 0.0016, 0.0016)`. Runtime setup remains as an idempotent fallback and
was verified to leave exactly one stylesheet attached. MainScene intentionally shows the shared
UXML's representative Start screen in Edit Mode; runtime state selection is unchanged. Before/after
evidence: `Docs/QA/issue-01-before-editmode-stacking.png` and
`Docs/QA/w5-after-editmode-mainmenu.png`.

**W6 result worth carrying forward:** the tutorial prompt is now a runtime world-space UI Toolkit
`UIDocument`, not uGUI/TMP. It reuses `VRMenuPanelSettings.asset` and `VRMenu.uss`, with dedicated
`VRTutorialPrompt.uxml` / `VRTutorialPromptStyles.uss`. Each message is placed once from the current
head pose at `(0, -0.10, 0.85)` metres and remains world-locked until the next message. The document
stays active while its root handles display/opacity; every element ignores picking and no collider or
XR UI manager is added. A typed director snapshot replaced observer reflection/TMP coupling while
preserving the tutorial trace schema. The separate ending fade remains camera-bound uGUI. No scene
serialization was required. Five final captures live at `Docs/QA/w6b-01-sonar.png` through
`w6b-05-warning.png`.

**W7 result worth carrying forward:** timed mazes now wait for the loading transition before starting
at the full 180 seconds and showing their four-flash introduction. Automatic one-shot reveals fire at
60/30/10 seconds for 3.0/3.75/4.5 seconds with progressively stronger playback of the existing
heartbeat clip. Pause freezes the countdown, visible warning, fade, and cue lifetime together; manual
A / Gamepad South / keyboard `T` reveal remains independent. Successful expiry hides the controller
readout before the time-up menu opens. The UI Toolkit MOVE prompt teaches `press A to reveal the
timer`; Maze E remains intentionally untimed pending a separate design decision. Final evidence:
`Docs/QA/w7-tutorial-timer-reveal.png`, `w7-10-second-warning.png`, and `w7-time-expired.png`.

**W9 result worth carrying forward:** both EventSystem `PanelInputConfiguration` components now use
`4294967291` / `Physics.DefaultRaycastLayers`. MainMenu's `VRMainMenu` serializes Right then Left Menu
UI Ray roots; MainScene's `MazeLevelTimer` serializes its own Right Controller transform and the XR
Main Camera. The loader keeps a typed, current-scene `XRRayInteractor.enableUIInteraction` scan
because it is persistent and cannot retain references into unloaded scenes. Missing menu refs fail
loudly; there is no legacy name-discovery fallback. Rename/decoy QA passed, and a persistent observer
sampled 2,121 frames across real menu→game→menu transitions with zero active UI pointers while
transitioning. Deferred `VRFrontEndMenu.cs`, `VRGameplayMenu.uxml`, and `EchoPuzzleController.cs`
remain untouched.

## 6. ⏳ Pending playtest checks — THE USER MUST DO THESE

W2's complete menu→maze lifecycle and W3's exact head-relative placement are now verified in Editor
Play Mode. The remaining physical-input/comfort checks are:

1. **W1:** point a controller ray at a menu button → it highlights and the trigger activates it.
2. **W8:** in the main menu, press WASD (XR simulator) → the player does NOT translate, but mouse-look
   still works.
3. **W3/W2 headset pass:** start a maze and confirm the loading cover remains centered while moving
   the head, does not visibly jitter, covers enough peripheral view, and stays up across activation.
4. **W4:** confirm the closer main menu (1.500 m forward / 1.536 m centre distance) is comfortable,
   readable, and still easy to target with controller rays. Open Pause while close to/looking across
   a wall and confirm the emergency pull-in/scale-to-fit behavior is preferable to clipping.
5. **W5:** confirm the authoring changes did not alter perceived scale, stereo readability, or
   physical controller-ray hover/trigger behavior on Quest.
6. **W6a/W6b:** verify the provisional 0.856 m centre distance is comfortable and readable; move the
   headset and controller independently to confirm no jitter, swimming, or recentering; turn away and
   back; and inspect all five prompt states in both eyes to confirm the negative-X/180° UI Toolkit
   orientation is readable, each new message reanchors comfortably, controller rays are not
   intercepted, and nearby geometry does not obscure the panel. The full sequence already passes in
   Editor Play Mode; these are physical Quest checks only.
7. **W7:** in a timed maze, confirm physical A reveals the timer; inspect it while moving the right
   hand and in both eyes; and confirm the 60/30/10 heartbeat cues are audible, comfortable, and
   appropriately urgent without overpowering gameplay audio.
8. **W9:** repeat physical controller hover/trigger in both main and pause menus, including immediately
   after both scene transitions, and watch for any one-frame pointer flash while the loading cover is
   still active. Desktop frame-by-frame observation found none.

## 7. Remaining work

All in-scope W0–W9 code work is complete. The remaining acceptance work is the consolidated physical
Quest checklist in §6. Issue 7a and Issues 12/14 remain explicitly deferred; the unused/duplicate
assets noted in W9 remain intentionally undeleted.

**Parallelisation & contention:** W4, W5, and W6 own distinct primary code/assets, but **scene files
are the bottleneck** — only one worker may hold `MainMenuScene.unity` / `MainScene.unity` at a time,
and the lead should serialise all scene writes through `script-execute` rather than hand-editing YAML.
Full contention map is in the plan.

**Delegation pattern that worked:** give a subagent a self-contained spec that says SCRIPT FILES ONLY /
no scene edits / no MCP / no commit / minimal diff / match existing style, and list the exact files it
owns and must not touch. Then the lead compiles, reviews the diff, does the scene wiring, screenshots,
and commits. (W2 and W8 were done this way.)

## 8. Open decisions the user still needs to make

1. **Issue 12 / 14 (wrist HUD/objective panel)** — deferred by the user to "later, when other issues
   are resolved." Data survey is already in the issue doc (timer/level-name free; "2 of 3 switches"
   exists but private; objective text and a multi-step phase model must be authored).
2. **Maze E has no time limit** (name-match falls through to `-1`). W7 intentionally preserved that
   behavior because adding a limit is a design change, not part of the fairness defect.

## 9. Quick-start for a fresh session / different model

1. Read `Docs/UI_FIX_PLAN.md` (status + specs) and this file. Skim `Docs/UI_ISSUES_2026-07-20.md` for
   the issue you're about to touch.
2. Confirm Unity is alive: `npx unity-mcp-cli run-system-tool ping --input '{}'`.
3. Verify the working tree matches the plan: `git log --oneline -8` should end at the commit named in
   §5. Ignore the non-ours churn in §4.8.
4. Confirm the execution table contains no in-scope `⬜` item. Run the Quest checklist in §6 before
   treating physical comfort/input as accepted.
5. If new work is authorized, preserve the one-issue/one-commit discipline, compile-verify per §4.1,
   and update both the plan log and `PROJECT_MEMORY.md`.
6. Report status honestly, including every remaining `PENDING-HEADSET` item.

### If you are a different AI model

Everything here is tool-agnostic except the exact tool *names* (`script-execute`, `unity-shot.js`,
etc.), which are specific to this Claude Code + Unity-MCP setup. The *facts* — root causes, file/line
references, the gotchas in §4, the design decisions — all transfer. If your harness can't reach the
Unity-MCP bridge, you can still do all the pure-`.cs` work and hand the user a precise scene-wiring
checklist to apply in the Unity Inspector themselves (that's how W8 was structured, and it worked).
