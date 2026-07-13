# Echo Room VR — Persistent Project Memory

This file is the durable, project-local memory for Echo Room VR. It is intended to be read by people and coding agents before work begins and updated whenever project changes are completed.

It begins on **2026-07-05**. Earlier project history is not assumed to be complete unless it is explicitly backfilled from verifiable sources such as version control, existing documentation, or Unity assets.

## Memory Rules

1. Record only verified facts. Do not guess scene hierarchy or object paths.
2. Use project-relative paths so the memory remains valid if the repository moves.
3. Unity objects use this format: `Assets/path/Scene.unity :: Root/Child/Object`.
4. Prefab objects use this format: `Assets/path/Prefab.prefab :: Root/Child/Object`.
5. List all files and Unity objects affected by a change, including created, modified, moved, and deleted items.
6. Include the reason, result, important decisions, verification, limitations, and next steps.
7. Keep the Change Journal append-only. Add a correction entry instead of silently altering older entries.
8. Never store secrets or sensitive personal data here.

## Current Project Index

This is a maintained summary of important known locations. Add entries when a file, scene, prefab, or object becomes relevant to completed work.

### Memory and documentation

| Purpose | Path |
| --- | --- |
| Agent-wide project instructions | `AGENTS.md` |
| Persistent project memory | `Docs/PROJECT_MEMORY.md` |
| Documentation index | `Docs/README.md` |
| Project roadmap | `Docs/ROADMAP.md` |
| Game design document | `Docs/GDD.md` |
| Game overview | `Docs/GAME_OVERVIEW.md` |
| Codebase analysis | `Docs/CODEBASE_ANALYSIS.md` |

### Unity scenes and objects

No scene hierarchy has been recorded yet. Populate this section only from a live Unity inspection or direct inspection of a serialized scene/prefab asset.

### Important code and assets

Add verified project paths here as they become relevant to completed changes.

## Active Decisions and Conventions

| ID | Decision | Reason | Status |
| --- | --- | --- | --- |
| DEC-0001 | Use `Docs/PROJECT_MEMORY.md` as the canonical project-local memory. | It is versionable, human-readable, and available across sessions and tools. | Active |
| DEC-0002 | Track only files and Unity objects affected by documented work, not every third-party asset in the repository. | A complete inventory would be noisy and would not describe change history. | Active |
| DEC-0003 | Direct Unity inspection is authoritative for scene, prefab, GameObject, and component paths. | It prevents stale or invented hierarchy references. | Active |

## Open Work and Known Limitations

- The memory is persistent within this repository, but it is not an automatic account-wide memory. A person or agent must read this file to use it.
- Changes made by tools or people who do not update this file will not appear automatically.
- History before 2026-07-05 has not yet been reconstructed.
- The initial live Unity scene/object index is pending a successful Unity Editor inspection.

## Change Journal

### MEM-0001 — Establish persistent project memory

- **Date:** 2026-07-05
- **Goal:** Create a durable, universal-within-the-project log for changes, paths, Unity objects, decisions, and verification.
- **Result:** Added a canonical project memory and root-level instructions requiring future agents to maintain it.
- **Files created:**
  - `AGENTS.md`
  - `Docs/PROJECT_MEMORY.md`
- **Files modified/moved/deleted:** None.
- **Unity objects affected:** None.
- **Decisions:** See `DEC-0001` through `DEC-0003`.
- **Verification:** Confirmed both files were written to the repository; content verification follows this entry's creation.
- **Limitations:** This establishes logging from this point forward. It does not claim to contain the project's full earlier history.
- **Follow-up:** Inspect the active Unity scene and add verified scene/object paths to the Current Project Index.

## Entry Template

Copy this section for each completed change:

```markdown
### CHANGE-ID — Short title

- **Date:** YYYY-MM-DD
- **Goal:**
- **Result:**
- **Files created:**
  - `project/relative/path`
- **Files modified:**
  - `project/relative/path`
- **Files moved:**
  - `old/path` -> `new/path`
- **Files deleted:**
  - `project/relative/path`
- **Unity objects affected:**
  - `Assets/path/Scene.unity :: Root/Child/Object`
- **Components/assets/settings:**
- **Decisions and assumptions:**
- **Verification:**
- **Known limitations:**
- **Follow-up:**
```


### TUT-SONAR-SEQ-001 — Staged sonar tutorial wall messages

- **Date:** 2026-07-06
- **Goal:** Extend the tutorial's opening sonar lesson so the first reveal teaches microphone input, the second reveal accepts either microphone or sonar-button input, and the player then receives a direction prompt.
- **Result:** The initial sonar instruction fades out after the first accepted ping and the microphone instruction fades in. A second accepted ping from either input fades to Go straight, then turn left. Approaching within 5 metres of the Interaction Wall fades the direction prompt into the existing interaction instruction.
- **Files created:** None.
- **Files modified:**
  - Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab
  - Docs/PROJECT_MEMORY.md
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Sonar Wall Tip
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Microphone Wall Tip
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Direction Wall Tip
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Interaction Wall Tip
- **Components/assets/settings:** Added editable TextMeshPro objects Microphone Wall Tip and Direction Wall Tip, copied from the existing sonar text transform and styling. Both are inactive in the prefab and controlled by TutorialDirector. PingEmitter.OnPingEmitted remains the common accepted-reveal signal; MicPingTrigger reaches it through PingEmitter.RequestPing.
- **Decisions and assumptions:** Kept each message as a separate prefab object so its wording, position, scale, and styling remain editable in Unity. The direction message remains visible until the existing Interaction Wall proximity condition is reached.
- **Verification:** Unity compilation completed with no script compilation failure. In Play Mode, first sonar emission changed Ping -> SecondReveal and fully faded to the microphone message. A second reveal through PingEmitter.RequestPing changed SecondReveal -> Move and fully faded to the direction message. A separate restart verified the second reveal through the sonar-button emission path also changed SecondReveal -> Move and displayed the direction message. Moving the player to the Interaction Wall changed Move -> Button, hid the direction text, and fully displayed the interaction text. Recent tutorial logs contained no runtime error.
- **Known limitations:** The microphone code path was verified through the same PingEmitter.RequestPing callback used by MicPingTrigger; a physical acoustic signal into the user's microphone was not generated by the automated test.
- **Follow-up:** User can adjust both new text objects directly in the tutorial prefab if different wording or wall placement is desired.


### TUT-SONAR-SEQ-002 — Require sonar control before microphone lesson

- **Date:** 2026-07-06
- **Goal:** Correct the opening lesson so microphone input cannot advance the first tutorial instruction.
- **Result:** PingEmitter now records whether an accepted ping came from the sonar control or microphone request. While TutorialDirector is on the first Ping step, only a SonarControl ping advances to the microphone message. On the following SecondReveal step, either source remains accepted and advances to the direction message.
- **Files created:** None.
- **Files modified:**
  - Assets/_EchoRoom/Scripts/PingEmitter.cs
  - Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs
  - Docs/PROJECT_MEMORY.md
- **Files moved/deleted:** None.
- **Unity objects affected:** None — code-only source classification and tutorial gating change.
- **Components/assets/settings:** Added PingEmitter.PingInputSource and PingEmitter.LastPingInputSource. External RequestPing calls are classified as Microphone; normal EmitPing calls are classified as SonarControl.
- **Decisions and assumptions:** Keyboard Space, configured VR/controller sonar input, and the normal sonar button path all use the internal EmitPing route and count as SonarControl. MicPingTrigger uses RequestPing and counts as Microphone.
- **Verification:** Static Unity compilation check completed with compiling=False and scriptCompilationFailed=False. Confirmed both the source classification and first-step gate are present.
- **Known limitations:** Play Mode was intentionally not run because the user requested to test this correction personally.
- **Follow-up:** User will verify the sequence in Play Mode.


### TUT-END-001 — Spoiler-free tutorial ending

- **Date:** 2026-07-06
- **Goal:** End the tutorial with a vague danger warning, heartbeat, and Entity sound without showing or spawning the Entity.
- **Result:** The interaction instruction now remains until both the tutorial button and lever are complete, then fades directly into the warning wall text. When the player remains within 3 metres of the warning wall for 2 seconds, the warning slowly fades over 2.5 seconds. A five-second camera-space black screen fade then runs while the Maze E heartbeat fades in/out and the Maze E Entity sound fades in, holds, and fades out. Completion returns to MainMenuScene. No Entity, fallback Entity, threat movement, route-opening, or first-level auto-load remains in the tutorial ending.
- **Files created:** None.
- **Files modified:**
  - Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs
  - Assets/_EchoRoom/Scripts/Tutorial/TutorialRuntimeObserver.cs
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab
  - Docs/PROJECT_MEMORY.md
- **Files moved/deleted:** None in this change. The Tutorial Entity had already been removed from the prefab by the user and was verified absent before implementation.
- **Unity objects affected:**
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Entity Wall Tip
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Tutorial Ending Audio
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Tutorial Ending Audio/Entity Sound
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Tutorial Ending Audio/Heartbeat
- **Components/assets/settings:** Entity Sound uses AudioSource with Assets/Audio/Entity Sound.mp3, loop enabled, non-spatial, target volume 0.72. Heartbeat uses AudioSource with Assets/Audio/universfield-fast-heartbeat-151928.mp3, loop enabled, non-spatial, target volume 0.65. Warning TextMeshPro text is: Sonar can attract unwanted attention. Something dangerous may be listening.
- **Decisions and assumptions:** Reused the exact clips assigned under Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Entity and Maze_5x5_E/Entity/Player Heartbeat Audio. Used non-spatial ending audio to avoid implying the hidden Entity's location. Kept the existing Entity wall and Entity Wall Tip object names so the user's prefab placement remains intact.
- **Verification:** Unity static compilation completed with compiling=False and scriptCompilationFailed=False. Verified the tutorial prefab contains both ending AudioSources with the correct clips and volumes, the user-removed Tutorial Entity remains absent, the warning text is assigned, the runtime observer tracks warning timing and both audio sources, and obsolete TutorialThreat, DangerPing, fallback Entity, and LoadFirstLevelAfterTutorial references are absent.
- **Known limitations:** Play Mode was not run; the user previously stated they would test the tutorial flow personally.
- **Follow-up:** In the user's Play Mode test, confirm the VR camera-space black fade fills both eyes and adjust the 2.5-second warning fade or five-second ending timing if desired.


### TUT-INTERACT-001 — Order-independent tutorial interactions

- **Date:** 2026-07-06
- **Goal:** Allow the tutorial button and lever to be activated in either order.
- **Result:** TutorialDirector records button and lever activation independently. The first valid interaction moves the lesson into a waiting-for-the-other state; once both flags are true, the interaction text transitions to the warning and the ending sequence can continue.
- **Files created:** None.
- **Files modified:**
  - Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs
  - Assets/_EchoRoom/Scripts/Tutorial/TutorialRuntimeObserver.cs
  - Docs/PROJECT_MEMORY.md
- **Files moved/deleted:** None.
- **Unity objects affected:** None — code-only interaction sequencing and diagnostic logging change.
- **Components/assets/settings:** Added private buttonActivated and leverActivated runtime flags. The observer heartbeat now reports both flags and validates both reflection fields.
- **Decisions and assumptions:** Only events from interactables under the active tutorial level root count. Both button-first and lever-first sequences are accepted; one activation of each is sufficient.
- **Verification:** Static Unity compilation completed with compiling=False and scriptCompilationFailed=False. Confirmed both handlers set their independent flag, completion requires both flags, and the observer contains both diagnostic fields.
- **Known limitations:** Play Mode was not run; the user is testing the tutorial flow.
- **Follow-up:** Review Logs/TutorialRuntime.log after the next test if either flag fails to register.


### FOLIAGE-MAT-001 — Restore stylized foliage textures

- **Date:** 2026-07-06
- **Goal:** Repair the imported stylized foliage model, whose foliage cards appeared black despite the supplied texture folder.
- **Result:** Reconfigured the material already assigned to every renderer in the FBX as a URP Lit foliage material. The base-color texture and normal map are assigned, alpha clipping removes the opaque card backgrounds, and two-sided rendering keeps leaves visible from both sides.
- **Files created:** None.
- **Files modified:**
  - `Assets/stylized-foliage/textures/New Material.mat`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** None — no scene or prefab object uses this material in the currently opened scenes, and no scene or prefab asset was saved.
- **Components/assets/settings:** `Assets/stylized-foliage/source/FOLIAGE.fbx` has ten renderers and all ten reference `Assets/stylized-foliage/textures/New Material.mat`. The material now uses `Universal Render Pipeline/Lit`, `Assets/stylized-foliage/textures/FOLIAGE_low_foliage_plane_BaseColor.png` as Base Map, and `Assets/stylized-foliage/textures/FOLIAGE_low_foliage_plane_Normal.png` as Normal Map. Alpha clipping is enabled with cutoff 0.35, render queue 2450, culling is Off for double-sided foliage, metallic is 0, and smoothness is 0.2. The separate opacity texture was not assigned because the BaseColor texture already contains the same opacity mask in its alpha channel.
- **Decisions and assumptions:** The previous `EchoRoom/EchoSonarReveal` shader is opaque and does not expose alpha clipping, so it rendered the transparent areas of every foliage card. The material was changed to URP Lit to provide ordinary visible foliage as requested. This means this material no longer participates in the custom sonar-reveal effect.
- **Verification:** Direct Unity inspection confirmed the project uses URP, the normal texture imports as a Normal Map, the BaseColor alpha ranges from 0 to 1, the repaired material retains both texture references, and all 10/10 FBX renderers use it. Isolated front and composite renders showed colored foliage with transparent card backgrounds. The temporary preview object was deleted and a loaded-scene scan found zero persistent scene renderers using the material.
- **Known limitations:** If the foliage must also react to sonar, a dedicated alpha-clipped variant of `EchoRoom/EchoSonarReveal` will be needed. The active `Assets/_EchoRoom/Scenes/MainScene.unity` reported dirty after temporary preview validation; the preview object was removed, the scene was not saved, and the dirty state was not cleared to avoid discarding or masking any pre-existing user edits.
- **Follow-up:** Optionally rename `New Material.mat` to a foliage-specific name, or add alpha-clipping support to the sonar shader for foliage that remains hidden until a ping.


### MAZE-TIMER-001 — Timed Maze A–D controller display

- **Date:** 2026-07-08
- **Goal:** Add an Inspector-configurable countdown to Maze A through Maze D, pause it with the VR pause menu, reveal it beside the right controller on demand, flash it four times at level start, and show a restart menu when time expires.
- **Result:** Maze A–D now start independent 180-second countdowns when GameManager reports the level loaded. The timer flashes four times, otherwise stays hidden, fades in for three seconds when the right-controller primary button (A) is pressed, and fades out again. Pausing stops the countdown; resuming continues it. Expiry opens the existing failure/restart menu with TIME EXPIRED / YOUR TIME IS UP. Maze E and the tutorial are intentionally unaffected.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Managers/MazeLevelTimer.cs`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Maze Timer Display` (runtime-generated and inspected in Play Mode)
- **Components/assets/settings:** Added MazeLevelTimer to GameManager. Inspector fields expose separate Maze A, B, C, and D durations (each defaults to 180 seconds), right-controller display placement/scale, three-second reveal duration, fade speed, colors, 30-second low-time threshold, and four-flash timing. Runtime TextMeshPro text is parented beneath Right Controller and billboarded toward Main Camera. Input bindings are right-hand XR primaryButton / Oculus buttonA, Gamepad South, and keyboard T for testing. VRPauseMenu now supports dedicated time-up failure copy while retaining its captured failure copy.
- **Decisions and assumptions:** Right-controller A was selected because physical interactions use the right trigger and pause uses the menu button. The timer uses scaled delta time so the existing VRPauseMenu timeScale=0 behavior pauses it exactly. The final 30 seconds turn orange-red for urgency. The timer remains stopped after level completion and is not applied to Maze E or the tutorial.
- **Verification:** Unity compilation completed with compiling=False and scriptCompilationFailed=False. MainScene saved clean with MazeLevelTimer attached and all four default durations confirmed at 180 seconds. In Play Mode, Maze A loaded; the countdown was staged at 10 seconds, VRPauseMenu entered Pause with timeScale=0, and remaining time was still exactly 10 after 2.5 real seconds. After resume it decreased normally. At expiry, timerRunning=False, remaining=0, menu state=Captured, timeScale=0, and the visible labels were TIME EXPIRED, YOUR TIME IS UP, and the restart explanation. The runtime display was verified at XR Origin (XR Rig)/Camera Offset/Right Controller/Maze Timer Display with TIME 00:00. Pressing Restart hid the menu, restored timeScale=1, reloaded Maze A, and restarted the countdown.
- **Known limitations:** Physical A-button input and headset readability/comfort still require a hardware pass; the binding and runtime hierarchy were verified in Editor Play Mode. Display offset and scale are exposed for that calibration.
- **Follow-up:** Test on the target headset and tune each maze duration plus displayOffsetFromView/displayScale in the GameManager Inspector if desired.


### MAZE-TIMER-002 — Move display controls to Right Controller

- **Date:** 2026-07-08
- **Goal:** Keep the timer beside the right controller and expose its visible duration and fade behavior in the Right Controller Inspector.
- **Result:** MazeLevelTimer was moved from GameManager to the scene's Right Controller. The controller Inspector now exposes Visible Duration Seconds, Fade In Seconds, Fade Out Seconds, Display Offset From View, and Display Scale. The default offset remains 0.13 metres to the viewer-relative left of the right controller, with a slight upward/forward offset. Existing Maze A–D countdown, pause, start flash, timeout, and restart behavior remains intact.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Managers/MazeLevelTimer.cs`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Maze Timer Display` (runtime-generated and inspected in Play Mode)
- **Components/assets/settings:** Removed MazeLevelTimer from GameManager and added it to Right Controller while preserving the four 180-second maze durations. Renamed the serialized buttonRevealSeconds setting to visibleDurationSeconds with FormerlySerializedAs migration. Split the previous single fade time into fadeInSeconds (0.18 seconds) and fadeOutSeconds (0.22 seconds). Right Controller also exposes displayOffsetFromView (-0.13, 0.035, 0.015), displayScale (0.035), colors, low-time threshold, and start-flash settings.
- **Decisions and assumptions:** Controller-display presentation settings belong on the Right Controller because it owns the runtime display and input reveal action. GameManager remains responsible for level loading; MazeLevelTimer resolves and subscribes to it at runtime.
- **Verification:** Unity compilation completed with scriptCompilationFailed=False. MainScene saved clean with MazeLevelTimer present on Right Controller and absent from GameManager. Inspector serialization confirmed Visible Duration=3, Fade In=0.18, Fade Out=0.22, Display Scale=0.035, and Start Flash Count=4. In Play Mode the timer remained running after relocation, Maze Timer Display was parented directly under Right Controller, and a long diagnostic fade produced an intermediate alpha of 0.3751592 with target alpha 1 before the fade-out reached target alpha 0. Runtime diagnostic values were not saved.
- **Known limitations:** The default offset is logically viewer-left and verified numerically, but final comfort/readability should be judged on the target headset.
- **Follow-up:** Adjust Visible Duration Seconds, Fade In Seconds, Fade Out Seconds, Display Offset From View, and Display Scale on Right Controller as desired.


### MAZE-TEXT-001 — Match Door Instruction Text to Maze D

- **Date:** 2026-07-08
- **Goal:** Apply the user-adjusted Maze D Door Instruction Text placement to the other maze prefabs.
- **Result:** Copied Maze D's Door Instruction Text local position, rotation, and scale to the corresponding objects in Maze A, Maze B, and Maze C. Maze E was unchanged because it has no Door Instruction Text object.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door Instruction Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door Instruction Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door Instruction Text`
- **Components/assets/settings:** Each affected Transform now has local position (-12.43, 2.78, -11.18), local Euler rotation (0, 270, 0), and local scale (1, 1, 1), copied from `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door Instruction Text`.
- **Decisions and assumptions:** Copied the complete local transform so the placement and facing remain consistent. Maze E was skipped rather than creating a new object because no matching Door Instruction Text exists there.
- **Verification:** Direct AssetDatabase inspection confirmed Maze A, B, C, and D all report identical position, rotation, and scale; allMatch=True. Unity reported scriptCompilationFailed=False and the active scene remained clean.
- **Known limitations:** None.
- **Follow-up:** None.


### LIGHT-NIGHT-001 — MainScene realtime night lighting

- **Date:** 2026-07-08
- **Goal:** Give MainScene a night-like realtime lighting setup without changing individual maze prefab lights.
- **Result:** Replaced the bright white ambient/daylight setup with a dark procedural night skybox, cool realtime moonlight, blue-black trilight ambient colors, and subtle long-range linear fog. Main Camera retains Skybox clearing with a dark fallback background.
- **Files created:**
  - `Assets/_EchoRoom/Materials/NightSkybox.mat`
- **Files modified:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Directional Light`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Main Camera`
- **Components/assets/settings:** RenderSettings now use Trilight ambient mode with sky (0.035, 0.055, 0.11), equator (0.018, 0.03, 0.065), ground (0.006, 0.01, 0.024), ambient intensity 0.72, reflection intensity 0.18, and NightSkybox.mat. Linear fog is enabled with color (0.014, 0.022, 0.048), start 18 m, end 68 m. Directional Light is Realtime, color (0.43, 0.56, 1), intensity 0.42, no shadows, bounce intensity 0.2, rotation (42, 325, 0), and remains assigned as RenderSettings.sun. Main Camera background is (0.005, 0.009, 0.022).
- **Decisions and assumptions:** Kept the moonlight shadowless to reduce VR cost and avoid the maze roofs making interiors completely black. No post-processing volume or maze-local light was added. Preserved and saved the MainScene edits that were already dirty before this lighting change rather than discarding them.
- **Verification:** MainScene saved successfully and reported sceneDirty=False. The night skybox asset is assigned, fog and ambient settings read back correctly, and the directional light reports Realtime with the intended color/intensity. An in-Unity 512x288 Main Camera render measured average luminance 0.0151, maximum 0.2353, 44.97% near-black pixels, 95.52% below luminance 0.05, and 0% above 0.5, confirming a deliberately dark frame without bright clipping.
- **Known limitations:** The screenshot-camera MCP capability was registered but unavailable in this session, so verification used numerical analysis of an actual Main Camera render rather than direct image inspection. Final brightness and comfort should be checked in the target headset.
- **Follow-up:** If headset testing feels too dark, raise Directional Light intensity first; if distant silhouettes disappear, brighten fogColor slightly.


### LIGHT-SONAR-001 — Make sonar geometry respond to realtime lighting

- **Date:** 2026-07-08
- **Goal:** Fix MainScene lighting having no visible effect on maze geometry.
- **Result:** Updated the shared EchoRoom/EchoSonarReveal shader from an effectively unlit output to include URP main directional light, realtime additional point/spot lights, spherical-harmonic ambient lighting, shadows when enabled, and scene fog. Sonar pulse/ring behavior remains intact. A new Environment Light Influence material slider defaults to 0.65 and can reduce or strengthen ordinary scene lighting independently of the sonar reveal.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Assets/_EchoRoom/Scenes/MainScene.unity` (saved after restoring the moon intensity used by the verification comparison)
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** The following Renderer objects use the shared shader and inherit the new lighting response without prefab serialization changes:
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door_Frame`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Exit_Hallway`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Frame`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Exit_Hallway`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Frame`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Exit_Hallway`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Frame`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Exit_Hallway`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Floor (1)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Exit_Hallway`
- **Components/assets/settings:** Added URP Lighting.hlsl integration, main-light and additional-light shader variants, per-pixel additional-light accumulation, ambient SampleSH contribution, optional shadow attenuation, multi_compile_fog, and MixFog. Environment Light Influence is Range(0,2), default 0.65.
- **Decisions and assumptions:** Scene lighting contributes to the dark between-ping base color; the sonar reveal remains the brighter dominant effect. The slider permits restoring a nearly pure sonar-only look by setting it toward zero. No material or prefab asset needed rewriting because all maze geometry shares this shader.
- **Verification:** Shader.isSupported=True with ShaderUtil messageCount=0 and scriptCompilationFailed=False. In Play Mode with Maze A loaded, comparing identical Main Camera renders at moon intensity 0.42 versus 0 changed 5.89% of pixels, mean pixel difference 0.018446, and reduced average luminance from 0.394484 to 0.378842. A temporary realtime point light also produced a nonzero response (0.52% changed pixels); it was destroyed immediately. Moon intensity was restored to 0.42, Play Mode exited, MainScene saved, and sceneDirty=False.
- **Known limitations:** Increasing Environment Light Influence makes geometry more visible between sonar pulses and can soften the original near-black gameplay. Additional realtime lights still carry normal VR performance costs.
- **Follow-up:** Tune Environment Light Influence on maze materials after headset testing; lower it if the maze becomes too readable without sonar.


### LIGHT-SONAR-002 — Separate ambient and realtime sonar lighting

- **Date:** 2026-07-08
- **Goal:** Correct the placed Maze A instance still appearing unaffected by scene-light changes.
- **Result:** Separated ambient-probe contribution from realtime direct-light contribution in EchoSonarReveal. Ambient lighting now has its own low default influence (0.08), while the existing Environment Light Influence property was relabeled Realtime Light Influence and continues to control the main directional and additional point/spot lights. This prevents a bright ambient probe from masking changes to realtime lights.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** No scene or prefab object was serialized by this change. Every Renderer using EchoRoom/EchoSonarReveal, as enumerated in `LIGHT-SONAR-001`, inherits the revised ambient/direct balance. The unsaved placed instance `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A (1)` was inspected but not saved or modified by this change.
- **Components/assets/settings:** Added shader property `_EnvironmentAmbientInfluence` (Ambient Light Influence, Range 0–1, default 0.08). `_EnvironmentLightInfluence` remains serialized on existing materials at 0.65 and is now labeled Realtime Light Influence. Ambient SampleSH and realtime main/additional light accumulation are applied separately.
- **Decisions and assumptions:** Kept direct-light influence on existing materials unchanged to avoid rewriting all maze material assets. The user's current Directional Light and placed Maze A scene edits were already dirty and were left unsaved.
- **Verification:** The active placed Maze A prefab instance was confirmed active with maze renderers on layer 0/rendering layer 1, matching the active Directional Light masks. Materials report EchoRoom/EchoSonarReveal, Realtime Light Influence 0.65, and Ambient Light Influence 0.08. Shader.isSupported=True with zero ShaderUtil messages. Controlled isolated composite renders of Maze A showed geometry dark under one directional orientation and visibly illuminated under the opposite orientation, confirming directional-light response. Scene View lighting was enabled. The current Directional Light is Realtime, enabled, intensity 5.24, soft shadows.
- **Known limitations:** Very high light intensity can saturate visible surfaces, making further intensity changes look similar. Light direction also determines which wall/floor normals receive illumination.
- **Follow-up:** For the intended night look, reduce the Directional Light from 5.24 to roughly 0.4–1.0 and rotate it until the desired walls/floor face the light; tune Realtime Light Influence per material if needed.


### LIGHT-SONAR-003 — Complete the actual lit shader body and shadow passes

- **Date:** 2026-07-08
- **Goal:** Correct the visibly flat/self-lit Maze A geometry shown in the user's Scene View screenshot.
- **Result:** Corrected an incomplete earlier shader patch. EchoSonarReveal now actually executes URP ambient, main directional, and additional point/spot lighting in its fragment shader; clamps the legacy white Base Color to a dark ambient floor; smoothly compresses HDR realtime contribution to retain shading at high intensity; applies scene fog; and exposes URP ShadowCaster and DepthOnly passes.
- **Correction to earlier entries:** `LIGHT-SONAR-001` and `LIGHT-SONAR-002` recorded intended lighting behavior after only property/pragmas/includes had been inserted. Windows CRLF line endings prevented the multi-line lighting-body replacements from matching, so the original `baseCol = albedo * _BaseColor` output remained self-lit. This entry records the completed implementation and supersedes those verification conclusions.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** No scene or prefab object was serialized. All Renderer objects using EchoRoom/EchoSonarReveal, listed under `LIGHT-SONAR-001`, inherit the corrected shader. The unsaved placed instance `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A (1)` was used for inspection and rendering only.
- **Components/assets/settings:** UnityPerMaterial now includes _EnvironmentLightInfluence and _EnvironmentAmbientInfluence. Varyings include fogFactor. The fragment stage uses GetMainLight with shadow coordinates, GetAdditionalLightsCount/GetAdditionalLight, SampleSH, separate ambient/direct controls, HDR compression `light/(1+light)`, and a capped legacy dark ambient of (0.008, 0.010, 0.016). Final color passes through MixFog. The SubShader reuses URP Lit ShadowCaster and DepthOnly passes.
- **Decisions and assumptions:** Preserved the sonar reveal/ring calculations and did not rewrite any material or prefab asset. Normal scene lighting affects the between-ping base, while the sonar pulse remains the stronger reveal. The user's dirty MainScene, placed Maze A, and current Directional Light settings were deliberately not saved.
- **Verification:** Shader.isSupported=True with zero ShaderUtil messages. Maze material pass count is 3: ForwardUnlit, ShadowCaster, DepthOnly. Source inspection confirmed the lighting CBUFFER fields, realtimeContribution calculations, MixFog, and ShadowCaster UsePass are physically present after reimport. Controlled isolated composite renders showed the maze dark without useful direct illumination, directional orientation changing which surfaces illuminate, and a high-intensity orange point light producing a localized red/orange response. No prefab asset changed.
- **Known limitations:** The current Directional Light is intensity 5.24 with Soft Shadows; high values are compressed but are still inappropriate for a restrained night look. Two-sided maze rendering means light direction and mesh-face normals strongly affect which surfaces illuminate.
- **Follow-up:** Reduce Directional Light to roughly 0.4–1.0, then rotate it while viewing Maze A. If stronger local contrast is needed, add carefully ranged realtime point/spot lights.


### SONAR-LIGHT-GATE-001 — Reveal scene lighting only through sonar

- **Date:** 2026-07-08
- **Goal:** Keep maze geometry completely dark before a ping, then reveal it with its realtime lighting, shadows, material texture, normal detail, and fog only inside the sonar pulse.
- **Result:** EchoSonarReveal now uses an absolute black pre-ping base. Ambient, directional, point/spot, shadow, material-normal, PBR, and fog contributions are evaluated for the revealed surface but multiplied by the sonar reveal mask. The emissive ring remains visible while the revealed area fades back to black using the existing pulse timing.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** No scene or prefab object was serialized. All Renderer objects using EchoRoom/EchoSonarReveal, listed under `LIGHT-SONAR-001`, inherit the ping-gated visibility. MainScene's existing dirty state was preserved and not saved.
- **Components/assets/settings:** `baseCol` is now (0,0,0). Flat reveal quality uses geometric-normal scene lighting. Normals/PBR qualities reevaluate ambient and realtime lights with the sampled normal map. PBR diffuse uses scene lighting and main-light shadowed specular. Fog is applied to `lit` before interpolation with the black base, preventing fog from making unrevealed geometry visible. Ambient Light Influence remains 0.08 so shadowed regions are faintly readable only while revealed.
- **Decisions and assumptions:** The ping is the sole visibility mask for sonar-shader geometry. Realtime lighting controls the appearance of revealed pixels but cannot make unrevealed pixels visible. No material, prefab, or scene asset needed modification.
- **Verification:** Shader.isSupported=True with zero ShaderUtil messages and three passes (ForwardUnlit, ShadowCaster, DepthOnly). A controlled isolated render under a bright directional light with an empty pulse buffer was completely black. In Play Mode with Maze A loaded, a same-call identical-camera comparison measured dark average luminance 0.044494 versus revealed average 0.070572, mean pixel difference 0.025672, and 10.03% changed pixels after injecting one current-time pulse at the camera; the pulse buffer was then cleared. Play Mode was stopped.
- **Known limitations:** Only renderers using EchoRoom/EchoSonarReveal are governed by this visibility gate. UI, skybox, and assets using ordinary Lit/Unlit shaders can remain visible independently.
- **Follow-up:** User will test the physical ping timing and revealed lighting balance in the headset.


### SONAR-SCENEVIEW-PREVIEW-001 — Material checkbox for edit-mode visibility

- **Date:** 2026-07-08
- **Goal:** Let artists reveal sonar materials while editing in Scene View without revealing them in Game View, Play Mode, or builds.
- **Result:** Added a Reveal In Scene View checkbox to EchoRoom/EchoSonarReveal materials. When enabled, the material renders its scene-lit revealed appearance only for Scene View cameras while Unity is outside Play Mode. Normal game cameras remain completely black until a sonar ping.
- **Files created:**
  - `Assets/_EchoRoom/Editor/EchoSonarSceneViewPreview.cs`
- **Files modified:**
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** No scene or prefab object was serialized. All Renderer objects using EchoRoom/EchoSonarReveal expose the new material property. MainScene's existing dirty state was preserved and not saved.
- **Components/assets/settings:** Added shader property `[Toggle] _SceneViewPreview (\"Reveal In Scene View\", Float) = 0`, default off. Added global `_EchoSceneViewCamera` and combined it with the ping reveal mask. The Editor-only InitializeOnLoad hook subscribes to URP begin/end camera rendering, sets the global to 1 only for CameraType.SceneView while EditorApplication.isPlaying is false, and resets it for all other cameras and play-mode transitions.
- **Decisions and assumptions:** Preview is per material, allowing selected materials to remain hidden while others are visible. It is camera- and editor-gated rather than a plain shader override, so accidentally leaving the checkbox enabled cannot reveal geometry in Game View, Play Mode, or player builds.
- **Verification:** Unity reported scriptCompilationFailed=False; shader.isSupported=True with zero ShaderUtil messages. The editor hook loaded from Assembly-CSharp-Editor. Existing maze materials report the checkbox property with default value 0. A framed controlled test over 14 Maze A materials produced Scene View mean pixel difference 0.017337 between checkbox off/on, while the equivalent Game camera difference was exactly 0.000000. Material values, pulse globals, object activation, and temporary cameras were restored after testing. MainScene remained dirty and unsaved as it was before the change.
- **Known limitations:** Because the setting is per material, shared materials reveal everywhere they are used in Scene View. Assets using another shader do not receive this checkbox.
- **Follow-up:** Select a sonar material and enable Reveal In Scene View while arranging a level; disable it when the preview is no longer needed.


### PREFAB-LIGHT-PREVIEW-001 — Enable realtime lighting preview across level prefabs

- **Date:** 2026-07-08
- **Goal:** Make realtime lights visible while editing all level prefabs.
- **Result:** Enabled Reveal In Scene View on every EchoSonarReveal material referenced by Maze A–E and updated the Editor-only preview hook to automatically enable Scene View lighting whenever a prefab stage opens or renders. This affects edit-time Prefab Mode only; Game View, Play Mode, and builds remain ping-gated.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Editor/EchoSonarSceneViewPreview.cs`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Stone.mat`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Red.mat`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Wood.mat`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Iron.mat`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Brass.mat`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Grip.mat`
  - `Assets/_EchoRoom/Materials/MazeB/Door_FrameIron.mat`
  - `Assets/_EchoRoom/Materials/MazeB/Door_Wood.mat`
  - `Assets/_EchoRoom/Materials/MazeB/Maze_FloorStone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeB/Maze_Stone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeB/Maze_RoofStone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeC/Door_FrameIron.mat`
  - `Assets/_EchoRoom/Materials/MazeC/Door_Wood.mat`
  - `Assets/_EchoRoom/Materials/MazeC/Maze_FloorStone.mat`
  - `Assets/_EchoRoom/Materials/MazeC/Maze_RoofStone.mat`
  - `Assets/_EchoRoom/Materials/MazeC/Maze_Stone.mat`
  - `Assets/_EchoRoom/Art/Materials/Echo Interactable/Default.mat`
  - `Assets/_EchoRoom/Art/Materials/Echo Interactable/Button.mat`
  - `Assets/_EchoRoom/Materials/MazeC/Maze_FloorStone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeC/Maze_Stone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeC/Maze_RoofStone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeD/Door_FrameIron.mat`
  - `Assets/_EchoRoom/Materials/MazeD/Door_Wood.mat`
  - `Assets/_EchoRoom/Materials/MazeD/Maze_FloorStone.mat`
  - `Assets/_EchoRoom/Materials/MazeD/Maze_RoofStone.mat`
  - `Assets/_EchoRoom/Materials/MazeD/Maze_Stone.mat`
  - `Assets/_EchoRoom/Materials/MazeD/Maze_FloorStone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeD/Maze_Stone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeD/Maze_RoofStone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeE/Maze_FloorStone.mat`
  - `Assets/_EchoRoom/Materials/MazeE/Maze_RoofStone.mat`
  - `Assets/_EchoRoom/Materials/MazeE/Maze_Stone.mat`
  - `Assets/_EchoRoom/Materials/MazeE/Maze_FloorStone 1.mat`
  - `Assets/_EchoRoom/Materials/MazeE/Maze_Stone 1.mat`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** None serialized. Maze prefab assets were inspected but not modified. `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Point Light` and `Maze_5x5_A/Point Light (1)` were temporarily enabled/disabled during render verification and restored.
- **Components/assets/settings:** All 45 unique EchoSonarReveal materials referenced by Maze A–E now have _SceneViewPreview=1; 11 were already enabled and 34 were changed. EchoSonarSceneViewPreview now subscribes to PrefabStage.prefabStageOpened and ensures every SceneView has sceneLighting=true while a prefab stage is active.
- **Decisions and assumptions:** Did not add invented lights to Maze B–E. Maze A currently contains two enabled Realtime point lights (intensities 4 and 1, ranges 10). Maze B, C, D, and E currently contain zero Light components; their materials preview with ambient lighting and will respond to realtime lights when the user adds them.
- **Verification:** Unity reported scriptCompilationFailed=False. All 45/45 level sonar materials report preview on and 0 off. The editor hook loaded successfully and Scene View lighting reports true. In the open Maze A prefab stage, a direct Scene View render measured average luminance 0.013321 with both point lights enabled versus 0.007640 disabled, mean difference 0.005573, confirming prefab-local realtime light response. Light enabled states were restored. Maze A prefab stage remained clean; MainScene's pre-existing dirty state was preserved and not saved.
- **Known limitations:** Levels without Light components cannot show local-light contribution until lights are added. Preview is shared-material based, so each enabled material is visible in every Prefab/Scene View where it is used.
- **Follow-up:** Add realtime point/spot lights to Maze B–E as desired; Prefab Mode will preview them automatically.


## 2026-07-08 — LEVEL-LIGHTS-001 — Four realtime point lights per maze

- Goal: Give every maze prefab a visible local-light setup that works in Prefab Mode and at runtime without requiring a directional light.
- Resulting behavior: Maze A, B, C, D, and E each contain exactly four evenly distributed cool-blue realtime Point Lights. Maze A's two existing test Point Lights were reused, renamed, repositioned, and normalized; two additional lights were added. Maze B-E received four new lights each.
- Modified files:
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab
  - Docs/PROJECT_MEMORY.md
- Created/moved/deleted files: None.
- Affected Unity objects:
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 4
- Light settings: Type Point; Mode Realtime; color #759EFF; intensity 2.1; range 10; bounce intensity 0.2; shadows None; culling mask Everything; rendering layer mask 1.
- Layout:
  - Maze A-D local positions: (-9.13, 2.09, -9.13), (-3.37, 2.09, -9.13), (-9.13, 2.09, -3.37), (-3.37, 2.09, -3.37).
  - Maze E local positions: (-18.38, 2.09, -9.13), (-6.97, 2.09, -9.13), (-18.38, 2.09, -3.37), (-6.97, 2.09, -3.37).
- Decisions/assumptions: Used a 2x2 distribution calculated from each maze's renderer bounds. Realtime shadows are disabled to control VR rendering cost. These are realtime lights and are not part of a baked lightmap.
- Known limitation/follow-up: Four overlapping realtime lights may need intensity/range tuning after headset testing. If the final production lighting becomes baked, these lights can be switched to Baked/Mixed after each maze has a valid lightmap workflow.
- Verification: Inspected all five saved prefab assets through Unity. Each has exactly four total lights, all four are under Level Realtime Lights, enabled, active, Point, Realtime, intensity 2.1, range 10, and shadows None. Maze A was reopened in Prefab Mode and is clean. MainScene.unity remained dirty and was not saved.


## 2026-07-08 — LEVEL-AO-001 — White maze lights and shared screen-space ambient occlusion

- Goal: Change all maze Point Lights to white and add ambient occlusion consistently to every level.
- Resulting behavior: All 20 managed maze Point Lights are neutral white. The active URP renderer now has an enabled Screen Space Ambient Occlusion feature, and EchoSonarReveal.shader consumes its indirect and direct AO factors while geometry is visible through the sonar reveal.
- Modified files:
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab
  - Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset
  - Assets/_EchoRoom/Shaders/EchoSonarReveal.shader
  - Docs/PROJECT_MEMORY.md
- Created/moved/deleted files: None.
- Affected Unity objects:
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 1
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 2
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 3
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 4
  - Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset :: Maze Screen Space Ambient Occlusion
- Important settings and dependencies:
  - Light color: white (#FFFFFF). Existing Realtime mode, intensity 2.1, range 10, and shadows None were preserved.
  - URP AO: Blue Noise; Downsample enabled; Before Opaques; Depth source with Medium reconstructed normals; Intensity 2; Direct Lighting Strength 0.25; Radius 0.035; Medium samples; High blur.
  - EchoSonarReveal.shader now compiles the _SCREEN_SPACE_OCCLUSION variant and applies screen-space AO to ambient and realtime lighting. AO does not bypass the sonar visibility mask.
  - The AO feature is shared through Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset, used by Assets/_EchoRoom/RenderPipeline/Default URP.asset, so it applies to all maze prefabs without per-level Volume objects.
- Decisions/assumptions: Used renderer-level SSAO because all levels share the same URP renderer. Depth reconstruction was selected because the custom sonar shader already supplies a DepthOnly pass and does not need an added DepthNormals pass.
- Known limitation/follow-up: SSAO is screen-space and visible only for geometry currently rendered/revealed by the sonar system. Downsampling reduces VR cost, but headset performance and AO intensity should be checked in Play Mode.
- Verification: Unity inspection confirmed exactly four white lights in each Maze A-E prefab. The active renderer contains two features total (Decal plus enabled Maze Screen Space Ambient Occlusion). AO settings persisted after asset reload. EchoSonarReveal.shader reports zero compiler messages/errors. Maze A Prefab Mode is clean. MainScene.unity remained dirty and was not saved.

## 2026-07-09 — LEVEL-INDIRECT-001 — Brighter indirect lighting for all maze levels

- Goal: Make all levels look a little brighter through indirect/ambient lighting without increasing any Light component intensity.
- Resulting behavior: Every EchoSonarReveal material used by Maze A-E has +0.08 Environment Ambient Influence. Materials at 0.08 now use 0.16; existing custom values were preserved proportionally by receiving the same additive lift. Direct Point Light settings are unchanged.
- Modified files:
  - Assets/_EchoRoom/Art/Materials/Echo Interactable/Button.mat
  - Assets/_EchoRoom/Art/Materials/Echo Interactable/Default.mat
  - Assets/_EchoRoom/Materials/Lever/Lever_Brass.mat
  - Assets/_EchoRoom/Materials/Lever/Lever_Grip.mat
  - Assets/_EchoRoom/Materials/Lever/Lever_Iron.mat
  - Assets/_EchoRoom/Materials/Lever/Lever_Red.mat
  - Assets/_EchoRoom/Materials/Lever/Lever_Stone.mat
  - Assets/_EchoRoom/Materials/Lever/Lever_Wood.mat
  - Assets/_EchoRoom/Materials/Maze/Door_FrameIron.mat
  - Assets/_EchoRoom/Materials/Maze/Door_Wood.mat
  - Assets/_EchoRoom/Materials/Maze/Maze_FloorStone 1.mat
  - Assets/_EchoRoom/Materials/Maze/Maze_FloorStone.mat
  - Assets/_EchoRoom/Materials/Maze/Maze_RoofStone 1.mat
  - Assets/_EchoRoom/Materials/Maze/Maze_RoofStone.mat
  - Assets/_EchoRoom/Materials/Maze/Maze_Stone 1.mat
  - Assets/_EchoRoom/Materials/Maze/Maze_Stone.mat
  - Assets/_EchoRoom/Materials/MazeB/Door_FrameIron.mat
  - Assets/_EchoRoom/Materials/MazeB/Door_Wood.mat
  - Assets/_EchoRoom/Materials/MazeB/Maze_FloorStone 1.mat
  - Assets/_EchoRoom/Materials/MazeB/Maze_FloorStone.mat
  - Assets/_EchoRoom/Materials/MazeB/Maze_RoofStone 1.mat
  - Assets/_EchoRoom/Materials/MazeB/Maze_RoofStone.mat
  - Assets/_EchoRoom/Materials/MazeB/Maze_Stone 1.mat
  - Assets/_EchoRoom/Materials/MazeB/Maze_Stone.mat
  - Assets/_EchoRoom/Materials/MazeC/Door_FrameIron.mat
  - Assets/_EchoRoom/Materials/MazeC/Door_Wood.mat
  - Assets/_EchoRoom/Materials/MazeC/Maze_FloorStone 1.mat
  - Assets/_EchoRoom/Materials/MazeC/Maze_FloorStone.mat
  - Assets/_EchoRoom/Materials/MazeC/Maze_RoofStone 1.mat
  - Assets/_EchoRoom/Materials/MazeC/Maze_RoofStone.mat
  - Assets/_EchoRoom/Materials/MazeC/Maze_Stone 1.mat
  - Assets/_EchoRoom/Materials/MazeC/Maze_Stone.mat
  - Assets/_EchoRoom/Materials/MazeD/Door_FrameIron.mat
  - Assets/_EchoRoom/Materials/MazeD/Door_Wood.mat
  - Assets/_EchoRoom/Materials/MazeD/Maze_FloorStone 1.mat
  - Assets/_EchoRoom/Materials/MazeD/Maze_FloorStone.mat
  - Assets/_EchoRoom/Materials/MazeD/Maze_RoofStone 1.mat
  - Assets/_EchoRoom/Materials/MazeD/Maze_RoofStone.mat
  - Assets/_EchoRoom/Materials/MazeD/Maze_Stone 1.mat
  - Assets/_EchoRoom/Materials/MazeD/Maze_Stone.mat
  - Assets/_EchoRoom/Materials/MazeE/Maze_FloorStone 1.mat
  - Assets/_EchoRoom/Materials/MazeE/Maze_FloorStone.mat
  - Assets/_EchoRoom/Materials/MazeE/Maze_RoofStone.mat
  - Assets/_EchoRoom/Materials/MazeE/Maze_Stone 1.mat
  - Assets/_EchoRoom/Materials/MazeE/Maze_Stone.mat
  - Docs/PROJECT_MEMORY.md
- Created/moved/deleted files: None.
- Modified Unity asset objects:
  - Assets/_EchoRoom/Art/Materials/Echo Interactable/Button.mat :: Button
  - Assets/_EchoRoom/Art/Materials/Echo Interactable/Default.mat :: Default
  - Assets/_EchoRoom/Materials/Lever/Lever_Brass.mat :: Lever_Brass
  - Assets/_EchoRoom/Materials/Lever/Lever_Grip.mat :: Lever_Grip
  - Assets/_EchoRoom/Materials/Lever/Lever_Iron.mat :: Lever_Iron
  - Assets/_EchoRoom/Materials/Lever/Lever_Red.mat :: Lever_Red
  - Assets/_EchoRoom/Materials/Lever/Lever_Stone.mat :: Lever_Stone
  - Assets/_EchoRoom/Materials/Lever/Lever_Wood.mat :: Lever_Wood
  - Assets/_EchoRoom/Materials/Maze/Door_FrameIron.mat :: Door_FrameIron
  - Assets/_EchoRoom/Materials/Maze/Door_Wood.mat :: Door_Wood
  - Assets/_EchoRoom/Materials/Maze/Maze_FloorStone 1.mat :: Maze_FloorStone 1
  - Assets/_EchoRoom/Materials/Maze/Maze_FloorStone.mat :: Maze_FloorStone
  - Assets/_EchoRoom/Materials/Maze/Maze_RoofStone 1.mat :: Maze_RoofStone 1
  - Assets/_EchoRoom/Materials/Maze/Maze_RoofStone.mat :: Maze_RoofStone
  - Assets/_EchoRoom/Materials/Maze/Maze_Stone 1.mat :: Maze_Stone 1
  - Assets/_EchoRoom/Materials/Maze/Maze_Stone.mat :: Maze_Stone
  - Assets/_EchoRoom/Materials/MazeB/Door_FrameIron.mat :: Door_FrameIron
  - Assets/_EchoRoom/Materials/MazeB/Door_Wood.mat :: Door_Wood
  - Assets/_EchoRoom/Materials/MazeB/Maze_FloorStone 1.mat :: Maze_FloorStone 1
  - Assets/_EchoRoom/Materials/MazeB/Maze_FloorStone.mat :: Maze_FloorStone
  - Assets/_EchoRoom/Materials/MazeB/Maze_RoofStone 1.mat :: Maze_RoofStone 1
  - Assets/_EchoRoom/Materials/MazeB/Maze_RoofStone.mat :: Maze_RoofStone
  - Assets/_EchoRoom/Materials/MazeB/Maze_Stone 1.mat :: Maze_Stone 1
  - Assets/_EchoRoom/Materials/MazeB/Maze_Stone.mat :: Maze_Stone
  - Assets/_EchoRoom/Materials/MazeC/Door_FrameIron.mat :: Door_FrameIron
  - Assets/_EchoRoom/Materials/MazeC/Door_Wood.mat :: Door_Wood
  - Assets/_EchoRoom/Materials/MazeC/Maze_FloorStone 1.mat :: Maze_FloorStone 1
  - Assets/_EchoRoom/Materials/MazeC/Maze_FloorStone.mat :: Maze_FloorStone
  - Assets/_EchoRoom/Materials/MazeC/Maze_RoofStone 1.mat :: Maze_RoofStone 1
  - Assets/_EchoRoom/Materials/MazeC/Maze_RoofStone.mat :: Maze_RoofStone
  - Assets/_EchoRoom/Materials/MazeC/Maze_Stone 1.mat :: Maze_Stone 1
  - Assets/_EchoRoom/Materials/MazeC/Maze_Stone.mat :: Maze_Stone
  - Assets/_EchoRoom/Materials/MazeD/Door_FrameIron.mat :: Door_FrameIron
  - Assets/_EchoRoom/Materials/MazeD/Door_Wood.mat :: Door_Wood
  - Assets/_EchoRoom/Materials/MazeD/Maze_FloorStone 1.mat :: Maze_FloorStone 1
  - Assets/_EchoRoom/Materials/MazeD/Maze_FloorStone.mat :: Maze_FloorStone
  - Assets/_EchoRoom/Materials/MazeD/Maze_RoofStone 1.mat :: Maze_RoofStone 1
  - Assets/_EchoRoom/Materials/MazeD/Maze_RoofStone.mat :: Maze_RoofStone
  - Assets/_EchoRoom/Materials/MazeD/Maze_Stone 1.mat :: Maze_Stone 1
  - Assets/_EchoRoom/Materials/MazeD/Maze_Stone.mat :: Maze_Stone
  - Assets/_EchoRoom/Materials/MazeE/Maze_FloorStone 1.mat :: Maze_FloorStone 1
  - Assets/_EchoRoom/Materials/MazeE/Maze_FloorStone.mat :: Maze_FloorStone
  - Assets/_EchoRoom/Materials/MazeE/Maze_RoofStone.mat :: Maze_RoofStone
  - Assets/_EchoRoom/Materials/MazeE/Maze_Stone 1.mat :: Maze_Stone 1
  - Assets/_EchoRoom/Materials/MazeE/Maze_Stone.mat :: Maze_Stone
- Affected prefab renderer consumers (prefab serialization unchanged):
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door_Frame
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door_Leaf
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Exit_Hallway/Hallway_Ceiling
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Exit_Hallway/Hallway_EndWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Exit_Hallway/Hallway_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Exit_Hallway/Hallway_NorthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Exit_Hallway/Hallway_SouthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P1_D1/Lever_Body
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P1_D1/Lever_Pivot/Lever_Handle
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P2_A4/Lever_Body
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P2_A4/Lever_Pivot/Lever_Handle
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P3_E5/Lever_Body
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P3_E5/Lever_Pivot/Lever_Handle
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Maze_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Maze_Roof
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Maze_Walls
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P1_Lever/Lever_Body
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P1_Lever/Lever_Pivot/Lever_Handle
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P2_Lever/Lever_Body
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P2_Lever/Lever_Pivot/Lever_Handle
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P3_Lever/Lever_Body
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P3_Lever/Lever_Pivot/Lever_Handle
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Frame
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Leaf
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Exit_Hallway/Hallway_Ceiling
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Exit_Hallway/Hallway_EndWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Exit_Hallway/Hallway_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Exit_Hallway/Hallway_NorthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Exit_Hallway/Hallway_SouthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Roof
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Walls
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P1_Button
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P1_Button/Button_Object
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P2_Button
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P2_Button/Button_Object
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P3_Button
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P3_Button/Button_Object
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Frame
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Leaf
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Exit_Hallway/Hallway_Ceiling
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Exit_Hallway/Hallway_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Exit_Hallway/Hallway_NorthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Exit_Hallway/Hallway_SouthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Exit_Hallway/Hallway_SouthWall (1)
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Roof
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Walls
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P1_Button
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P1_Button/Button_Object
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P2_Button
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P2_Button/Button_Object
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P3_Button
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P3_Button/Button_Object
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Frame
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Leaf
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Exit_Hallway/Hallway_Ceiling
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Exit_Hallway/Hallway_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Exit_Hallway/Hallway_NorthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Exit_Hallway/Hallway_NorthWall (1)
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Exit_Hallway/Hallway_SouthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Roof
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Walls
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Exit_Hallway/Exit_Door
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Exit_Hallway/Hallway_Ceiling
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Exit_Hallway/Hallway_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Exit_Hallway/Hallway_NorthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Exit_Hallway/Hallway_SouthWall
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Floor
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Floor (1)
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Roof
  - Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Walls
- Important settings and dependencies:
  - EchoSonarReveal property changed: _EnvironmentAmbientInfluence += 0.08, clamped to 0-1.
  - Final value groups across 45 materials: 0.16 on 40 materials; 0.252 on 1; 0.31 on 2; 0.58 on 2.
  - This changes SampleSH ambient/indirect contribution only. _EnvironmentLightInfluence and all Unity Light intensities remain unchanged.
  - The existing sonar visibility mask remains authoritative, so levels are still completely dark before a ping.
- Decisions/assumptions: Applied an additive lift instead of forcing one common value so the user's existing Maze A/B material tuning remains distinct.
- Known limitation/follow-up: The visible result depends on the ambient probe/sky lighting available to the active scene or Prefab Mode. If headset testing is still too dark, increase this property in another small step rather than increasing realtime light intensity.
- Verification: Unity inspection found 45 affected materials referenced by Maze A-E. All saved values match the expected +0.08 lift. A before/after Light intensity snapshot confirmed every maze Light intensity is unchanged. MainScene.unity remained dirty and was not saved.


## 2026-07-09 — AO-VISIBILITY-BOOST-MAZE-A-E

- Goal and resulting behavior: Increased the screen-space ambient occlusion strength used by Maze A-E because the existing effect was present but too subtle to see in the maze corridors. The active URP renderer feature now produces stronger contact darkening at wall/floor/ceiling intersections and around nearby geometry.
- Created/modified/moved/deleted files:
  - Modified: Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset
  - Modified: Docs/PROJECT_MEMORY.md
- Affected Unity objects:
  - Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset :: Maze Screen Space Ambient Occlusion
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_B
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_C
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_D
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_E
- Important settings and dependencies:
  - Render pipeline asset: Assets/_EchoRoom/RenderPipeline/Default URP.asset (`Default URP`).
  - Renderer asset: Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset (`Default URP_Renderer`).
  - Renderer feature: `Maze Screen Space Ambient Occlusion`, type `UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion`, active=True.
  - AO settings changed from Intensity=2, Radius=0.035, DirectLightingStrength=0.25 to Intensity=3.25, Radius=0.18, DirectLightingStrength=0.35.
  - Kept Downsample=True, Source=Depth, Samples=Medium, AfterOpaque=False to avoid a larger VR performance jump.
  - Main Camera still has URP post-processing enabled, so the renderer feature is available to gameplay rendering.
- Decisions/assumptions: Raised radius first because the previous 0.035 radius was too small for maze-scale contact shadows. Chose a stronger but still conservative intensity/radius pair for VR instead of maxing the effect.
- Known limitations/follow-up: This is a global URP renderer feature, so it affects all cameras/scenes using `Default URP_Renderer`, not only Maze A-E. If the headset view is still too subtle, increase `Radius` slightly before increasing `Intensity`; if it looks dirty or noisy, reduce `Intensity` first.
- Verification: Direct Unity MCP inspection confirmed MainScene contains Maze_5x5_A through Maze_5x5_E. Renderer feature values were saved and read back as Intensity=3.25, Radius=0.18, DirectLightingStrength=0.35, Downsample=True, Source=Depth, Samples=Medium. MainScene.unity was already dirty before this task and was not saved by this change.


## 2026-07-09 — AO-RENDER-PATH-DIAGNOSIS-FIX

- Goal and resulting behavior: Investigated why the boosted Maze A-E ambient occlusion still was not visible, then changed the URP render path so SSAO has a reliable depth source and is composited after opaque rendering. Maze A-E now use the active renderer's full-screen SSAO application instead of relying only on shader-side AO sampling during the custom sonar forward pass.
- Created/modified/moved/deleted files:
  - Modified: Assets/_EchoRoom/RenderPipeline/Default URP.asset
  - Modified: Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset
  - Modified: Docs/PROJECT_MEMORY.md
  - Created then deleted during diagnosis: Temp/Maze_AO_Diagnosis.txt
- Affected Unity objects:
  - Assets/_EchoRoom/RenderPipeline/Default URP.asset :: Default URP
  - Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset :: Maze Screen Space Ambient Occlusion
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_B
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_C
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_D
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_E
- Important settings and dependencies:
  - Diagnosis found 85 Maze A-E MeshRenderers, all opaque render queue 2000, all receive shadows, and no maze renderers with baked lightmaps (`lightmapIndex=-1`).
  - Maze A-E scene roots are linked to model assets: Assets/_EchoRoom/Models/Maze_5x5_A.fbx through Maze_5x5_E.fbx. Separate prefab assets exist under Assets/_EchoRoom/Prefabs/Level prefabs/, but the visible issue was not caused by missing per-prefab AO components.
  - All scanned Maze A-E materials use `EchoRoom/EchoSonarReveal` from Assets/_EchoRoom/Shaders/EchoSonarReveal.shader. The shader includes `_SCREEN_SPACE_OCCLUSION` and calls `GetScreenSpaceAmbientOcclusion`, so the materials/shader are AO-aware.
  - Root cause indicated by scan: the active Main Camera inherited `UsePipelineSettings` for depth, while `Default URP.asset` had `supportsCameraDepthTexture=False` / `m_RequireDepthTexture=False`. The AO feature was also `AfterOpaque=False`, meaning it depended on shader-side consumption rather than visibly compositing over opaque maze pixels.
  - Changed `Default URP.asset`: `m_RequireDepthTexture=True` / `supportsCameraDepthTexture=True`.
  - Changed `Maze Screen Space Ambient Occlusion`: `AfterOpaque=True`, `Source=Depth`, `Intensity=3.5`, `Radius=0.22`, `DirectLightingStrength=0.45`, kept `Downsample=True` and `Samples=Medium`.
- Decisions/assumptions: Did not add AO to individual prefabs because SSAO is renderer/camera driven in URP, not a per-prefab component. Chose the full-screen After Opaque path because it makes AO visible on the final opaque maze image and is less dependent on the custom sonar shader's lighting branch.
- Known limitations/follow-up: `AfterOpaque=True` is less physically correct but more visually direct. It affects all opaque rendering through `Default URP_Renderer`, not only Maze A-E. If AO becomes too heavy in headset, reduce `Intensity` first or disable `AfterOpaque` only after confirming the camera depth texture path remains valid.
- Verification: Direct Unity MCP scan and read-back confirmed `supportsCameraDepthTexture=True`, Main Camera still uses `requiresDepthOption=UsePipelineSettings`, and `Maze Screen Space Ambient Occlusion` reads back as active with `AfterOpaque=True`, `Source=Depth`, `Intensity=3.5`, `Radius=0.22`, `DirectLightingStrength=0.45`, `Downsample=True`, `Samples=Medium`. MainScene.unity was already dirty before this task and was not saved.


## 2026-07-09 — TUTORIAL-PING-REVEAL-FIX

- Goal and resulting behavior: Fixed tutorial level materials not revealing reliably when the player pings. Tutorial reveal-capable surfaces now receive a global reveal pulse directly from `PingEmitter`, and the tutorial T-junction door/knob materials now use the same `EchoRoom/EchoSonarReveal` shader path as the rest of the tutorial geometry.
- Created/modified/moved/deleted files:
  - Modified: Assets/_EchoRoom/Scripts/Controller/SonarRevealController.cs
  - Modified: Assets/_EchoRoom/Scripts/PingEmitter.cs
  - Modified: Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab
  - Created/modified: Assets/_EchoRoom/Materials/Tutorial/Tutorial_Door_3_Brown_Reveal.mat
  - Created/modified: Assets/_EchoRoom/Materials/Tutorial/Tutorial_Door_Silver_Reveal.mat
  - Modified: Docs/PROJECT_MEMORY.md
  - Created then deleted during diagnosis: Temp/TutorialRevealPrefabScriptScan.txt
  - Created then deleted during diagnosis: Temp/TutorialRevealScriptsFull.txt
- Affected Unity objects:
  - Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/PingEmiiter
  - Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/EchoPulseController
  - Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Door Frame/Door_3_Brown/Door
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Door Frame/Door_3_Brown/Door/Knob
  - Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Door Frame/Door_3_Brown/Frame
- Important settings and dependencies:
  - Diagnosis found the tutorial T-junction wall/floor/interactable materials already used `EchoRoom/EchoSonarReveal`, but the door and knob used third-party URP/Lit materials: `Door_3_Brown.mat` and `Silver.mat`, which cannot respond to the sonar reveal shader pulse.
  - Created reveal material copies under Assets/_EchoRoom/Materials/Tutorial/ using `EchoRoom/EchoSonarReveal`, copied common base/normal/metallic/occlusion inputs from the original URP materials where present, set `_SceneViewPreview=0`, `_RevealRadius=8`, `_RevealBrightness=1.2`, `_EnvironmentAmbientInfluence=0.12`, `_EnvironmentLightInfluence=0.85`, and render queue 2000.
  - `SonarRevealController` now owns a static shared `_SonarPulses` buffer and exposes `RevealGlobal(Vector3 origin)` with same-frame duplicate protection.
  - `PingEmitter.EmitPing` now calls `SonarRevealController.RevealGlobal(origin)` whenever a ping actually fires, after raising `OnPingEmitted`. This keeps existing event listeners working while making shader reveal independent of whether the scene `SonarRevealController` component is active/subscribed.
- Decisions/assumptions: Did not convert TextMeshPro tutorial labels or entity particle/smoke materials to sonar reveal materials because they are intentionally UI/transparent visual effects, not opaque tutorial room geometry. Kept tutorial material reveal radius at the established value of 8 to match the existing wall/floor/interactable materials.
- Known limitations/follow-up: The scan showed `XR Origin (XR Rig)` is currently inactive in the editor hierarchy, so editor-time activeInHierarchy checks for ping scripts are not representative of play mode. The runtime fix is placed in `PingEmitter`, so any successful ping now pushes the shader pulse even if a separate reveal controller listener is unavailable.
- Verification: Unity refresh completed with no C# compile errors; existing warnings remained unrelated. Direct read-back confirmed `SonarRevealController` contains `RevealGlobal`, `PingEmitter` calls `SonarRevealController.RevealGlobal(origin)`, and `T_Junction_Tutorial.prefab` has `Tutorial_Door_3_Brown_Reveal` on Door and Frame plus `Tutorial_Door_Silver_Reveal` on Knob. Verification found `nonRevealOpaqueNonTextOrParticleMaterials=0` for the tutorial T-junction prefab. MainScene.unity was already dirty before this task and was not saved.


### AUDIO-MUSIC-001 — Button SFX and fading scene music

- **Date:** 2026-07-09
- **Goal:** Apply the newly added button on/off clips to puzzle buttons, add ambient gameplay music, and use menu music for the main menu and paused game without abrupt sound changes.
- **Result:** Echo buttons now expose Button Audio fields and play `ButtonOn.mp3` on activation and `ButtonOFF.mp3` when reset. Gameplay and menu music are controlled by a persistent `EchoMusicManager` in both main scenes. It crossfades to ambient music during gameplay, crossfades to menu music in `MainMenuScene`, and crossfades to menu music while the VR pause/captured/start menu is open. Music sources ignore `AudioListener.pause` so menu music can still play while the pause menu mutes environment audio.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Managers/EchoMusicManager.cs`
  - `Assets/_EchoRoom/Scripts/Managers/EchoMusicManager.cs.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Interactables/EchoButtonInteractable.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Assets/_EchoRoom/Prefabs/Auto_P3_Button Variant.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab`
  - `Assets/_EchoRoom/Prefabs/PuzzleButton_Auto.prefab`
  - `Assets/_EchoRoom/Prefabs/Tutorial/Tutorial Button.prefab`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Echo Music Manager`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Echo Music Manager`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_C/Auto_P1_Button`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_C/Auto_P2_Button`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_C/Auto_P3_Button`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_D/Auto_P1_Button`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_D/Auto_P2_Button`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_D/Auto_P3_Button`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Button_Prefab`
  - `Assets/_EchoRoom/Prefabs/Auto_P3_Button Variant.prefab :: Auto_P3_Button Variant`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P1_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P2_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P3_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P1_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P2_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P3_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Tutorial Button`
  - `Assets/_EchoRoom/Prefabs/PuzzleButton_Auto.prefab :: PuzzleButton_Auto`
  - `Assets/_EchoRoom/Prefabs/Tutorial/Tutorial Button.prefab :: Tutorial Button`
- **Components/assets/settings:** Button audio uses `Assets/Audio/ButtonOn.mp3` and `Assets/Audio/ButtonOFF.mp3` through `EchoButtonInteractable` fields, with an `AudioSource` on each affected button. Music uses `Assets/Audio/Ambient Music.mp3` at volume 0.42 and `Assets/Audio/Menu Music.mp3` at volume 0.5. `EchoMusicManager` fade duration is 1.25 seconds and main menu scene name is `MainMenuScene`. `VRPauseMenu` now raises `OnMenuStateChanged` when menus open or close.
- **Decisions and assumptions:** Music clip references are serialized on scene objects instead of loaded by filename at runtime, so builds keep valid AudioClip references. A duplicate manager in the newly loaded scene passes missing clip references to the persistent manager and then destroys itself. Button off audio is tied to `ResetElement()` because these buttons are one-shot puzzle elements and do not otherwise have a player-facing off transition.
- **Verification:** Unity AssetDatabase refresh completed after script changes. Recent Unity console errors showed only the pre-existing MCP warning about spaces in the project path and no new script compile errors. Direct Unity inspection confirmed `Echo Music Manager` exists in both `MainScene` and `MainMenuScene` with the ambient/menu clips assigned. A verification pass checked 17 scene/prefab buttons and reported zero missing button audio assignments.
- **Known limitations:** Play Mode audio listening was not run in this pass, so final loudness and fade feel should be checked in headset or Editor Play Mode. `MainScene` was already dirty before this work; saving the intended audio setup also saved the scene's current dirty state.
- **Follow-up:** Tune `ambientVolume`, `menuVolume`, and `fadeSeconds` on either scene's `Echo Music Manager` if the mix needs adjustment after a listening pass.


### SONAR-LINGER-001 — Central reveal linger control

- **Date:** 2026-07-09
- **Goal:** Make the ping reveal linger a little longer and expose the timing from one central control point.
- **Result:** `SonarRevealController` now has an Inspector field named Reveal Linger Seconds. The active scene controller is set to 2.25 seconds, up from the shader material default of 1.5 seconds. The reveal shader now respects a global `_EchoRevealLingerOverride` value, so the controller setting overrides individual material linger values at runtime.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Controller/SonarRevealController.cs`
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/EchoPulseController`
- **Components/assets/settings:** The affected object has `SonarRevealController.revealLingerSeconds` set to 2.25. The shader declares `_EchoRevealLingerOverride` and uses it when greater than zero, otherwise it falls back to each material's `_RevealLinger` value.
- **Decisions and assumptions:** The central control lives on the existing scene `SonarRevealController` because all ping reveal pulses already flow through that controller/static shader buffer. A global shader override was used so users do not need to edit every sonar material individually.
- **Verification:** Unity AssetDatabase refresh completed successfully after the fix. Direct Unity verification reported one `SonarRevealController` at the recorded path, `Reveal Linger Seconds = 2.25`, shader override present, controller field present, and global shader value 2.25.
- **Known limitations:** Play Mode visual timing was not run in this pass; final feel should be checked in headset or Editor Play Mode and tuned from the controller field if desired.
- **Follow-up:** Tune `Reveal Linger Seconds` on the recorded controller object if the effect feels too short or too bright for the maze mood.


### LOAD-RETURN-SMOOTH-001 — Smooth return-to-menu loading cover

- **Date:** 2026-07-13
- **Goal:** Prevent controller/player visuals from popping into view before the loading screen appears when returning from a gameplay level to the main menu.
- **Result:** The pause menu no longer hides itself before delegating to `GameManager.ReturnToMainMenu()`. `GameManager` now starts a controlled return coroutine, restores time/audio, and yields through `VRLoadingScreen.LoadSceneRoutine()` so the loading screen owns the transition instead of firing as a detached scene-load call. If no loading screen exists, the existing `VRScreenFade` fades to black before loading the main menu.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Scripts/Managers/GameManager.cs`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
- **Components/assets/settings:** `GameManager` references `VRScreenFade` on `GameManager`, `VRLoadingScreen` on `VR Loading Screen`, and `mainMenuSceneName=MainMenuScene`. `VR Pause Menu` has `EchoRoom.UI.VRPauseMenu`, `UIDocument`, `BoxCollider`, and `XRUIToolkitManager` active in `MainScene`.
- **Decisions and assumptions:** The visible roughness was caused by `VRPauseMenu.ReturnToMenu()` hiding/restoring player overlay visuals before the loading UI rendered. Keeping the menu visible until the loading transition begins avoids exposing the controller rig for a frame. The world-space loading screen remains the primary transition surface; `VRScreenFade` is only a fallback.
- **Verification:** Direct Unity MCP inspection confirmed `MainScene` is open and not dirty, `GameManager`, `VR Pause Menu`, and `VR Loading Screen` are active at the recorded paths, and `GameManager` is wired to `VRScreenFade`, `VRLoadingScreen`, and `MainMenuScene`. MCP AssetDatabase refresh completed successfully. Fresh Unity Console error query showed no new compile errors; only the pre-existing MCP warning about spaces in the project path remains.
- **Known limitations:** Headset Play Mode was not run in this pass, so the subjective smoothness should still be checked in VR. If a flash remains, the next step is to make `VRLoadingScreen` draw a camera-attached black fade/backplate for the first frame before showing progress text.
- **Follow-up:** Test Return to Main Menu from pause/captured/time-up states in headset and tune `VRLoadingScreen.minimumDisplayTime` or add a first-frame black cover if needed.


### LIGHT-SONAR-RESPONSE-001 — More responsive revealed maze lighting

- **Date:** 2026-07-13
- **Goal:** Make maze materials respond more naturally to realtime light intensity while preserving the sonar reveal mechanic.
- **Result:** Updated `EchoRoom/EchoSonarReveal` so revealed maze pixels use a tunable realtime light response instead of the previous hard compression that made intensity 4 and intensity 10 look too similar. Unrevealed pixels remain black and the sonar visibility mask still controls when geometry appears.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** No scene or prefab object was serialized by this change. Renderer objects using `EchoRoom/EchoSonarReveal`, previously enumerated under `LIGHT-SONAR-001` and `LEVEL-INDIRECT-001`, inherit the shader behavior.
- **Components/assets/settings:** Added `_RealtimeLightResponse` (`Realtime Light Response`, Range 0-4, default 1.35) and `_RealtimeLightCompression` (`Realtime Light Compression`, Range 0-1, default 0.2). Both geometric-normal and normal-map lighting paths now multiply realtime contribution by response and use the softer tunable compression. Existing `_EnvironmentLightInfluence`, `_EnvironmentAmbientInfluence`, `_RevealBrightness`, sonar pulse buffer, black `baseCol`, and `visibilityMask` blend remain intact.
- **Decisions and assumptions:** Kept the sonar reveal gate unchanged so lights cannot reveal maze geometry before a ping. Chose conservative defaults to improve light response without immediately blowing out the existing bright maze lights.
- **Verification:** Direct Unity MCP refresh completed successfully. `Shader.isSupported=True`, `ShaderUtil` reported zero shader messages, and the Unity Console showed no fresh errors. MCP source verification confirmed both new properties and CBUFFER fields are present, both realtime lighting paths use the new response math, and the black base plus `lerp(baseCol, revealedCol, visibilityMask)` reveal gate remain present.
- **Known limitations:** Existing maze lights include high intensities such as 4 and 10, so headset testing may now feel brighter than before. If so, reduce maze Point Light intensity before reducing the shader response.
- **Follow-up:** In headset, test a ping near Maze A lights. If revealed surfaces are too bright, lower Point Lights first to roughly 2-4, then tune `Realtime Light Response` or `Realtime Light Compression` on materials only if needed.


### LEVEL-FOG-001 — Per-level performance fog controller

- **Date:** 2026-07-13
- **Goal:** Add level fog in a performance-efficient way that works with the sonar reveal mechanics.
- **Result:** Added `EchoFogController`, an inexpensive `RenderSettings` linear fog controller with Inspector-tunable default, tutorial, and Maze A-E profiles. The controller listens to `GameManager.OnLevelLoaded` for normal levels and applies a tutorial fog profile through a null-safe hook in `GameManager.LoadTutorialLevel()`. The fog changes only Unity distance fog; `EchoRoom/EchoSonarReveal` still gates surface visibility, so fog affects revealed surfaces without making hidden geometry visible before sonar.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Managers/EchoFogController.cs`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Managers/GameManager.cs`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager`
- **Components/assets/settings:** Added `EchoFogController` to `GameManager`. The component defaults to `FogMode.Linear`, default fog color `(0.015, 0.020, 0.035, 1)`, start `16`, end `62`; tutorial fog color `(0.020, 0.024, 0.040, 1)`, start `18`, end `70`; Maze profiles use progressively closer end distances from Maze A `64` to Maze E `46`. MCP inspection confirmed project fog was already enabled, URP depth texture support is enabled, opaque texture is disabled, and 86 scene renderers use `EchoRoom/EchoSonarReveal`.
- **Decisions and assumptions:** Used built-in linear distance fog instead of volumetric fog, fog particles, transparent hallway volumes, or camera opaque texture effects because this is the cheapest VR-friendly approach and the existing sonar shader already mixes fog only into revealed surface color. Kept fog start distances outside the main close-range sonar readability zone so nearby pings remain readable.
- **Verification:** Unity MCP created/imported `EchoFogController.cs`, refreshed the AssetDatabase, confirmed `EchoFogController` compiles as a valid `MonoBehaviour`, confirmed `GameManager.cs` contains the tutorial fog hook, added and saved the `EchoFogController` component on `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager`, and confirmed `MainScene` is loaded and not dirty after save.
- **Known limitations:** Play Mode/headset visual tuning was not run, so the exact fog density and color feel should still be checked in VR. Current profiles are conservative starting points and may need per-maze art tuning.
- **Follow-up:** In headset, test Maze A and Maze E pings at corridor distance. If pings feel muddy, increase each profile's fog end distance first; if the maze feels too clear, lower end distance in small steps.

## 2026-07-13 — AO-REVEAL-VISIBILITY-001 — Revealed-surface AO visibility control

- **Goal:** Make ambient occlusion visibly affect sonar-revealed maze surfaces without revealing hidden geometry.
- **Resulting behavior:** `EchoRoom/EchoSonarReveal` now applies the sampled screen-space AO once more to the final revealed surface color. Hidden pixels remain black, while revealed wall/floor intersections receive stronger contact darkening.
- **Files modified:**
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Docs/PROJECT_MEMORY.md`
- **Files created, moved, or deleted:** None.
- **Affected Unity objects:** None — shader asset change only; no scene or prefab object was serialized.
- **Components/assets/settings:**
  - Shader asset: `EchoRoom/EchoSonarReveal` at `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`.
  - Added `_AOVisibilityStrength` as a `Range(0, 1)` material property with default `0.75`.
  - Final revealed color uses the minimum of indirect and direct screen-space AO, blended by `_AOVisibilityStrength`.
  - Existing renderer feature `Maze Screen Space Ambient Occlusion` and its URP settings were not changed.
- **Decisions/assumptions:** Kept the sonar visibility rule intact and strengthened AO only after a surface is revealed. No Volume or per-level AO setup was added.
- **Verification:** Applied and imported through the Unity Editor MCP connection. Shader loaded and reported supported; `_AOVisibilityStrength` was discoverable; each inserted declaration/operation occurred exactly once; Unity reported zero shader compilation messages. Active scene `Assets/_EchoRoom/Scenes/MainScene.unity` dirty state was `True` and the scene was not saved by this change.
- **Known limitations:** Headset/play-mode visual tuning was not performed; the default strength may need adjustment for the target display.
- **Follow-up:** In headset, trigger sonar near wall/floor intersections and tune `AO Visibility Strength` if the effect is too strong or subtle.


## 2026-07-13 — AO-REVEAL-RUNTIME-002 — Shader-accessible SSAO with VR cost compensation

- **Goal:** Make ambient occlusion reliably affect sonar-revealed surfaces while keeping Play Mode performance effectively unchanged.
- **Resulting behavior:** The active URP renderer now generates SSAO before opaque surface shading, so `EchoRoom/EchoSonarReveal` can receive `_SCREEN_SPACE_OCCLUSION` and sample the SSAO texture during its reveal lighting. SSAO remains Depth-based and downsampled; reconstructed normal samples were reduced from Medium to Low to compensate for the additional shader-accessible AO texture.
- **Files modified:**
  - `Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset`
  - `Docs/PROJECT_MEMORY.md`
- **Files created, moved, or deleted:** None.
- **Affected Unity objects:** None — renderer asset and project journal only; no scene or prefab object was serialized.
- **Components/assets/settings:**
  - Renderer asset: `Assets/_EchoRoom/RenderPipeline/Default URP_Renderer.asset`.
  - Renderer feature: `Maze Screen Space Ambient Occlusion`.
  - `After Opaque`: `True → False`.
  - `Normal Samples`: `Medium → Low`.
  - Preserved `Source=Depth`, `Downsample=True`, `Intensity=3.5`, `Radius=0.22`, `Falloff=100`, `Direct Lighting Strength=0.45`, `Blur Quality=High`, and `AO Method=Blue Noise`.
  - Reveal shader remains `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`; no shader or material values were changed.
- **Decisions/assumptions:** Disabling After Opaque is required by this URP version for opaque shaders to sample SSAO. Low reconstructed-normal sampling offsets the VR cost while High blur remains enabled to stabilize the downsampled result. Existing AO intensity and radius were intentionally preserved to avoid another aesthetic retune.
- **Verification:** Unity MCP readback confirmed `AfterOpaque=False`, `NormalSamples=Low`, Depth source and Downsample enabled, with every other listed SSAO setting unchanged. The reveal shader is supported, has three passes, and reports no shader errors. A fresh Unity Console Error query returned no entries. Two equivalent 90-frame Play Mode samples measured baseline average/median/p95 = `10.000/9.355/14.745 ms` and post-change = `10.111/9.429/14.910 ms`; the approximately 0.11 ms average difference is within normal Editor sampling variance.
- **Known limitations:** Editor Play Mode timing is not a substitute for headset GPU profiling. `MainScene` was already dirty before this renderer-asset change and was not saved or otherwise modified.
- **Follow-up:** Validate the stronger wall/floor contact darkening in headset. If GPU headroom is unexpectedly worse on-device, keep this functional ordering and reduce SSAO Blur Quality before reverting After Opaque, because reverting After Opaque would again disconnect AO from the reveal shader.


## 2026-07-13 — AO-PLAYTEST-LOGGER-001 — Persistent AO playtest diagnostics

- **Goal:** Capture enough runtime evidence during a real AO/sonar playtest to diagnose renderer configuration, reveal state, viewed materials, XR timing, and failures afterward.
- **Resulting behavior:** Every Play Mode session now auto-creates a lightweight `AO Playtest Logger` in the runtime `DontDestroyOnLoad` scene. It writes structured JSON Lines to `Logs/AOPlaytests/AOPlaytest_latest.jsonl`, overwriting the previous session so the latest test is unambiguous. The file permits concurrent reads, allowing MCP inspection while Play Mode is still running.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Diagnostics/AOPlaytestLogger.cs`
  - `Assets/_EchoRoom/Scripts/Diagnostics/AOPlaytestLogger.cs.meta`
  - `Logs/AOPlaytests/AOPlaytest_latest.jsonl` (runtime-generated verification log; overwritten on each Play Mode start)
- **Files modified:**
  - `Docs/PROJECT_MEMORY.md`
- **Files moved or deleted:** None.
- **Affected Unity objects:**
  - `Runtime DontDestroyOnLoad scene (not a serialized asset) :: AO Playtest Logger`
- **Components/assets/settings:** `EchoRoom.Diagnostics.AOPlaytestLogger` uses `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`, so no scene or prefab serialization is required. Default sampling interval is 1 second and buffered flush interval is 5 seconds. It records session/device/graphics metadata, complete active URP SSAO settings and changes, reveal shader support, Main Camera depth status, XR display refresh rate, CPU/GPU frame timing when Unity supplies it, averaged/max frame time, sonar pulse origins/start times, the center-view reveal renderer/material, AO Visibility Strength, Reveal Quality, occlusion-map state, and a CPU reconstruction of the target point's sonar reveal amount. It also records warnings/errors and emits health events for missing/inactive SSAO, After Opaque regression, missing depth, zero material AO visibility, or sustained frame-budget overruns.
- **Decisions/assumptions:** Runtime bootstrap was chosen because `MainScene` was already dirty before this task; saving a temporary logger component would also have serialized unrelated dirty scene state. The log is outside `Assets` to avoid AssetDatabase refresh/import work during VR testing. Disk writes are buffered and sampling is once per second to minimize measurement distortion.
- **Verification:** Unity compiled `AOPlaytestLogger` into `Assembly-CSharp` with no script compilation errors. MCP Play Mode smoke testing directly inspected the runtime object as `AO Playtest Logger` in `DontDestroyOnLoad`, read the log concurrently while it was open, and confirmed the log reported the corrected configuration: feature active, `AfterOpaque=False`, `Downsample=True`, `Source=Depth`, `NormalSamples=Low`, pipeline depth enabled, and reveal shader found/supported. The smoke log contained 17 JSONL events and ended cleanly with `session_end` at frame 760. No new logger exception or compilation error was reported; existing MCP project-path warnings remain unrelated.
- **Known limitations:** Unity may report GPU frame time as zero on platforms or Editor configurations that do not expose Frame Timing data. Center-view material diagnostics require the viewed reveal surface to have a Physics collider. The latest log is intentionally overwritten when a new Play Mode session starts.
- **Follow-up:** Run the intended headset playtest, trigger several sonar pings while looking at wall/floor intersections where AO should be visible, exit Play Mode, then ask Codex to read the AO playtest log and diagnose it before starting another Play Mode session.
