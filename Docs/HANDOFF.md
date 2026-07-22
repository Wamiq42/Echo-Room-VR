# Echo Room VR — UI Fix Pass · Session Handoff

**Purpose.** Everything a fresh Claude Code session — or a different AI model — needs to continue this
UI-fix pass without re-discovering what's already known. Read this top to bottom before touching code.

**Last updated:** after W2 committed (`068dee8`). Branch: `Development-Phase`. No pushes made.

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
   ever land on the wrong button, this is why. Untangling it is part of W5; don't bake it into assets.
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
| **W3** | Loading screen head-relative | 13 | ⬜ next |
| **W4** | Menu occlusion + distance | 4, 7b | ⬜ |
| **W5** | Editor authoring parity | 1 | ⬜ |
| **W6** | Tutorial prompt anchor + style | 3, 11 | ⬜ |
| **W7** | Timer fairness | 10 | ⬜ |
| **W9** | Interaction layers + safe cleanup | adjacent | ⬜ |

**Out of scope this pass (user's call):** Issue 12 / Issue 14 (in-game HUD / wrist objective panel)
and Issue 7a (panel resolution reflow to 1920×1080). Don't start these without the user.

## 6. ⏳ Pending playtest checks — THE USER MUST DO THESE

None of the three shipped fixes are fully verified, because driving a VR controller in Play Mode isn't
reliably scriptable. Ask the user to confirm, in a headset or the XR simulator:

1. **W2 (highest priority):** press NEW GAME → the loading screen appears *before* the maze is visible,
   stays up across the scene swap, and hides only once inside the maze — with no pause menu visible and
   controller rays dead for the whole transition.
2. **W1:** point a controller ray at a menu button → it highlights and the trigger activates it.
3. **W8:** in the main menu, press WASD (XR simulator) → the player does NOT translate, but mouse-look
   still works.

If W2 misbehaves, **stop and fix it before building W3/W4** — both build directly on it.

## 7. Remaining work — how to execute each

Full specs are in `UI_FIX_PLAN.md` under each `W#`. Condensed here with the dependency order.

- **W3 — loading screen head-relative** (do next; small). Switch `VRLoadingScreen` to its non-fixed
  placement branch so it follows the head instead of the `(-3.04, 0.95, -1.342)` hallway anchor; then
  remove the now-redundant `sceneAnchors` W2 added. Depends on W2's camera re-resolution. Script-only;
  loading-screen file only.
- **W4 — menu occlusion + distance** (Issues 4, 7b). `VRPauseMenu.cs:547-553`: replace the single
  centre `Physics.Raycast` with a `BoxCast` sized to the panel on an explicit geometry layer mask,
  applied on both the fixed and dynamic paths; pull menu viewing distance toward ~1.5 m (measured
  current main-menu distance is 2.5 m). Distance is `PENDING-HEADSET`. Owns `VRPauseMenu.cs`.
- **W5 — editor authoring parity** (Issue 1). Move UI Toolkit authoring data into assets: `<Style>` in
  UXML, `display:none` USS defaults for inactive screens, serialized `UIDocument` fields matching what
  `Awake()` assigns — so the editor shows one styled screen instead of all screens stacked unstyled
  (`Docs/QA/issue-01-before-editmode-stacking.png` is the "before"). Do NOT use `[ExecuteAlways]`.
  Sequence after W1 (done) so the mirrored-transform hack isn't codified.
- **W6 — tutorial prompt** (Issues 3, 11). **Anchor:** `TutorialDirector.cs:345` re-drives the prompt
  from the controller every frame → make it head-relative, world-locked on show (place once in front
  of the player, then freeze). **Style: DECISION PENDING (see §8)** — recommended path is restyle the
  uGUI prompt by hand, NOT port to UI Toolkit. Owns `TutorialDirector.cs`.
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

**Parallelisation & contention:** W3, W6 touch disjoint files and can run alongside others. **Scene
files are the bottleneck** — only one worker may hold `MainMenuScene.unity` / `MainScene.unity` at a
time, and the lead should serialise all scene writes through `script-execute` rather than hand-editing
YAML. Full contention map is in the plan.

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
   User can overrule. Until decided, do only the W6 *anchoring* half.
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
4. Pick the next `⬜` item in dependency order (W3 next). Read the files it owns *fully* before editing.
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
