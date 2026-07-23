# Echo Room VR — UI Fix Pass · Session Handoff

**Purpose.** Everything a fresh Claude Code session — or a different AI model — needs to continue this
UI-fix pass without re-discovering what's already known. Read this top to bottom before touching code.

**Last updated:** after W5 completion and QA (2026-07-23). Branch: `Development-Phase`. No pushes made.

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
- **UI is UI Toolkit** (`UIDocument` + UXML/USS, world-space panels), *not* uGUI — with one exception:
  the tutorial prompt and screen fades are runtime-built uGUI canvases.
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
5. **Three separate name-string GameObject lookups exist** and silently break on rename:
   `VRMainMenu.cs` (`"Menu UI Ray"`), `MazeLevelTimer.cs` (`"Right Controller"`), and the tutorial
   prompt. W9 replaces them.
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
| **W6b** | Tutorial prompt styling | 11 | ⏸ awaiting user choice |
| **W7** | Timer fairness | 10 | ⬜ next executable item |
| **W9** | Interaction layers + safe cleanup | adjacent | ⬜ |

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

**W6a result worth carrying forward:** the tutorial prompt remains a runtime world-space uGUI/TMP
canvas, but it no longer follows the Right Controller. Each message is placed once from the current
head pose at `(0, -0.10, 0.85)` metres and remains world-locked until the next message. Missing-camera
placement hides the prompt and logs once. The `Move → Button` proximity update, prompt copy, TMP
auto-sizing/outline, fades, observer field contract, and ending sequence remain structurally intact.
The ending fade no longer falls back to non-stereo `ScreenSpaceOverlay`. No scene serialization was
required. QA image: `Docs/QA/w6-issue3-world-locked-prompt.png`.

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
6. **W6a:** verify the provisional 0.856 m centre distance is comfortable and readable; move the
   headset and controller independently to confirm no jitter, swimming, or recentering; turn away and
   back; and run all five natural prompt states to confirm each new message reanchors once and does
   not intersect nearby geometry.

## 7. Remaining work — how to execute each

Full specs are in `UI_FIX_PLAN.md` under each `W#`. Condensed here with the dependency order.

- **W6b — tutorial styling** (Issue 11). **DECISION PENDING (see §8):** manually restyle the existing
  uGUI/TMP prompt, or authorize a broader UI Toolkit port that also migrates observer diagnostics,
  auto-fit, outline treatment, and world-space transform behavior. Owns `TutorialDirector.cs`.
- **W7 — timer fairness** (Issue 10). 180 s/maze timer is invisible unless the A button is held.
  Add escalating auto-reveals at 60/30/10 s with an on-theme audio cue, keep manual reveal, and teach
  the reveal button in the tutorial. Depends on W2 (the level-start flash was firing behind the old
  loading screen). Player-facing copy must be drafted in the existing voice and shown to the user for
  approval. Owns `MazeLevelTimer.cs` (+ `TutorialDirector.cs` after W6).
- **W9 — interaction layers + safe cleanup.** Reconcile `PanelInputConfiguration.m_InteractionLayers`
  (`4294967291` in MainMenuScene vs `1075` in MainScene). Replace the three name-string lookups with
  serialized refs. **Do NOT delete** `VRFrontEndMenu.cs` (duplicate of `VRMainMenu`) or
  `VRGameplayMenu.uxml` (unused, but possible groundwork for the out-of-scope wrist panel) without the
  user — flag them instead.

**Parallelisation & contention:** W4, W5, and W6 own distinct primary code/assets, but **scene files
are the bottleneck** — only one worker may hold `MainMenuScene.unity` / `MainScene.unity` at a time,
and the lead should serialise all scene writes through `script-execute` rather than hand-editing YAML.
Full contention map is in the plan.

**Delegation pattern that worked:** give a subagent a self-contained spec that says SCRIPT FILES ONLY /
no scene edits / no MCP / no commit / minimal diff / match existing style, and list the exact files it
owns and must not touch. Then the lead compiles, reviews the diff, does the scene wiring, screenshots,
and commits. (W2 and W8 were done this way.)

## 8. Open decisions the user still needs to make

1. **W6 styling — port vs restyle.** Detailed scoping found that porting the tutorial prompt to UI
   Toolkit would break `TutorialRuntimeObserver.cs:78`, which reads the private field `tutorialPromptText`
   as a `TMP_Text` by reflection — a UI Toolkit `Label` isn't a `TMP_Text`, so it returns null and
   `:153` emits a hard `Debug.LogError` every tutorial run; it also loses TMP auto-sizing that the
   76-char `InteractionMessage` depends on. **Lead recommendation: keep it uGUI, restyle by hand.**
   User can overrule. W6a anchoring is complete independently; this decision now governs only W6b.
2. **Issue 12 / 14 (wrist HUD/objective panel)** — deferred by the user to "later, when other issues
   are resolved." Data survey is already in the issue doc (timer/level-name free; "2 of 3 switches"
   exists but private; objective text and a multi-step phase model must be authored).
3. **Maze E has no time limit** (name-match falls through to `-1`). Left as-is in W7; flagged for the
   user to confirm intentional.

## 9. Quick-start for a fresh session / different model

1. Read `Docs/UI_FIX_PLAN.md` (status + specs) and this file. Skim `Docs/UI_ISSUES_2026-07-20.md` for
   the issue you're about to touch.
2. Confirm Unity is alive: `npx unity-mcp-cli run-system-tool ping --input '{}'`.
3. Verify the working tree matches the plan: `git log --oneline -8` should end at the commit named in
   §5. Ignore the non-ours churn in §4.8.
4. Resolve the W6b styling choice with the user, or take the next executable `⬜` item (W7). Read the
   files it owns *fully* before editing.
5. Make the change (delegate if it parallelises), **compile-verify per §4.1**, screenshot if visual,
   review, commit one-per-issue, update the plan's running log with what was done and what's still
   `PENDING-HEADSET`/`PENDING-PLAYTEST`.
6. Report status honestly, including the pending playtest checks in §6.

### If you are a different AI model

Everything here is tool-agnostic except the exact tool *names* (`script-execute`, `unity-shot.js`,
etc.), which are specific to this Claude Code + Unity-MCP setup. The *facts* — root causes, file/line
references, the gotchas in §4, the design decisions — all transfer. If your harness can't reach the
Unity-MCP bridge, you can still do all the pure-`.cs` work and hand the user a precise scene-wiring
checklist to apply in the Unity Inspector themselves (that's how W8 was structured, and it worked).
