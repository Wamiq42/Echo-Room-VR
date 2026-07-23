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

| Purpose | Unity object |
| --- | --- |
| Main-menu world-space UI document | `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu` |
| Main-menu persistent loading UI document | `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen` |
| Gameplay pause/failure world-space UI document | `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu` |
| Gameplay loading UI document | `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen` |
| Tutorial runtime director | `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System` |
| Runtime-created UI Toolkit tutorial prompt | `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System/Tutorial UI Toolkit Prompt` |

### Important code and assets

| Purpose | Path |
| --- | --- |
| Shared menu authoring layout | `Assets/_EchoRoom/Resources/UI/VRMenu.uxml` |
| Shared menu stylesheet | `Assets/_EchoRoom/Resources/UI/VRMenu.uss` |
| Loading authoring layout | `Assets/_EchoRoom/Resources/UI/VRLoadingScreen.uxml` |
| Loading stylesheet | `Assets/_EchoRoom/Resources/UI/VRLoadingScreen.uss` |
| Shared world-space UI Toolkit panel settings | `Assets/_EchoRoom/Resources/UI/VRMenuPanelSettings.asset` |
| Tutorial prompt UI Toolkit layout | `Assets/_EchoRoom/Resources/UI/VRTutorialPrompt.uxml` |
| Tutorial prompt UI Toolkit stylesheet | `Assets/_EchoRoom/Resources/UI/VRTutorialPromptStyles.uss` |
| Tutorial sequence and runtime prompt | `Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs` |
| Tutorial runtime diagnostics | `Assets/_EchoRoom/Scripts/Tutorial/TutorialRuntimeObserver.cs` |

### Unity MCP connection

| Purpose | Value |
| --- | --- |
| Integration | Ivan Murzak Unity-MCP / AI Game Developer |
| Unity Connection window server URL | `http://localhost:26566` |
| Codex project MCP endpoint | `http://localhost:26566/p/a679b99a` |
| Transport | `http` |
| Local authorization | `none` (no authorization token) |
| Codex configuration | `.codex/config.toml` under `[mcp_servers.ai-game-developer]` |

Connection procedure: open this Unity project, open the Ivan Murzak AI Game Developer/Connection window, select **Custom**, use `http://localhost:26566`, select **http** transport, set Authorization Token to **none**, and start the MCP server. Wait until the window reports **Unity: Connected** and **MCP server: Running (http)**. Codex should then use the project-scoped endpoint recorded above. The orange **AI agent** indicator only means that an MCP client is not currently attached; it should change after the client connects.

## Active Decisions and Conventions

| ID | Decision | Reason | Status |
| --- | --- | --- | --- |
| DEC-0001 | Use `Docs/PROJECT_MEMORY.md` as the canonical project-local memory. | It is versionable, human-readable, and available across sessions and tools. | Active |
| DEC-0002 | Track only files and Unity objects affected by documented work, not every third-party asset in the repository. | A complete inventory would be noisy and would not describe change history. | Active |
| DEC-0003 | Direct Unity inspection is authoritative for scene, prefab, GameObject, and component paths. | It prevents stale or invented hierarchy references. | Active |
| DEC-0004 | Use the loopback Ivan Murzak Unity-MCP server at `http://localhost:26566` with HTTP transport and no local authorization token; Codex uses the configured project route. | This matches the Unity Connection window and the repository-local Codex MCP configuration. | Active |

## Open Work and Known Limitations

- The memory is persistent within this repository, but it is not an automatic account-wide memory. A person or agent must read this file to use it.
- Changes made by tools or people who do not update this file will not appear automatically.
- History before 2026-07-05 has not yet been reconstructed.
- The live Unity scene/object index is intentionally partial and should be expanded only from verified work.

## Change Journal

### MAINMENU-BAKED-LIGHTING-001 ? Bake tutorial T-junction lighting in main menu

- **Date:** 2026-07-17
- **Goal:** Make the tutorial T-junction environment in `MainMenuScene` respond to baked lighting.
- **Result:** Configured `MainMenuScene` with its own lighting settings asset, marked the stripped tutorial environment renderers for baked GI/lightmaps, converted the scene lights outside the XR rig to baked lighting, and baked the scene. `MainMenuScene` now has assigned baked lighting data, two active lightmaps, and all 14 tutorial environment renderers have valid lightmap indices/scale offsets.
- **Files created:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/LightingData.asset`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/LightingData.asset.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-0_comp_dir.png`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-0_comp_dir.png.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-0_comp_light.exr`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-0_comp_light.exr.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-1_comp_dir.png`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-1_comp_dir.png.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-1_comp_light.exr`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-1_comp_light.exr.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-2_comp_dir.png`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-2_comp_dir.png.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-2_comp_light.exr`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-2_comp_light.exr.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-3_comp_dir.png`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-3_comp_dir.png.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-3_comp_light.exr`
  - `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-3_comp_light.exr.meta`
  - `Assets/_EchoRoom/Scenes/MainMenuScene_LightingSettings.lighting`
  - `Assets/_EchoRoom/Scenes/MainMenuScene_LightingSettings.lighting.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Floor`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Wall (1)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Interaction Wall`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Wall With Corridor Face`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Long Wall`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Long Wall (2)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Door`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Door/Knob`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Frame`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Long Wall (1)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/tip`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/tip (1)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Entity wall`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Point Light`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Point Light (1)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Point Light (2)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Directional Light`
- **Components/assets/settings:** Added `Assets/_EchoRoom/Scenes/MainMenuScene_LightingSettings.lighting` using baked GI, realtime GI disabled, Progressive CPU lightmapper, 16 texels/unit lightmap resolution, 1024 max lightmap size, combined directional lightmaps, ambient occlusion enabled, 2 bounces, normal-quality compression. Set the tutorial renderers to `ContributeGI`, positive scale in lightmap, and serialized lightmap indices/scale offsets. Converted the non-XR scene lights to `LightmapBakeType.Baked` with soft shadows; the XR camera point light was left out because it is inactive and belongs to the rig.
- **Decisions and assumptions:** Investigation showed the T-junction was already static and marked `ContributeGI`, but `MainMenuScene` had `lightmaps=0`, so there was no baked data for it to use. The fix therefore baked `MainMenuScene` directly instead of adding runtime lighting scripts or re-enabling tutorial systems.
- **Verification:** MCP verification after baking reported `lightmaps=2`, `lightingData=Assets/_EchoRoom/Scenes/MainMenuScene/LightingData.asset`, `settings=Assets/_EchoRoom/Scenes/MainMenuScene_LightingSettings.lighting`, `renderers=14`, `contribute=14`, `lightmapped=14`, and `scaleOffsets=14`. Every tutorial renderer reported a valid lightmap index (`0` or `1`) and non-zero scale offset. `MainMenuScene` saved with dirty=false. Editor state reported playing=false, paused=false, compiling=false, updating=false. A later console query still included the earlier failed inspection script error caused by using an unavailable `Renderer.receiveGI` API; the successful bake and verification did not report shader or bake failures. Attempting to clear the Unity MCP log cache failed because Unity had the MCP log file open.
- **Known limitations:** The bake was verified through Editor/MCP data, not a headset visual pass. Unity also produced unused lightmap texture files 2 and 3 in the scene lighting folder even though `LightmapSettings.lightmaps.Length` is 2; they were left untouched because they are Unity-generated bake artifacts.
- **Follow-up:** Visually inspect `MainMenuScene` in Play Mode/headset to confirm the hallway lighting has the desired contrast and rebake at higher resolution if the menu environment needs a more polished final look.


### MAINMENU-TUTORIAL-ENV-001 ? Main menu in tutorial T-junction hallway

- **Date:** 2026-07-17
- **Goal:** Put the main menu player into the tutorial T-junction environment at the start of the big hallway, facing toward the two arms of the T, with only visual environment geometry and floor/collision support.
- **Result:** Replaced the prior standalone arrival platform under `Main Menu Environment` with a stripped visual copy of `T_Junction_Tutorial`. The player now starts at `(0.000, -0.180, -12.000)` facing down `+Z` toward the T arms. The `Main Menu` and `VR Loading Screen` are centered ahead in the hallway at `(0.000, 1.150, -8.750)`, rotation `(0.000, 180.000, 0.000)`, with readable negative-X UI scale preserved. Controller menu rays remain active in `MainMenuScene` and hit the menu collider.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Arrival Platform`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Floor`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Wall (1)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Interaction Wall`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Wall With Corridor Face`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Long Wall`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Long Wall (2)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/tip`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/tip (1)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Entity wall`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Point Light`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Point Light (1)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Point Light (2)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray`
- **Components/assets/settings:** Source prefab was `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab`, instantiated as a scene-only stripped copy. Tutorial/interactable/text/audio/nav/helper MonoBehaviours were removed from the copied environment, as were objects `StartCheckpoint`, `Tutorial Button`, `Tutorial Lever`, all wall-tip TextMeshPro objects, and `Tutorial Ending Audio`. Verification found `monoBehaviours=0`, `audioSources=0`, `renderers=14`, `enabledColliders=13`, and `floorColliders=1` under the tutorial copy. The menu keeps `MenuRay_AlwaysOnTop` ray material behavior from the prior ray-rendering fix.
- **Decisions and assumptions:** The start of the big hallway was taken as the corridor near local `z=-12`, before the T opens near `z=14..20`. Facing toward the two arms means player forward is `+Z`. The tutorial copy is intentionally not a prefab instance dependency for gameplay; it is a static menu-scene visual environment with collision, not a live tutorial system.
- **Verification:** MCP verification opened `MainMenuScene` and reported dirty=false. `Tutorial T Junction Environment` exists, contains zero MonoBehaviours and zero AudioSources, has one enabled non-trigger floor collider, and no removed tutorial extras present. `XR Origin (XR Rig)` is at `(0.000, -0.180, -12.000)`, rotation `(0.000, 0.000, 0.000)`, forward `(0.000, 0.000, 1.000)`. `Main Menu` and `VR Loading Screen` are at `(0.000, 1.150, -8.750)`, rotation `(0.000, 180.000, 0.000)`. Both left and right `Menu UI Ray` objects are active and raycast-hit the `Main Menu` collider at approximately `(0.000, 0.820, -8.753)`. Editor state reported playing=false, paused=false, compiling=false, updating=false. Recent console error query returned no errors.
- **Known limitations:** Not headset-tested. The exact hallway menu height/distance may need a comfort pass in headset.
- **Follow-up:** In headset, confirm the player starts at the hallway entrance, sees the T arms beyond the menu, stands on the tutorial floor, and can press menu buttons with both controller rays.


### UI-RAY-RENDER-001 ? Render controller menu rays over menu panels

- **Date:** 2026-07-17
- **Goal:** Fix the visible controller rays appearing behind the main menu panel while preserving menu readability and click hit detection.
- **Result:** Added an always-on-top transparent shader/material for the `Menu UI Ray` line renderers and assigned it in both menu-related scenes. The controller rays keep their existing raycast hit behavior, but the line visual now renders with `ZTest Always` in the overlay queue so it stays visible over the menu surface instead of being hidden by the panel.
- **Files created:**
  - `Assets/_EchoRoom/Shaders/MenuRayAlwaysOnTop.shader`
  - `Assets/_EchoRoom/Shaders/MenuRayAlwaysOnTop.shader.meta`
  - `Assets/_EchoRoom/Materials/MainMenu/MenuRay_AlwaysOnTop.mat`
  - `Assets/_EchoRoom/Materials/MainMenu/MenuRay_AlwaysOnTop.mat.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray`
- **Components/assets/settings:** Added shader `EchoRoom/UI/MenuRayAlwaysOnTop` with transparent overlay queue, `ZWrite Off`, `ZTest Always`, and vertex-color tint support. Created material `Assets/_EchoRoom/Materials/MainMenu/MenuRay_AlwaysOnTop.mat` with renderQueue `5000`. Assigned that material to each affected `LineRenderer`, set `sortingOrder=5000`, and kept `MainMenuScene` rays active by default while `MainScene` rays remain inactive until the pause menu enables them.
- **Decisions and assumptions:** Live Play Mode MCP inspection showed both controller rays physically raycast-hit the front of `MainMenuScene :: Main Menu`; the reported problem was visual occlusion behind the panel. Rendering the line visual over the UI fixes the visible ray position without moving the menu, shifting the collider, or weakening button hit detection.
- **Verification:** MCP live inspection before the fix found `boxRaycastHit=True` for both rays against the main menu front surface. Post-fix MCP verification found both scenes dirty=false; all four `Menu UI Ray` line renderers use material `MenuRay_AlwaysOnTop`, shader `EchoRoom/UI/MenuRayAlwaysOnTop`, renderQueue `5000`, sortingOrder `5000`, and raycast hit=true. Editor state reported playing=false, paused=false, compiling=false, updating=false. Recent console errors were the existing Unity MCP project-path-spaces warning/error and the earlier expected failed attempt to open scenes while Play Mode was active; no shader/material error was reported.
- **Known limitations:** Not headset-tested after the material assignment. A visual headset check should confirm the red line is drawn over the menu panel and still clicks UI buttons.
- **Follow-up:** Enter `MainMenuScene`, aim both controllers at the menu, and confirm the rays visibly appear on top of the panel instead of behind it.


### UI-READABLE-ORIENTATION-001 ? Restore readable menu UI orientation

- **Date:** 2026-07-17
- **Goal:** Correct the horizontally flipped visual orientation of the main menu and loading screen in both scenes while preserving world-space placement and controller ray alignment.
- **Result:** Restored the UI Toolkit-readable negative X scale for `VRMainMenu`, `VRLoadingScreen`, and `VRPauseMenu` fixed menu panels while keeping the corrected `89.752` yaw that faces the player and lets controller rays hit the menu colliders.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRMainMenu.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
- **Components/assets/settings:** `VRMainMenu` and `VRLoadingScreen` again use `new Vector3(-worldScale, worldScale, worldScale)` for their fixed world-space UI panels. `VRPauseMenu.fixedStartMenuScale` is `(-0.002, 0.002, 0.002)`, and `VRPauseMenu.ConfigureDocument` again forces negative X scale for UI Toolkit readability. Scene instances are serialized with negative X scale and yaw `89.752`.
- **Decisions and assumptions:** MCP transform math showed the panels were facing the player, but the user-visible UI remained horizontally flipped. This indicates the UI Toolkit panel needs the negative X scale for readable left-to-right presentation in this setup. The ray-facing yaw from `UI-RAY-ALIGN-001` was kept so controller rays still intersect the menu surface.
- **Verification:** MCP verification found `MainMenuScene :: Main Menu` and `MainMenuScene :: VR Loading Screen` at `(-3.040, 0.950, -1.342)`, rotation `(0.000, 89.752, 0.000)`, scale `(-0.001600, 0.001600, 0.001600)`, with both menu rays active and hit=true. `MainScene :: VR Pause Menu` uses scale `(-0.002000, 0.002000, 0.002000)` and `MainScene :: VR Loading Screen` uses scale `(-0.001600, 0.001600, 0.001600)`, both at yaw `89.752`; saved rays are inactive for the hidden pause menu but hit=true in raycast tests. Both scenes reported dirty=false. Editor state reported playing=false, paused=false, compiling=false, updating=false. Recent console errors were only the existing Unity MCP plugin project-path-spaces warning/error.
- **Known limitations:** Not headset-tested. A headset pass should confirm text reads left-to-right and the controller rays still visually land on the panels.
- **Follow-up:** In headset, inspect `MainMenuScene` and trigger a loading screen; confirm the text is no longer horizontally flipped and both controllers can click menu buttons.


### UI-RAY-ALIGN-001 ? Align controller rays with world-space menus

- **Date:** 2026-07-17
- **Goal:** Fix controller UI rays not landing on the world-space menus in both the main menu scene and gameplay scene.
- **Result:** Rotated fixed world-space menu/loading anchors from yaw `269.752` to `89.752` so the UI faces the player and controller rays approach the front side of the panels. `MainMenuScene` now opens with both `Menu UI Ray` objects active. `VRPauseMenu` no longer forces a negative panel X scale and its delayed input activation now respects fixed start-menu placement instead of moving the panel back in front of the camera. The pause menu's controller ray references were serialized so it can enable the correct rays when visible.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRMainMenu.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray`
- **Components/assets/settings:** `VRMainMenu.fixedWorldEulerAngles`, `VRLoadingScreen.fixedWorldEulerAngles`, and `VRPauseMenu.fixedStartMenuEulerAngles` now use `(0, 89.752, 0)`. `VRPauseMenu.fixedStartMenuScale` is positive, and `ConfigureDocument` preserves positive X scale. `VRPauseMenu.EnableInputAfterLayout` uses fixed placement when the current state requires it. `MainMenuScene` `Menu UI Ray` GameObjects are active by default; `MainScene` rays remain inactive by default because the pause menu starts hidden and enables them when shown.
- **Decisions and assumptions:** The ray objects were already aimed close enough to the menu collider in both scenes, but the panels were using the old fixed rotation after the mirror-scale fix, meaning the UI face was pointed away from the player. A rotation-only correction preserves the fixed world anchor and avoids reintroducing negative scale.
- **Verification:** MCP ray verification found `MainMenuScene :: Main Menu` at `(-3.040, 0.950, -1.342)`, rotation `(0.000, 89.752, 0.000)`, scale `(0.001600, 0.001600, 0.001600)`, and both left/right `Menu UI Ray` objects active with raycast hit=true. `MainScene :: VR Pause Menu` reported the same anchor with scale `(0.002000, 0.002000, 0.002000)`; its saved rays were inactive as expected for hidden pause UI, but raycast hit=true and `VRPauseMenu` has valid pointer roots for runtime activation. Both scenes reported dirty=false. Editor state reported playing=false, paused=false, compiling=false, updating=false. Recent console errors were only the existing Unity MCP plugin project-path-spaces warning/error.
- **Known limitations:** Not headset-tested. Runtime XR controller poses can differ from edit-mode transforms, so a headset check should confirm the visible line ends on the menu surface and UI buttons receive clicks.
- **Follow-up:** In headset, open `MainMenuScene` and the gameplay pause menu, verify the left and right controller rays visibly land on the panels, and press a menu button from each hand.


### UI-FLIP-FIX-001 ? Correct menu and loading UI horizontal mirroring

- **Date:** 2026-07-17
- **Goal:** Fix the horizontally flipped main menu and loading screen while keeping them fixed in world space.
- **Result:** Removed the negative X scale used by `VRMainMenu` and `VRLoadingScreen`. The main menu and loading screen now use positive uniform world scale, so their UI should no longer render mirrored while staying at the same fixed anchor.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRMainMenu.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
- **Components/assets/settings:** Replaced `new Vector3(-worldScale, worldScale, worldScale)` with `new Vector3(worldScale, worldScale, worldScale)` in both UI placement scripts. Serialized the affected scene objects with local scale `(0.0016, 0.0016, 0.0016)`.
- **Decisions and assumptions:** Kept the existing fixed world position and rotation because the reported problem was horizontal mirroring, and MCP inspection showed negative X scale on both menu objects before the fix. The loading screen was corrected in both `MainMenuScene` and `MainScene` so the shared loading UI stays consistent.
- **Verification:** MCP verification found `VRMainMenu negativeScaleAssignments=0` and `VRLoadingScreen negativeScaleAssignments=0`. `MainMenuScene :: Main Menu` and `MainMenuScene :: VR Loading Screen` both reported scale `(0.001600, 0.001600, 0.001600)` and `mirroredX=False`; `MainScene :: VR Loading Screen` reported the same. Both inspected scenes reported dirty=false after save. Editor state reported playing=false, paused=false, compiling=false, updating=false. Recent console errors were only the existing Unity MCP plugin project-path-spaces warning/error.
- **Known limitations:** Not headset-tested. If the UI appears to face away after removing the mirror scale, the next adjustment should be a rotation-only correction, not reintroducing negative scale.
- **Follow-up:** Run `MainMenuScene`, confirm menu text is readable left-to-right, and trigger loading once to confirm the loading UI is also readable left-to-right.


### MAINMENU-SCENE-ANCHOR-001 ? Move arrival platform into menu scene

- **Date:** 2026-07-17
- **Goal:** Remove the standing platform from the gameplay `MainScene`, add it to the actual `MainMenuScene`, and make the main menu scene UI stay fixed in world space.
- **Result:** Removed `Main Menu Environment/Arrival Platform` from `Assets/_EchoRoom/Scenes/MainScene.unity`. Added the static octagonal arrival platform to `Assets/_EchoRoom/Scenes/MainMenuScene.unity` under the XR rig floor position at approximately `(-0.710, 0.000, -1.070)`. Updated `VRMainMenu` so the `Main Menu` object uses a fixed world anchor by default instead of following the camera in `LateUpdate`. Serialized the `Main Menu` and `VR Loading Screen` objects in `MainMenuScene` at the shared anchor position `(-3.040, 0.950, -1.342)` and rotation `(0.000, 269.752, 0.000)`.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRMainMenu.cs`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment/Arrival Platform`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Arrival Platform`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Arrival Platform/Octagonal Grounding Pad`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Arrival Platform/Outer Comfort Rim`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Arrival Platform/Inner Footing Ring`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Arrival Platform/Center Balance Ring`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Arrival Platform/Peripheral Grounding Markers`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen`
- **Components/assets/settings:** `VRMainMenu` now has `useFixedWorldPlacement`, `fixedWorldPosition`, and `fixedWorldEulerAngles` fields. When fixed placement is enabled, its placement method sets the menu transform to the serialized world anchor and returns instead of using `Camera.main.forward`. The arrival platform keeps the existing `ArrivalPlatform_Base`, `ArrivalPlatform_Detail`, and `ArrivalPlatform_Rim` materials, and the grounding pad includes a `MeshCollider`.
- **Decisions and assumptions:** `MainMenuScene` is the actual menu scene because Unity MCP asset inspection found `Assets/_EchoRoom/Scenes/MainMenuScene.unity`, and scene inspection found the `Main Menu` root there. The gameplay `MainScene` should not contain the standing platform, but its previously fixed `VR Loading Screen` behavior remains unchanged.
- **Verification:** MCP verification opened both scenes. `MainScene` reported dirty=false, `Main Menu Environment=false`, and `platform=false`. `MainMenuScene` reported dirty=false, `Main Menu Environment=true`, `platform=true`, platform position `(-0.710, 0.000, -1.070)`, and pad collider=true. `MainMenuScene :: Main Menu` reported fixed=true at `(-3.040, 0.950, -1.342)` with rotation `(0.000, 269.752, 0.000)`. `MainMenuScene :: VR Loading Screen` reported fixed=true at the same anchor. Editor state reported playing=false, paused=false, compiling=false, updating=false. Recent console errors were only the existing Unity MCP plugin project-path-spaces warning/error.
- **Known limitations:** Not headset-tested. The fixed menu anchor and platform placement should be checked in VR to confirm the player stands on the platform and the menu remains comfortably readable while turning the head.
- **Follow-up:** Run `MainMenuScene` in headset or Play Mode, rotate the head, and confirm the menu and loading screen stay in the room instead of following view direction.


### UI-WORLD-ANCHOR-001 ? Fixed main menu and loading UI world anchors

- **Date:** 2026-07-17
- **Goal:** Stop the main menu UI and loading screen from following the player's head/camera direction, so both remain in a stable world-space location.
- **Result:** Main menu/start UI now uses a fixed world anchor instead of camera-relative placement. The loading screen also uses the same fixed world anchor whenever shown, including during its visible update loop. The shared anchor matches the existing menu pose at approximately position `(-3.040, 0.950, -1.342)` and rotation `(0.000, 269.752, 0.000)`.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
- **Components/assets/settings:** `VRPauseMenu` now has fixed-start-menu placement fields and uses them for the `Start` state and settings opened from the start menu; pause/captured gameplay menus still use camera-relative placement so they remain usable outside the main menu. `VRLoadingScreen` now has fixed-world-placement fields and uses them instead of camera-relative placement when visible. The scene instances serialize the fixed anchor values.
- **Decisions and assumptions:** The user's report targeted the main menu and loading screen, not the in-game pause/captured menus. Preserving camera-relative placement for pause/captured states avoids making gameplay failure or pause UI appear back at the main menu location during a maze.
- **Verification:** Unity MCP inspection showed `VR Pause Menu` at `(-3.040, 0.950, -1.342)`, rotation `(0.000, 269.752, 0.000)`, scale `(-0.0020, 0.0020, 0.0020)`, with `useFixedStartMenuPlacement=True`. `VR Loading Screen` is at the same position and rotation, scale `(-0.0016, 0.0016, 0.0016)`, with `useFixedWorldPlacement=True`. `MainScene` reported dirty=false after save. Editor state reported playing=false, paused=false, compiling=false, updating=false. Recent console query showed no UI/script compile errors; it did show the existing Unity MCP plugin project-path-spaces error.
- **Known limitations:** Not headset-tested. The fixed anchor should be checked in VR to confirm both panels are comfortably readable from the grounded arrival platform.
- **Follow-up:** In headset, rotate the head on the main menu and during loading to confirm the panels stay in place in the room instead of following view direction.


### MAINMENU-ARRIVAL-001 ? Main menu grounded arrival platform

- **Date:** 2026-07-17
- **Goal:** Give the player a stable object to stand on in the main menu so the menu no longer feels like a floating void.
- **Result:** Added a static world-space `Main Menu Environment` with an octagonal arrival platform centered under the active XR rig at approximately `(-0.710, 0.000, -1.070)`. The pad top sits at floor height, with thickness extending downward, a subtle cyan outer rim, two quiet footing rings, and four low peripheral reference markers for comfort grounding.
- **Files created:**
  - `Assets/_EchoRoom/Materials/MainMenu.meta`
  - `Assets/_EchoRoom/Materials/MainMenu/ArrivalPlatform_Base.mat`
  - `Assets/_EchoRoom/Materials/MainMenu/ArrivalPlatform_Base.mat.meta`
  - `Assets/_EchoRoom/Materials/MainMenu/ArrivalPlatform_Detail.mat`
  - `Assets/_EchoRoom/Materials/MainMenu/ArrivalPlatform_Detail.mat.meta`
  - `Assets/_EchoRoom/Materials/MainMenu/ArrivalPlatform_Rim.mat`
  - `Assets/_EchoRoom/Materials/MainMenu/ArrivalPlatform_Rim.mat.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment/Arrival Platform`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment/Arrival Platform/Octagonal Grounding Pad`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment/Arrival Platform/Outer Comfort Rim`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment/Arrival Platform/Inner Footing Ring`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment/Arrival Platform/Center Balance Ring`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Main Menu Environment/Arrival Platform/Peripheral Grounding Markers`
- **Components/assets/settings:** `Octagonal Grounding Pad` uses a custom eight-sided prism mesh with `MeshFilter`, `MeshRenderer`, and `MeshCollider`. The outer rim, inner rings, and peripheral markers use static cube primitives with shared materials. Materials use URP Lit when available: dark matte base, subtle detail material, and cyan-emissive rim material. Platform hierarchy is not parented to the camera or XR rig.
- **Decisions and assumptions:** Kept the geometry stationary and world-locked to reduce nausea from standing in empty space. The platform top remains at `Y=0` because live MCP inspection showed the active XR rig floor origin near zero. The shape is octagonal instead of a plain plane so the menu spawn feels intentional and has peripheral reference cues.
- **Verification:** Unity MCP script execution returned `Saved=True` for `Assets/_EchoRoom/Scenes/MainScene.unity`. MCP scene inspection found `Main Menu Environment/Arrival Platform` active with bounds centered near `(-0.710, 0.050, -1.070)` and size about `(5.144, 0.420, 5.144)`. The scene reported `IsDirty=false`. Editor state reported playing=false, paused=false, compiling=false, updating=false. Recent MCP console error query returned no errors.
- **Known limitations:** No physical headset comfort test was run. The final feel should still be checked in VR to confirm the platform scale, rim brightness, and marker visibility are comfortable in stereo.
- **Follow-up:** In headset, verify the player starts centered on the pad and the main menu remains readable without the platform edge or markers feeling too close.


### POLISH-INTEGRATION-001 — Complete and audit the coordinated polish wave

- **Date:** 2026-07-15
- **Goal:** Integrate the nine independently assigned polish packages, confirm their combined Unity state, and close the coordination queue without losing cross-task behavior.
- **Result:** POLISH-01 through POLISH-09 are complete. Updated the task index to show every package complete. The combined project now includes corrected movement speeds, explicit button/microphone ping profiles, restrained haptics, persistent settings and turn modes, floor-safe teleport, actual-motion comfort vignette, deterministic seated height offset, spatial door audio, and representative surface/size-aware echoes.
- **Files created/moved/deleted:** None in this integration-only audit.
- **Files modified:**
  - `Docs/PolishTasks/README.md`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:** None — this entry records an integration audit and documentation status update only.
- **Components/assets/settings:** Final live Unity audit loaded the authoritative XR rig and all six shipped level/tutorial prefabs. It found exactly one `DynamicSprintController`, `PingEmitter`, `TurnSettingsController`, `TeleportLocomotionController`, `ComfortVignetteController`, and `HeightOffsetController` on/in the rig; movement remained `2.0/3.5`; ping profiles remained `10/10/2.5` and `16/16/4`; shipped levels contained exactly eleven `EchoTeleportationArea` components, five valid `Door` components, three representative Maze C `EchoSurface` components, and zero missing scripts. `EchoRoom/EchoSonarReveal` remained supported with zero shader messages.
- **Decisions and assumptions:** Device-only acceptance remains outside Editor automation. The final release gate still requires Quest testing for comfort, stereo vignette coverage, controller arcs, seated reach/recenter, physical microphone input, haptic strength, door/echo loudness, sustained on-device frame rate, permissions/privacy, release configuration, and Meta submission requirements.
- **Correction/combined serialization note:** POLISH-03 itself did not save a scene, but the later POLISH-04 `MainScene` save serialized the new default proximity-haptic fields on `Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_E/Entity`. This is an intended combined result (`maxHapticDistance=8`, amplitude `0.08–0.55`, duration `0.08`, cadence `0.45–0.12`) and explains why the final `MainScene.unity` diff includes POLISH-03 settings even though its original entry described code-only serialization.
- **Verification:** Final dynamic Unity audit returned `pass=True; areas=11; doors=5; missing=0; shader=True; issues=[]`. The active scene was clean. A fresh five-minute Console Error query returned zero entries. Repository checks found no merge-conflict markers and no new Unity asset missing its `.meta` file. All nine `POLISH-XX-001` journal entries are present. Unity finished in Edit Mode, unpaused, not compiling, and not updating.
- **Known limitations:** No physical Quest headset was connected for the final integration audit. The worktree also contains unrelated/pre-existing Unity/package/configuration changes that were deliberately not cleaned or reverted.
- **Follow-up:** Perform one controlled Quest acceptance pass using the device-only checks from every POLISH task entry, then create a separate launch-gate checklist for on-device performance, permissions/privacy, full campaign regression, release configuration, store assets, and Meta submission.

### POLISH-QUEUE-001 — Split polish checklist into coordinated work packages

- **Date:** 2026-07-15
- **Goal:** Convert the monolithic polish checklist into corrected, independently assignable task handoffs suitable for coordinated multi-session implementation.
- **Result:** Added a coordination index and nine task files covering movement speeds, ping profiles, haptics, settings/turning, teleport, comfort vignette, height offset, door audio, and echo surface variation. Corrected the task assumptions for Unity 6000.3.8f1, URP 17.3.0, XRI 3.3.1, current resource paths, serialized XR rig speed overrides, per-material sonar reveal range, existing vignette assets, and existing door clips. Added explicit dependencies and shared-file serialization rules.
- **Files created:**
  - `Docs/PolishTasks/README.md`
  - `Docs/PolishTasks/POLISH-01-MOVEMENT-SPEEDS.md`
  - `Docs/PolishTasks/POLISH-02-PING-PROFILES.md`
  - `Docs/PolishTasks/POLISH-03-HAPTICS.md`
  - `Docs/PolishTasks/POLISH-04-SETTINGS-AND-TURNING.md`
  - `Docs/PolishTasks/POLISH-05-TELEPORT.md`
  - `Docs/PolishTasks/POLISH-06-VIGNETTE.md`
  - `Docs/PolishTasks/POLISH-07-HEIGHT-OFFSET.md`
  - `Docs/PolishTasks/POLISH-08-DOOR-AUDIO.md`
  - `Docs/PolishTasks/POLISH-09-ECHO-SURFACE-VARIATION.md`
- **Files modified:**
  - `Docs/HANDOFF.md`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** None — documentation and coordination change only.
- **Components/assets/settings:** No runtime component, asset, scene, prefab, package, or project setting was changed.
- **Decisions and assumptions:** The nine original work areas remain separate task identities, but their technical plans were corrected before assignment. Work touching the same scene, prefab, or script is sequenced instead of run concurrently. Headset-only checks are explicit acceptance steps and cannot be marked complete from Editor-only evidence.
- **Verification:** Confirmed all ten Markdown files exist under `Docs/PolishTasks/` with non-empty content. The live Unity MCP transport and Editor API were checked immediately before task creation; Unity reported not playing, not paused, not compiling, and not updating.
- **Known limitations:** These nine packages do not replace launch gates for on-device performance, microphone permissions/privacy, release configuration, full regression, or Meta Store submission.
- **Follow-up:** Dispatch the first non-overlapping work wave: POLISH-01, POLISH-03, and POLISH-08.

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


### POLISH-01-001 — Correct smooth locomotion speeds

- **Date:** 2026-07-15
- **Goal:** Set smooth locomotion to the GDD starting values of 2.0 m/s walking and 3.5 m/s sprinting without changing movement input, acceleration, turning, or sprint activation behavior.
- **Result:** `DynamicSprintController` source defaults now use 2.0/3.5, and the authoritative XR rig prefab serializes the same values. The loaded `MainScene` rig inherits both prefab values and has no independent `normalSpeed` or `sprintSpeed` property overrides.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Controller/DynamicSprintController.cs`
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/PlayerController`
- **Components/assets/settings:** `DynamicSprintController.normalSpeed=2.0` and `DynamicSprintController.sprintSpeed=3.5`. The existing `DynamicMoveProvider` and `PlayerInputManager` references and all other locomotion fields/logic were preserved.
- **Decisions and assumptions:** The live `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/PlayerController` instance was inspected but not serialized because Unity reported both speed fields as inherited rather than instance overrides. The prefab is therefore the authoritative serialized runtime value source.
- **Verification:** Unity AssetDatabase refresh and C# recompilation completed with `EditorUtility.scriptCompilationFailed=False`, `EditorApplication.isCompiling=False`, and the active `MainScene` remained clean. MCP prefab readback reported exactly one `DynamicSprintController` at the recorded object path with 2.0/3.5. MCP loaded-scene readback reported exactly one instance at `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/PlayerController`, also 2.0/3.5 with both override flags false. The focused Git diff contains only the two default changes and two prefab values; no movement input, acceleration, turning, or sprint activation code changed. Unity Console errors in the verification window were limited to MCP transport/dynamic-inspection diagnostics and contained no project-script compilation failure.
- **Known limitations:** Play Mode and headset comfort/feel were not tested during this coordinated multi-session pass. Physical sprint-input validation remains a user acceptance check.
- **Follow-up:** In headset, confirm comfortable walking at 2.0 m/s and sprinting at 3.5 m/s, then tune only if the GDD values feel unsuitable on target hardware.


### POLISH-08-001 — Spatial door open/close audio wiring

- **Date:** 2026-07-15
- **Goal:** Ensure every shipped maze/tutorial `Door` uses the existing wood-door open and close clips through a restrained spatial AudioSource without disturbing puzzle, animator, or prefab wiring.
- **Result:** Maze A-D now have explicit open/close clip references on their existing Door components. The shared brown-door source prefab now carries an explicit AudioSource reference and spatial settings, which propagate through `Door Frame.prefab` to the tutorial door. All five shipped doors use the same restrained 3D configuration. Maze E was inspected and correctly left unchanged because it contains no `Door` component.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Prefabs/Door_3_Brown.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/Door_3_Brown.prefab :: Door_3_Brown/Door`
  - `Assets/_EchoRoom/Prefabs/Door Frame.prefab :: Door Frame/Door_3_Brown/Door` (inherits the shared source-prefab change; asset serialization unchanged)
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Door Frame/Door_3_Brown/Door` (inherits through nested prefab sources; asset serialization unchanged)
- **Components/assets/settings:** Reused `Assets/_Third Party Assets/Free Wood Door Pack/Audio/Door_Open.wav` (0.5 seconds) and `Assets/_Third Party Assets/Free Wood Door Pack/Audio/Door_Close.wav` (1.1 seconds). Each affected `Door.asource` references the AudioSource on the same GameObject. AudioSource settings are `playOnAwake=false`, `loop=false`, volume `0.55`, spatial blend `1.0`, Doppler `0`, logarithmic rolloff, minimum distance `1 m`, and maximum distance `12 m`; no AudioSource was added or duplicated.
- **Decisions and assumptions:** Updated `Door_3_Brown.prefab` rather than adding tutorial overrides so its existing nested source chain remains authoritative. Patched Maze A-D directly because their Door components are local to each level prefab. `Assets/_EchoRoom/SObjects/LevelData.asset` was inspected through Unity and confirms the shipped campaign contains exactly Maze A-E; the separate legacy `Assets/_EchoRoom/Prefabs/Level - 1 (echo puzzle).prefab` is not referenced by that LevelData and was not changed. Existing `Door.SetOpen` state guarding and initialization logic were preserved: repeated requests for the current state return without playback, and `Start()` synchronizes the animator without playing a clip.
- **Verification:** Unity MCP readback after synchronous reimport found five shipped Door components total and validated 5/5 with non-null same-object AudioSource, exact open/close asset references, and all intended spatial settings. Animator references on Maze A-D remained assigned; the tutorial's existing animator fallback was unchanged. Maze E read back with zero Door components. The focused Git diff contains only the intended audio references/settings. Unity reported not playing, not paused, not compiling, and not updating. Recent Console errors were limited to pre-existing/shared MCP transport and dynamic-tool diagnostics; no project-script compilation failure or door-audio error was present.
- **Known limitations:** Coordinated sessions were simultaneously using the shared Unity Editor, so Play Mode was intentionally not entered. One-shot open/close playback and silent closed-state initialization are supported by the unchanged `Door.SetOpen`/`Start` control flow but still require an isolated Play Mode or headset confirmation. Final loudness and spatial feel require headset listening.
- **Follow-up:** In the next clear headset/Play Mode window, open and close one maze door and the tutorial door, confirm each transition plays once with no sound on initial closed-state load, and tune only volume/max distance if the mix feels too loud or carries too far.


### POLISH-03-001 — Restrained controller haptic feedback foundation

- **Date:** 2026-07-15
- **Goal:** Add reusable controller feedback for sonar emission, button/lever interaction, tracked-hand wall contact, and Entity proximity without per-frame device lookup or impulse spam.
- **Result:** Added a central XR haptic service that caches left/right controller devices and refreshes them on XR connection changes. Accepted sonar pings use right-hand feedback; right-trigger interactions identify the right controller; tracked-hand interactions and wall contacts resolve left/right from the existing hand hierarchy; Entity danger uses rate-limited bilateral pulses whose strength and cadence rise with closeness. Entity feedback stops when out of range, paused/menu-captured, disabled, destroyed, or captured.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Utility/EchoHaptics.cs`
  - `Assets/_EchoRoom/Scripts/Utility/EchoHaptics.cs.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/PingEmitter.cs`
  - `Assets/_EchoRoom/Scripts/Controller/SimpleControllerInteraction.cs`
  - `Assets/_EchoRoom/Scripts/Interactables/EchoButtonInteractable.cs`
  - `Assets/_EchoRoom/Scripts/Interactables/LeverInteractable.cs`
  - `Assets/_EchoRoom/Scripts/Interactables/HandPressCollider.cs`
  - `Assets/_EchoRoom/Scripts/AI/PingAttractedEntity.cs`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** No scene or prefab asset was serialized. Direct Unity inspection verified these principal runtime consumers:
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/PingEmiiter`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Hand Tracking/L_Wrist/L_Palm/Collider`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Hand Tracking/L_Wrist/L_IndexMetacarpal/L_IndexProximal/L_IndexIntermediate/L_IndexDistal/L_IndexTip/Collider`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Hand Tracking/R_Wrist/R_Palm/Collider`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Hand Tracking/R_Wrist/R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal/R_IndexTip/Collider`
  - `Assets/_EchoRoom/Prefabs/Colliders/Index Collider.prefab :: Index Collider`
  - `Assets/_EchoRoom/Prefabs/Colliders/Palm Collider.prefab :: Palm Collider`
  - `Assets/_EchoRoom/Prefabs/Auto_P3_Button Variant.prefab :: Auto_P3_Button Variant`
  - `Assets/_EchoRoom/Prefabs/PuzzleButton_Auto.prefab :: PuzzleButton_Auto`
  - `Assets/_EchoRoom/Prefabs/Tutorial/Tutorial Button.prefab :: Tutorial Button`
  - `Assets/_EchoRoom/Prefabs/Lever_P1_D1 Variant.prefab :: Lever_P1_D1 Variant`
  - `Assets/_EchoRoom/Prefabs/Tutorial/Tutorial Lever.prefab :: Tutorial Lever`
  - `Assets/_EchoRoom/Prefabs/Entity.prefab :: Entity`
  - `Assets/_EchoRoom/Prefabs/Tutorial/Tutorial Entity.prefab :: Tutorial Entity`
  - `Assets/_EchoRoom/Prefabs/Level - 1 (echo puzzle).prefab :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P1_D1`
  - `Assets/_EchoRoom/Prefabs/Level - 1 (echo puzzle).prefab :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P2_A4`
  - `Assets/_EchoRoom/Prefabs/Level - 1 (echo puzzle).prefab :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P3_E5`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P1_D1`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P2_A4`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P3_E5`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P1_Lever`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P2_Lever`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P3_Lever`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P1_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P2_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P3_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P1_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P2_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P3_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Entity`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Tutorial Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Tutorial Lever`
- **Components/assets/settings:** `EchoHaptics` clamps amplitude to 0–1 and duration to 0–1 second, checks `HapticCapabilities.supportsImpulse`, and uses `InputDevices.deviceConnected/deviceDisconnected` with cached `XRNode.LeftHand`/`RightHand` devices. Patterns are ping `0.3/0.15 s`, button `0.5/0.1 s`, lever `0.4/0.2 s`, and wall touch `0.2/0.05 s`. Entity defaults are 8 m maximum range, amplitude `0.08–0.55`, duration `0.08 s`, and cadence `0.45–0.12 s` from far to near.
- **Decisions and assumptions:** `SimpleControllerInteraction` is verified as a right-controller trigger path, so it passes `HapticHand.Right`. The four tracked-hand collider instances infer left/right from their verified `Left Hand Tracking`/`Right Hand Tracking` ancestry. Unknown future/custom hand hierarchies use both hands as the explicitly documented compatibility fallback. Entity proximity is intentionally bilateral because it is body-level danger rather than a hand-originated interaction. Wall feedback is limited to non-trigger colliders with `wall` in their inspected hierarchy names and is emitted only on trigger entry with an unscaled-time cooldown. Keyboard-only testing does not request controller haptics. Gameplay events, puzzle state, animations, and existing audio calls were preserved.
- **Verification:** Unity AssetDatabase refresh and C# compilation completed successfully. After clearing the Console, a fresh refresh produced zero Error entries. A focused Play Mode diagnostic observed logical requests of Ping Right `0.300/0.150`, Button Right `0.500/0.100`, Lever Left `0.400/0.200`, WallTouch Left `0.200/0.050`, and near Entity Both `0.530/0.080`. Two immediate Entity updates produced only one request, proving cadence limiting; moving out of range cleared the active haptic state; setting `Time.timeScale=0` also changed the Entity haptic state from active to inactive. Play Mode was stopped and `MainScene` remained clean.
- **Known limitations:** Actual tactile strength, cross-device support, and comfort require testing on the target Quest headset. Current accepted sonar input is right-controller based; if a future ping binding identifies another hand, that source should be passed through instead of the current verified right-hand mapping. Wall recognition intentionally follows the project's inspected `*Wall*` naming convention rather than vibrating on floors, doors, the Entity collider, or every solid surface.
- **Follow-up:** In headset, verify the four event patterns and Entity escalation, then tune wall/Entity values only if they feel intrusive. Confirm that opening pause/captured menus immediately silences Entity rumble on device.


### POLISH-04-001 — Persistent VR settings and turning controls

- **Date:** 2026-07-15
- **Goal:** Add a shared in-headset settings screen to the main and pause menus, persist comfort/locomotion preferences independently of UI lifetime, and make snap/smooth turning plus continuous-turn speed apply immediately.
- **Result:** Both menu controllers now expose the shared Settings screen and return to the screen that opened it. The pause menu stays at `Time.timeScale=0` and keeps environment audio paused while browsing Settings. Locomotion mode, turn mode, vignette preference, height offset, and turn speed persist under namespaced PlayerPrefs keys. The XR rig now enables exactly one turning provider at runtime, defaults to 45-degree snap turning, and applies the saved continuous speed immediately when smooth turning is selected. Vignette, height, and teleport preferences are exposed for POLISH-05/06/07 without implementing those behaviors here.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/UI/EchoRoomSettings.cs`
  - `Assets/_EchoRoom/Scripts/UI/EchoRoomSettings.cs.meta`
  - `Assets/_EchoRoom/Scripts/UI/VRSettingsPanelController.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRSettingsPanelController.cs.meta`
  - `Assets/_EchoRoom/Scripts/Controller/TurnSettingsController.cs`
  - `Assets/_EchoRoom/Scripts/Controller/TurnSettingsController.cs.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Resources/UI/VRMenu.uxml`
  - `Assets/_EchoRoom/Resources/UI/VRMenu.uss`
  - `Assets/_EchoRoom/Scripts/UI/VRMainMenu.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Locomotion System/Turn`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Locomotion System/Turn`
- **Components/assets/settings:** `MainScene :: VR Pause Menu` now references the same `Assets/_EchoRoom/Resources/UI/VRMenu.uxml` and `VRMenu.uss` used by the main menu. `TurnSettingsController` serializes the existing `ActionBasedSnapTurnProvider` and `ActionBasedContinuousTurnProvider` references on the Turn object. PlayerPrefs keys are `EchoRoom.Settings.LocomotionMode`, `TurnMode`, `VignetteEnabled`, `HeightOffsetMeters`, and `TurnSpeedDegreesPerSecond`; the same namespace reserves master/music/SFX/subtitle preferences for future UI. Defaults are Smooth locomotion, Snap turning, vignette enabled, zero height offset, and 60 degrees/second continuous turning; limits are 30–120 degrees/second and -0.30 to +0.30 metres. The snap angle remains 45 degrees. POLISH-01's `DynamicSprintController` values remain 2.0 m/s normal and 3.5 m/s sprint.
- **Decisions and assumptions:** Preserved 60 degrees/second as the continuous-turn default because that is the existing serialized rig value verified by POLISH-04; 45 is the separate snap-turn angle. Retained the rig's legacy action-based XRI providers and their existing input action references rather than migrating bindings during a settings task. Every UI and static settings-event subscription added here has a matching disposal/unsubscription path. Settings getters read PlayerPrefs directly, so gameplay consumers do not depend on a menu having been instantiated.
- **Verification:** Unity AssetDatabase refresh/recompile completed with `IsCompiling=False`, `EditorUtility.scriptCompilationFailed=False`, and no Console Errors in the verification window. MCP prefab and loaded-scene readback confirmed `TurnSettingsController` on the exact recorded Turn paths, both provider references assigned, snap angle 45, continuous speed 60, and movement 2.0/3.5 preserved. The imported shared UXML contains every required main, pause, settings, Back, mode, vignette, height, and speed element. In focused MainScene Play Mode, NavigationSubmit events exercised the same standard UI Toolkit Buttons used by controller pointers: Pause -> Settings kept time at 0 and audio paused; all five controls wrote exact namespaced PlayerPrefs values; Smooth enabled only continuous turning and applied 75 degrees/second immediately; Back returned to Pause without resuming; Continue restored time to 1 and audio. Runtime layout measured 900x560 with the 780x492 settings screen fully inside it. A second Play session read Teleport/Smooth/75/vignette-off/+0.05 m from PlayerPrefs before opening any settings UI, applied Smooth/75 to the providers, and populated the matching UI labels. Verification preferences were then restored to the documented defaults and the providers returned to Snap/60. Unity finished stopped, unpaused, not compiling, and not updating.
- **Known limitations:** No physical Quest/controller-ray or headset comfort test was performed. The main-menu object and shared UXML/button bindings were inspected through Unity and compiled, while the focused runtime navigation pass used the pause-menu instance in MainScene. Teleport locomotion, vignette rendering, and camera-height application intentionally remain for POLISH-05/06/07. The legacy XRI providers should eventually be migrated separately before their deprecated types are removed by a future XRI upgrade.
- **Follow-up:** In headset, open Settings from both main and pause menus with controller rays, confirm snap/smooth comfort and pointer targeting, and tune the 60 degrees/second default only if user testing supports a change. POLISH-05/06/07 should consume `EchoRoomSettings.LocomotionMode`, `VignetteEnabled`, and `HeightOffset` respectively.


### POLISH-02-001 — Distinct button and microphone ping profiles

- **Date:** 2026-07-15
- **Goal:** Give the sonar control and microphone shout explicit visual range, directional echo distance, and cooldown profiles while preserving tutorial source classification and POLISH-03 haptics.
- **Result:** Accepted sonar-control pings now reveal/cast to 10 m with a 2.5-second lockout; accepted microphone pings reveal/cast to 16 m with a 4-second lockout. Both inputs share one timer owned by the accepted ping, so neither can bypass the other's remaining cooldown. Each GPU pulse stores its own visual range beside its origin/start time, allowing simultaneous button and microphone pulses to retain independent limits. Microphone polling now receives the exact full or remaining shared wait duration.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/PingEmitter.cs`
  - `Assets/_EchoRoom/Scripts/MicPingTrigger.cs`
  - `Assets/_EchoRoom/Scripts/Controller/SonarRevealController.cs`
  - `Assets/_EchoRoom/Scripts/Diagnostics/AOPlaytestLogger.cs`
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab`
  - `Logs/AOPlaytests/AOPlaytest_latest.jsonl` (runtime-generated verification log; overwritten by the focused Play Mode run)
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/PingEmiiter`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/PingEmiiter` (inherits the prefab profiles; scene serialization unchanged)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/EchoPulseController` (runtime `SonarRevealController`; scene serialization unchanged)
  - `Runtime DontDestroyOnLoad scene (not a serialized asset) :: AO Playtest Logger`
- **Components/assets/settings:** `PingEmitter.basicPingProfile` serializes visual range `10`, echo cast distance `10`, and cooldown `2.5`; `microphonePingProfile` serializes `16`, `16`, and `4`. `SonarRevealController` writes matching per-slot `_SonarPulses` and `_SonarPulseRanges` global vector arrays with 16 slots. `EchoRoom/EchoSonarReveal` uses each positive per-pulse range for lifetime, distance cutoff, reveal falloff, and ring lifetime; non-profile callers retain `_RevealRadius` as a legacy fallback. `AOPlaytestLogger` now reconstructs and logs the matching per-pulse range. The accepted-emission path still calls `EchoHaptics.PlayPing(HapticHand.Right)` using POLISH-03's `0.3` amplitude and `0.15 s` duration.
- **Decisions and assumptions:** Kept `OnPingEmitted` as the existing origin-only event so TutorialDirector, Entity AI, and other subscribers remain compatible. `PingEmitter` writes the range-aware pulse before raising that event; the existing same-frame/origin deduplication prevents `SonarRevealController`'s legacy event subscription from replacing its profile. Removed the previous implicit cooldown dependency on echo travel time and clip length because the profiles now define cooldown explicitly. `LastPingInputSource` is still assigned only for accepted pings; controller and Editor Space use `SonarControl`, while `RequestPing` uses `Microphone`.
- **Verification:** Unity synchronous refresh and C# compilation completed with the Editor idle afterward and zero Console Error entries. MCP prefab readback and loaded-`MainScene` readback both reported exact `10/10/2.5` and `16/16/4` profile values. Saving the XR rig preserved POLISH-01 movement `2.0/3.5` and POLISH-04 `TurnSettingsController` references. Shader inspection reported `IsSupported=True`, `HasErrors=False`, and zero compilation messages. A focused Play Mode diagnostic produced `[POLISH02_CHECK] pass=True`: button range `10.00`, cooldown `2.500`, one active pulse; microphone range `16.00`, cooldown `4.000`, one active pulse; button-to-mic and mic-to-button lockouts both held; repeated mic returned `4.000`; final accepted source remained `Microphone` after a blocked button attempt; exactly two accepted right-hand ping haptic requests were observed and blocked attempts added none. TutorialDirector's first-step check still requires `LastPingInputSource == SonarControl`. Play Mode was stopped and Unity finished idle with no Console Errors.
- **Known limitations:** The focused test invoked the same runtime request/emission paths but did not generate a physical acoustic microphone signal or visually measure the ring in a Quest headset. Actual echo audibility at 10/16 m, tactile feedback, reveal appearance, and comfort remain device checks. Per-material `_RevealRadius` now acts only as the fallback for legacy non-profile pulse calls; explicit button/microphone pulses use their profile ranges by design.
- **Follow-up:** In headset, compare button and microphone pings in the same corridor, confirm the visual wave ends near 10/16 m, verify the directional echo distance and 2.5/4-second cadence by feel, and confirm the tutorial still rejects microphone input for its first sonar-control instruction.


### POLISH-05-001 — Persistent floor-safe teleport locomotion

- **Date:** 2026-07-15
- **Goal:** Implement the persisted Teleport locomotion choice from POLISH-04 as a functional XRI 3.3.1 alternative to smooth movement, with deliberate destinations across every shipped maze and the tutorial.
- **Result:** The XR rig now has left- and right-controller projectile teleport arcs, a modern `TeleportationProvider`, and a mode controller that applies `EchoRoomSettings.LocomotionMode` on startup and immediately after settings changes. Smooth mode enables the existing move/sprint stack and hides/disables teleport input and visuals. Teleport mode disables the smooth move provider, sprint controller, and Move actions while enabling Teleport Mode actions and the teleport provider. Holding either thumbstick forward shows its arc; releasing queues a valid floor teleport and hides the arc. Only the explicit maze, hallway, and tutorial floor objects accept teleport. Destination validation rejects non-floor colliders, slopes over 20 degrees, blocked body capsules, and floor points hidden behind maze walls, preventing projectile arcs from bypassing corridor walls.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Controller/TeleportLocomotionController.cs`
  - `Assets/_EchoRoom/Scripts/Controller/TeleportLocomotionController.cs.meta`
  - `Assets/_EchoRoom/Scripts/Controller/EchoTeleportationProvider.cs`
  - `Assets/_EchoRoom/Scripts/Controller/EchoTeleportationProvider.cs.meta`
  - `Assets/_EchoRoom/Scripts/Controller/EchoTeleportationArea.cs`
  - `Assets/_EchoRoom/Scripts/Controller/EchoTeleportationArea.cs.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)`
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Locomotion System/Teleportation`
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Camera Offset/Left Controller/Teleport Interactor`
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Camera Offset/Right Controller/Teleport Interactor`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Locomotion System/Teleportation`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Teleport Interactor`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Teleport Interactor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Exit_Hallway/Hallway_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Exit_Hallway/Hallway_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Exit_Hallway/Hallway_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Exit_Hallway/Hallway_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Exit_Hallway/Hallway_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Floor`
- **Components/assets/settings:** The rig root now serializes one `XRBodyTransformer` and one `LocomotionMediator`; `MainScene` inherits those components after its two obsolete added-component overrides were removed. `Locomotion System/Teleportation` contains `EchoTeleportationProvider` at transformation priority 20 with zero delay and `TeleportLocomotionController`. The two nested `Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/Prefabs/Interactors/Teleport Interactor.prefab` instances use Projectile Curve, 14 m maximum ray distance, velocity 10, sample frequency 50, Directional/Blocking Teleport Reticles, physical raycast mask `4294967291` (all except Ignore Raycast), and interaction layer bit `2147483648` (layer 31, Teleport). They reference `XRI Left Locomotion/Teleport Mode` and `XRI Right Locomotion/Teleport Mode`; the controller also owns the matching left/right Move action state. `EchoTeleportationArea` uses interaction layer 31, `MatchOrientation.None`, OnSelectExited, 20-degree hit-normal/slope limits, a 0.30 m radius by 1.70 m clearance capsule, and the same obstruction mask. The XRI interactor prefab's extra `SimpleHapticFeedback` is disabled so it does not duplicate the project's POLISH-03 haptic service.
- **Decisions and assumptions:** Both hands support teleport because XRI's shipped input asset already exposes equivalent left/right Teleport Mode controls. Turn actions/providers remain independent and enabled according to `TurnSettingsController`; only smooth Move actions/providers are exchanged for teleport. Direct head-to-destination line of sight intentionally prevents teleporting around or over maze walls even when the projectile arc could physically reach the floor behind them. Only the eleven inspected floor objects were made teleportable; roofs, walls, doors, props, trigger volumes, the Entity, and exterior space were left non-teleportable. The existing scene-only `Menu UI Ray` objects were not reused or modified. The XRI input-action asset was referenced but not changed. The POLISH-01 movement values `2.0/3.5`, POLISH-02 ping profiles `10/10/2.5` and `16/16/4`, POLISH-03 haptics, POLISH-04 turn controller/settings, and menu wiring were preserved.
- **Verification:** Unity AssetDatabase refresh and C# compilation completed with the Editor idle and zero fresh Console Errors. Live MCP prefab readback found zero missing scripts, exactly one body transformer/mediator, assigned provider/controller/action/interactor references, exact interaction/raycast masks, both arcs initially inactive, and exactly two floor areas in each Maze A-E plus one tutorial floor area; every area had a collider, correct Teleport mask, and no area existed on a non-floor object. `MainScene` readback confirmed one inherited body transformer/mediator, no duplicate added overrides, provider/controller/rays present, zero missing scripts, and a clean scene. Focused Play Mode checks proved Smooth and Teleport apply exact inverse provider/action states, four repeated mode changes do not duplicate or stick, and a right-hand aim becomes visible then hides after release. The active Maze produced two runtime floor areas; a clear floor request succeeded, a wall hit failed, and a floor point behind `Maze_Walls` failed line-of-sight validation. Queueing the valid request moved the camera XZ to the destination with `0.000 m` measured error, left the CharacterController enabled at height `1.60` and radius `0.30`, and returned the provider to Idle. A full Play Mode stop/restart loaded persisted Teleport before settings UI creation and applied it correctly; verification then restored Smooth and removed diagnostic PlayerPrefs. The second focused run had zero Console Errors. Unity finished stopped, unpaused, not compiling/updating, with all prefabs closed.
- **Known limitations:** Physical thumbstick input, arc appearance, reticle readability, destination feel, and comfort were not tested on a Quest headset. Static live-Editor validation covered every shipped Maze A-E and Tutorial asset, while the focused runtime request test used the currently loaded Maze A. Direct line-of-sight is intentionally conservative and disallows teleporting around corners; headset testing should confirm that this feels appropriate rather than overly restrictive.
- **Follow-up:** On Quest, test both controller sticks in each maze and the tutorial, confirm the arc/reticles are comfortable and every corridor has enough reachable floor, verify walls and closed doors cannot be bypassed, and tune the 14 m arc or clearance values only from observed headset evidence.


### POLISH-06-001 — Actual-motion comfort vignette

- **Date:** 2026-07-15
- **Goal:** Add the persisted optional tunneling vignette to artificial smooth movement and continuous turning without obscuring stationary play or activating during snap turns and teleport locomotion.
- **Result:** The XRI 3.3.1 Starter Assets tunneling-vignette prefab is now nested directly under the XR rig's Main Camera. A new `ComfortVignetteController` measures actual XR Origin planar translation and rotation, requests XRI's vignette only for saved Smooth locomotion and Smooth turning, releases it when motion stops, suppresses it while paused, and excludes snap-turn and Teleport modes. The renderer follows `EchoRoomSettings.VignetteEnabled` immediately and at startup; turning the setting off disables the overlay renderer immediately.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Controller/ComfortVignetteController.cs`
  - `Assets/_EchoRoom/Scripts/Controller/ComfortVignetteController.cs.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab`
  - `Logs/AOPlaytests/AOPlaytest_latest.jsonl` (ignored runtime-generated verification log; overwritten by the focused Play Mode sessions)
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Camera Offset/Main Camera/Comfort Vignette`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Main Camera/Comfort Vignette` (inherits the XR rig prefab change; scene serialization unchanged)
- **Components/assets/settings:** Reused the nested source `Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/TunnelingVignette/TunnelingVignette.prefab` with its `TunnelingVignetteController`, hemisphere mesh, material, `VR/TunnelingVignette` shader, MeshRenderer, and SortingGroup order 30010. `ComfortVignetteController` implements `ITunnelingVignetteProvider`, references the rig root as its measured motion Transform, and uses aperture `0.72`, feathering `0.22`, ease-in `0.18 s`, and ease-out `0.22 s`. Actual-motion thresholds are `0.05 m/s` planar movement and `3 degrees/s` rotation, with per-frame discontinuity guards of `1 m` and `40 degrees` plus a `0.08 s` release hold. `EchoRoomSettings.VignetteEnabled`, `LocomotionMode`, and `TurnMode` changes apply through the existing settings event.
- **Decisions and assumptions:** Measured XR Origin motion instead of provider enabled/state because the continuous move provider can remain active for zero-input/gravity housekeeping; physical tracked-head motion changes the camera pose rather than the XR Origin pose and therefore does not trigger the effect. Locomotion and turn settings explicitly exclude Teleport and Snap even if the rig Transform changes. Kept the sample as a nested prefab so its maintained XRI mesh/material/shader remain authoritative. Disabling the renderer on setting-off provides an immediate no-vignette guarantee while retaining the XRI controller's normal easing lifecycle when motion ends.
- **Verification:** Unity compiled the new controller with the Editor idle afterward. Live MCP readback confirmed exactly one prefab and one loaded-scene driver at the recorded paths, assigned controller/renderer/root references, local position/rotation zero, unit scale, no missing scripts, supported vignette shader with zero compiler messages, and no duplicate overlay. Preserved readback reported movement `2.0/3.5`, button/microphone ping profiles `10/10/2.5` and `16/16/4`, one turn controller, one teleport mode controller/provider, two teleport rays, and all eleven teleport areas. Focused Play Mode produced `[POLISH06_CHECK] pass=True`: stationary aperture `1`, actual smooth movement and smooth turning requested/closed the vignette, stopping reopened it, snap and teleport Transform changes were excluded, setting-off disabled the renderer immediately, pause ended and fully reopened the effect, and repeated lifecycle changes retained one XRI provider record. A full Play Mode restart loaded persisted vignette-off before UI interaction and kept the renderer off; verification then restored Smooth/Snap/Vignette On. Unity finished stopped, unpaused, not compiling/updating, MainScene clean, all prefab stages closed, and a final Console Error query returned no entries.
- **Known limitations:** Stereo correctness, both-eye edge coverage, controller-pointer/hand/UI readability, final comfort, and threshold tuning require a Quest headset test. Editor automation changed the rig Transform to exercise real-motion detection but did not use physical controller input or a live OpenXR headset. Discontinuous runtime repositioning smaller than the configured guards could be interpreted as smooth motion when the saved mode is Smooth.
- **Follow-up:** On Quest, test walking, sprinting, and smooth turning at several speeds; confirm the vignette is comfortable in both eyes, central UI/hands/pointers remain readable, snap/teleport never tunnel, and the setting switches cleanly from both menus. Tune the serialized comfort profile or thresholds only from headset evidence.


### POLISH-07-001 — Deterministic seated height offset

- **Date:** 2026-07-15
- **Goal:** Apply the persisted seated/standing height preference without moving the tracked Camera or XR Origin locomotion root, and remain correct when XROrigin changes tracking-origin baselines.
- **Result:** The XR rig now has one `HeightOffsetController` on its root. It captures the authoritative Camera Offset baseline, applies `EchoRoomSettings.HeightOffset` as an absolute relative value, clamps the runtime safety envelope to -0.5..+0.5 m (the shared UI setting remains the stricter -0.3..+0.3 m), and never accumulates previous applications. It subscribes to height-setting, scene-load, XR tracking-origin, and subsystem-reload changes; Floor rebases to 0, Device/Unbounded rebases to `XROrigin.CameraYOffset`, and Unknown/no-XR mode safely recognizes an external raw baseline reset before reapplying on the following frame.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Controller/HeightOffsetController.cs`
  - `Assets/_EchoRoom/Scripts/Controller/HeightOffsetController.cs.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab`
  - `Logs/AOPlaytests/AOPlaytest_latest.jsonl` (ignored runtime-generated verification log; overwritten by the focused Play Mode sessions)
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)`
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Camera Offset`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)` (inherits the root controller; scene serialization unchanged)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset` (runtime height target; scene serialization unchanged)
- **Components/assets/settings:** `HeightOffsetController` requires the existing `Unity.XR.CoreUtils.XROrigin` and resolves its assigned `CameraFloorOffsetObject`; no serialized Camera or root Transform reference is introduced. The rig retains `RequestedTrackingOriginMode=NotSpecified`, `CameraYOffset=1.361`, serialized Camera Offset local Y `1.0`, and root local position zero. `EchoRoomSettings.HeightOffset` remains persisted by POLISH-04 under `EchoRoom.Settings.HeightOffsetMeters` with UI limits -0.30..+0.30 m and 0.05 m steps.
- **Decisions and assumptions:** Only the Camera Offset local Y is written, preserving its X/Z coordinates. The tracked Main Camera is never written and the locomotion root is never repositioned. XROrigin's own tracking-origin result is treated as the baseline rather than altering `CameraYOffset`. A next-frame reapply is used after tracking-origin callbacks so XROrigin's baseline write completes first regardless of callback order. Scene-load handling also covers a persistent rig; a newly created rig captures its own baseline during `Awake`.
- **Verification:** Unity synchronous refresh and compilation completed with the Editor idle and zero fresh Console Errors. MCP readback found exactly one prefab controller on the rig root and one inherited loaded-scene controller; Camera Offset remained `(0,1,0)`, Main Camera remained its child with unchanged local pose, XR Origin root remained local zero, and MainScene remained clean. Focused Play Mode checks passed absolute +0.30, -0.30, +0.15, and 0 targets; 25 repeated applications at each value produced no drift. The root and Camera local pose stayed fixed; left/right hand roots, right controller, `Maze Timer Display`, and `Menu UI Ray` retained local alignment and followed Camera Offset by the exact preference delta; CharacterController Transform, height, and center remained stable. Simulating an external XROrigin-style raw reset from baseline 1.000 to 1.361 while +0.20 was saved rebased on the next frame to baseline 1.361 and target/actual 1.561. A full Play Mode stop/restart loaded persisted +0.20 at baseline 1.000/actual 1.200, after which the setting was restored and persisted to 0. Unity finished stopped, unpaused, idle, MainScene clean, and with zero Console Errors.
- **Known limitations:** The Editor had no active XR headset, so `CurrentTrackingOriginMode` was `Unknown`; Floor/Device/Unbounded baseline selection is covered by direct XROrigin API behavior and the external-reset runtime check, but an actual Quest tracking-origin transition was not generated. Seated reach, physical floor relationship, stereo viewpoint comfort, and real controller/hand interaction reach require a headset/manual check.
- **Follow-up:** On Quest, test at -0.30/0/+0.30 in seated and standing use, recenter or change tracking origin while a non-zero offset is active, and confirm floor relationship, hand/controller reach, menus, timer display, teleport, and CharacterController behavior remain comfortable and aligned.


### POLISH-09-001 — Surface- and size-aware directional echoes

- **Date:** 2026-07-15
- **Goal:** Give directional echoes restrained material and object-size character while preserving the stable ping profiles, default untagged sound, spatial rolloff, and haptic path.
- **Result:** Directional ray hits now resolve optional `EchoSurface` metadata before the distance delay and pass bounded pitch and volume multipliers to one explicit `EchoSoundController.ConfigureAndPlay` call. The controller captures the echo prefab's authored base volume, disables legacy Play On Awake at runtime, applies the multiplier before playback, rejects duplicate play requests, preserves 3D logarithmic rolloff, and owns the one normal playback/destruction lifecycle. Untagged surfaces remain exactly pitch 1 and volume multiplier 1. Representative Maze C iron, stone/masonry, and wood geometry is tagged Metal, Concrete, and Wood without expanding the mapping across every maze.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Sonar.meta`
  - `Assets/_EchoRoom/Scripts/Sonar/EchoSurface.cs`
  - `Assets/_EchoRoom/Scripts/Sonar/EchoSurface.cs.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/PingEmitter.cs`
  - `Assets/_EchoRoom/Scripts/Controller/EchoSoundController.cs`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Frame`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/EchoSound.prefab :: EchoSound` (runtime behavior changes through `EchoSoundController`; prefab serialization unchanged)
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/PingEmiiter` (runtime directional-echo path changes through `PingEmitter`; rig serialization unchanged)
- **Components/assets/settings:** `EchoSurfaceType` supports Default, Metal, Concrete, Wood, Glass, and FabricAbsorptive. Built-in 2 m pitch/volume multipliers are Metal `1.08/0.96`, Concrete `0.97/0.86`, Wood `0.92/0.78`, Glass `1.12/0.72`, and FabricAbsorptive `0.88/0.58`; pitch clamps to `0.85..1.15` and volume multiplier to `0.55..1.15`. Tagged surfaces derive a logarithmic loudness contribution from the hit collider's largest world-space bound, with renderer fallback, normalized at 2 m and limited to `-5%/+8%`. Untagged or explicitly Default surfaces skip size scaling and return exact `1/1`. Maze C readback resolved Door_Frame `1.08/0.97`, Maze_Walls `0.97/0.93`, and Door_Leaf `0.92/0.78`. The EchoSound prefab retains its authored clip, base volume 1, min distance 3 m, max distance 30 m, and logarithmic rolloff.
- **Decisions and assumptions:** Applied size contribution only to deliberately tagged surfaces so untagged content has no regression. Tagged volume profiles attenuate from the unchanged full-volume default, leaving headroom for the conservative size contribution instead of saturating immediately at AudioSource volume 1. Maze C was chosen because its imported materials identify `Door_FrameIron`, `Maze_Stone`, and `Door_Wood`; stone is used as the representative concrete/masonry acoustic class. Pitch/volume trims remain 1 on all three representative objects. No XR rig, scene, other maze, teleport, or door-audio serialization was changed.
- **Verification:** Unity synchronous refresh and compilation completed with `EditorUtility.scriptCompilationFailed=False`; the final Editor state was stopped, unpaused, not compiling, and not updating. Live prefab-stage inspection verified the three exact Maze C object paths and bounds before adding one `EchoSurface` to each; the prefab was saved and closed. Asset readback found exactly three surface tags, exactly two existing `EchoTeleportationArea` components, and retained the Door AudioSource plus assigned `Door.openDoor`/`Door.closeDoor` clips. Focused Play Mode verified an untagged 40 m collider remained exact `1/1`; Metal/Concrete/Wood responses were distinct; Metal's size response increased from `0.91` at 0.1 m to `1.04` at 40 m; a spawned EchoSound was idle before configuration, the first configuration played, a duplicate returned false, spatial blend/logarithmic 3/30 m rolloff remained, and a shortened diagnostic lifetime self-destroyed. XR rig readback preserved button `10/10/2.5`, microphone `16/16/4`, and the accepted right-hand haptic call. The final code audit found one `audioSource.Play()` call in `EchoSoundController` and no direct echo AudioSource playback in `PingEmitter`. A final post-compile gate had zero Console Errors in its fresh one-minute window. Two earlier diagnostic assertions failed because the harness incorrectly expected the intentionally null Door AudioSource clip instead of the Door open/close fields, and compared unclamped volume `1.05` to AudioSource's clamped `1.0`; corrected assertions passed and neither was a product defect. Unity's MCP Console-clear helper could not clear its own locked log-cache file, so final error verification used a fresh timestamp window after clearing the Editor Console directly.
- **Known limitations:** Surface intelligibility, echo loudness, spatial localization, and whether the differences remain comfortably non-cartoonish require Quest/headset listening. Only three representative Maze C objects are tagged; Glass and FabricAbsorptive mappings are implemented but not yet assigned to shipped geometry. Large responses can still clamp at AudioSource volume 1 after multiplying a high prefab base volume. Unity-generated prefab YAML retains standard blank `m_Name` trailing spaces reported by `git diff --check`; source files had no whitespace errors.
- **Follow-up:** In headset, compare the same directional ping against Maze C Door_Frame, Maze_Walls, Door_Leaf, and an untagged surface at similar distances. Tune built-in multipliers only from listening evidence, then deliberately tag a small number of proven Glass/Fabric surfaces before considering broader maze authoring.


### POLISH-02-PTT-001 — Make microphone sonar push-to-talk

- **Date:** 2026-07-15
- **Goal:** Replace the always-listening microphone sonar path with explicit push-to-talk behavior.
- **Result:** Microphone sonar now captures only while the left controller secondary face button (Y on Quest) is held. Editor testing uses V. Releasing the control, pausing gameplay, disabling the component, or leaving its lifecycle stops `Microphone` capture. A loud threshold crossing can request at most one microphone ping per hold, so continuous speech cannot repeatedly fire after cooldown; the player must release and hold again. The existing 16 m / 4 s microphone profile, shared ping lockout, tutorial source classification, sound, and haptic emission path are unchanged.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/MicPingTrigger.cs`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab`
  - `Docs/PolishTasks/POLISH-02-PING-PROFILES.md`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/PingEmiiter` (runtime `MicPingTrigger` behavior changes through its script; prefab serialization unchanged)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Custom Objects Scripts/PingEmiiter` (inherits the XR rig behavior; scene serialization unchanged)
  - `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Microphone Wall Tip`
- **Components/assets/settings:** `MicPingTrigger` owns a runtime `InputAction` bound to `<XRController>{LeftHand}/secondaryButton` plus Editor-only `<Keyboard>/v`. Capture still uses the first available microphone at 44.1 kHz with a one-second looping clip, a 128-sample peak window, serialized sensitivity `0.1`, and serialized polling interval `0.1 s`. Public read-only state exposes whether push-to-talk is held and whether the microphone is actually recording. The tutorial tip now reads, `Hold Y and speak to use a stronger microphone ping.`
- **Decisions and assumptions:** Left Y was selected after a project-wide input audit found right secondary reserved for normal sonar, right primary reserved for the maze timer, trigger/grip reserved for interactions, and sticks reserved for movement/turn/teleport. Starting and ending `Microphone` capture on each hold was chosen over merely ignoring an always-running stream so the behavior is genuinely push-to-talk. A threshold crossing during another ping's lockout consumes that hold and requires release/re-hold; this makes each physical hold one deliberate attempt. Existing concurrent teleport serialization in the tutorial prefab was preserved untouched; this change edits only the microphone instructional text there.
- **Verification:** Project-wide binding search found no gameplay use of left secondary/Y. Source audit confirmed capture begins only on the held edge, ends on release/pause/disable, uses the named device consistently for position/end calls, and permits one threshold attempt per hold. `git diff --check` found no new C# or documentation whitespace issue; the tutorial prefab retains one pre-existing Unity-generated blank `m_Name` trailing-space warning. Unity's generated Roslyn response compiled the entire game `Assembly-CSharp` successfully with exit code 0 and only existing obsolete/unused-field warnings after excluding six missing generated Ivan Murzak MCP reference assemblies. Not inspected — Unity connection unavailable: the running Editor's MCP transport could not start because `com.ivanmurzak.unity.mcp` 0.84.0 currently fails in `UnityMcpPlugin.Config.cs(148,50)` with CS0115 (`CredentialProvider` has no suitable method to override).
- **Known limitations:** Full Unity refresh, live component readback, Play Mode microphone simulation, Android microphone-permission behavior, and physical Quest Y/microphone testing remain pending because the unrelated MCP package compile error blocks the normal Editor/MCP verification loop. Starting microphone hardware on press can introduce device-specific startup latency; headset testing must confirm speech is not clipped.
- **Follow-up:** Repair or align the Ivan Murzak MCP package and its bundled `McpPlugin` dependency, then refresh Unity and run a focused Play Mode/device pass: verify idle capture is off, hold Y starts capture, speech emits one 16 m ping, sustained speech cannot repeat, release ends capture, re-hold can emit again after shared cooldown, and pause immediately ends recording.


### MCP-COMPAT-001 — Restore compatible Unity-MCP dependency family

- **Date:** 2026-07-15
- **Goal:** Resolve CS0115 in `UnityMcpPlugin.Config.cs` and restore the Unity Editor/MCP connection without altering gameplay packages or code.
- **Result:** Replaced the incompatible direct Unity-MCP core 0.84.0 pin with 0.82.1, matching the exact core dependency declared by the installed Animation 1.2.21, Cinemachine 1.0.7, and Input System 1.0.7 extensions. Unity Package Manager resolved the compatible package, regenerated the MCP assemblies, and restored the NuGet `McpPlugin`/`McpPlugin.Common` binaries from the mismatched 6.11.0 state to the repository-compatible 6.10.0 state. The `CredentialProvider` override compile error is gone and the local MCP endpoint is reachable again.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Packages/manifest.json`
  - `Packages/packages-lock.json`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:** None — package resolution and editor tooling only.
- **Components/assets/settings:** `com.ivanmurzak.unity.mcp` is now a direct registry dependency at 0.82.1 with lock depth 0. Existing extension versions remain Animation 1.2.21, Cinemachine 1.0.7, and Input System 1.0.7. Unity resolution restored `Assets/Plugins/NuGet/McpPlugin.dll` and `McpPlugin.Common.dll` to file version 6.10.0; those files and `.nuget-installed.json` match the repository afterward and therefore have no final diff.
- **Decisions and assumptions:** Chose the version required by all three installed MCP extensions instead of editing generated `Library/PackageCache` source or manually replacing DLLs. This keeps UPM authoritative and avoids mixing the 0.84.0 source API, which expects McpPlugin 7.x `CredentialProvider`, with older precompiled binaries. No other package version or dependency entry was changed.
- **Verification:** Unity resolved `com.ivanmurzak.unity.mcp@0.82.1` into a new package-cache directory, rebuilt `com.IvanMurzak.Unity.MCP.Runtime.dll`, rebuilt `Assembly-CSharp.dll`, and the current Editor log contains no fresh CS compiler error, Tundra failure, or script-compilation failure after resolution. Live MCP package-list readback reports core 0.82.1 and the three unchanged extension versions. `unity-mcp-cli status` reports the running Editor and local server connected, and the system readiness probe returns `pong`. The original CS0115 no longer appears in the fresh post-resolution compilation output.
- **Known limitations:** `console-clear-logs` still cannot clear `Temp/mcp-server/ai-editor-logs.txt` while the server holds the file; invoking it logged a tool-specific file-lock error but did not affect compilation or connectivity. The Meta XR SDK package also reports two immutable-package missing-`.meta` import messages independently of this fix.
- **Follow-up:** No MCP compatibility work is required unless the extension family is upgraded together. If upgrading to Unity-MCP 0.84.x later, update its required McpPlugin dependency family atomically and verify all extension packages declare a compatible core version before opening Unity.


### MCP-COMPAT-002 — Correct MCP-COMPAT-001 and complete the 0.84 dependency bootstrap

- **Date:** 2026-07-15
- **Correction:** `MCP-COMPAT-001` records a successful intermediate 0.82.1 rollback, but that was not the durable final state. The managed Unity-MCP server restored the direct core dependency to 0.84.0, revealing that the correct repair was to complete 0.84.0's NuGet bootstrap rather than hold the older package. The final authoritative state is recorded here.
- **Goal:** Keep Unity-MCP 0.84.0 while installing its required precompiled dependency family and making the local server configuration restart-safe.
- **Result:** Reopened Unity through the official `unity-mcp-cli open` bootstrap path, which dismissed the expected launch compiler dialog once so the package's editor dependency resolver could run. The resolver upgraded `McpPlugin` and `McpPlugin.Common` from 6.x to 7.0.0 and `ReflectorNet` from 5.3.1 to 5.3.2. This supplies the `ConnectionConfig.CredentialProvider` API required by Unity-MCP 0.84.0, eliminating CS0115. The saved local auth option was migrated from legacy `required` to supported local `none`; a subsequent ordinary restart without an auth override connected successfully.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Packages/manifest.json`
  - `Packages/packages-lock.json`
  - `Assets/Plugins/NuGet/.nuget-installed.json`
  - `Assets/Plugins/NuGet/McpPlugin.dll`
  - `Assets/Plugins/NuGet/McpPlugin.Common.dll`
  - `Assets/Plugins/NuGet/ReflectorNet.dll`
  - `UserSettings/AI-Game-Developer-Config.json` (ignored local configuration; only `authOption` changed)
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:** None — editor tooling, package dependencies, and ignored local connection configuration only.
- **Components/assets/settings:** Direct registry package `com.ivanmurzak.unity.mcp` is 0.84.0 at lock depth 0. Existing MCP extensions remain Animation 1.2.21, Cinemachine 1.0.7, and Input System 1.0.7. NuGet manifest versions are `com.IvanMurzak.McpPlugin` 7.0.0, `com.IvanMurzak.McpPlugin.Common` 7.0.0, and `com.IvanMurzak.ReflectorNet` 5.3.2; DLL file versions match. Local connection mode remains Custom on the existing localhost endpoint with keep-connected/server behavior preserved, while auth is now `none` as supported by server 9.0.0. No token or credential value is recorded here.
- **Decisions and assumptions:** Followed the package's official CLI bootstrap behavior instead of editing generated `Library/PackageCache` source or downloading individual DLLs manually. The temporary 0.82.1 rollback was useful to restore tooling, but the managed server owns the 0.84 package family and rewrote it; the final fix therefore aligns all required binaries with 0.84.0. Local `none` auth is appropriate for this loopback-only Custom endpoint; no remote or cloud endpoint was placed in scope.
- **Verification:** Unity exited cleanly and reopened through the supported CLI bootstrap, after which `McpPlugin.dll` reported file version 7.0.0.0 and the CS0115 error disappeared. Live MCP package-list readback reports core 0.84.0 and unchanged extension versions. The readiness probe returned `pong`. Unity was then closed and reopened normally without passing an auth override; `wait-for-ready` connected immediately to the existing local endpoint. A fresh two-minute MCP Console Error query after that durable restart returned an empty result. `git diff --check` reports no new whitespace errors in the tracked text files.
- **Known limitations:** The package's `console-clear-logs` helper still has a known file-lock failure while its own log file is open; verification used a fresh Editor process/log window instead. Physical Quest testing remains unrelated and pending as documented in the polish entries.
- **Follow-up:** Upgrade the Unity-MCP core, MCP server, NuGet binaries, and extension compatibility as one tested family in future. Do not downgrade or replace only `McpPlugin.dll`, and do not edit `Library/PackageCache` directly.


### UI-MOCK-001 — Rebuild shared VR menus from the sonar-facility mock

- **Date:** 2026-07-15
- **Goal:** Translate the supplied `mock menu.jpg` composition into the project's existing Unity UI Toolkit menu while preserving all current screens, element names, settings controls, and button callbacks.
- **Result:** The shared menu now presents a dark sonar-facility background with concentric echo rings, a centered double-line signal frame, corner telemetry marks, wide-spaced Echo Room typography, and a vertical main-menu stack matching the mock's hierarchy. New Game has the brighter default signal state; hover/focus, pressed, disabled, secondary, and danger states remain native UI Toolkit styling. Level select, overwrite warning, pause, settings, and captured screens were restyled to the same frame and spacing system. The main menu and settings screen both fit within the existing 900×560 world-space document without changing the runtime C# bindings.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Resources/UI/VRMenu.uxml`
  - `Assets/_EchoRoom/Resources/UI/VRMenu.uss`
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu` (runtime presentation changes through the shared `VRMenu` UXML/USS; scene serialization unchanged)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu` (runtime presentation changes through the same shared `VRMenu` UXML/USS; scene serialization unchanged)
- **Components/assets/settings:** Both menu objects retain their existing `UIDocument`/world-space setup and 900×560 layout size. `VRMenu.uss` references the user-supplied `Assets/_EchoRoom/Art/UI/width1680.jpg` as the root background. The mock source `Assets/_EchoRoom/Art/UI/mock menu.jpg` guided the composition. Existing query names such as `new-game-button`, `load-game-button`, `tutorial-replay-button`, `main-settings-button`, the level buttons, pause/captured actions, and every settings control were preserved for `VRMainMenu`, `VRPauseMenu`, `VRFrontEndMenu`, and `VRSettingsPanelController`.
- **Decisions and assumptions:** Recreated panels, borders, buttons, labels, and interaction states in UXML/USS instead of slicing them into raster assets, keeping text readable and controls responsive in VR. The supplied selected/unselected button JPGs were not used because their apparent checkerboard transparency is baked into the pixel data and would render as pale rectangular backgrounds. No C# script, scene, prefab, collider, panel size, navigation callback, or save/settings behavior was changed. During visual verification only, the MainMenuScene EventSystem was temporarily disabled in memory to prevent a retained XR simulator trigger from immediately activating New Game; it was restored to active afterward and the scene remained clean and unsaved.
- **Verification:** PowerShell XML parsing accepted the complete UXML, a required-name audit found every existing script-bound element, and `git diff --check` reported no whitespace errors. Unity `ForceSynchronousImport` refreshed the assets successfully with no import/compiler error. Live 1920×1080 Game View captures verified the runtime main screen and settings screen: the main title/tagline, vertical four-button layout, selected state, sonar frame, all five settings rows, steppers, values, and Back button render without clipping. Live scene inspection confirmed the exact `Main Menu` and `VR Pause Menu` object paths; MainMenuScene finished stopped, unpaused, clean, with EventSystem active. Two MCP diagnostic errors were generated while restoring the temporarily inactive EventSystem by name before accounting for inactive-object lookup; the corrected inclusive lookup succeeded and these were tooling-only, not product/runtime faults. A fresh final one-minute Unity Console Error query returned no entries.
- **Known limitations:** The corridor artwork is intentionally extremely dark and subtle at the current world-space size; its contrast and text comfort still require a Quest headset check. The main and settings screens were visually captured; the remaining screens share the verified frame/button system and retained bindings but were not each captured separately. The UI uses the project's current default runtime font because no shippable matching condensed font asset was supplied.
- **Follow-up:** Check the menu at normal Quest viewing distance, especially the small controller hint and settings labels. If the corridor disappears in the headset, raise only the background image exposure or reduce the dim overlay rather than brightening the core panel. Add a licensed condensed font asset later if closer typography matching is desired.

### LIGHT-PREFAB-001 — Runtime baked lighting for prefab levels

- **Date:** 2026-07-14
- **Goal:** Create a Unity workflow that bakes a level prefab instance in MainScene, preserves its generated lightmaps in a standalone per-level asset, and restores that lighting when GameManager loads the prefab at runtime.
- **Result:** Added Tools > Echo Room > Prefab Baked Lighting. The window can bake the active scene and capture the selected level, or capture an existing bake. It copies color, directional, and shadow-mask textures into a level-specific folder; records each baked renderer using a sibling-index hierarchy path, component index, lightmap index, and scale/offset; creates or updates a LevelLightingData asset; and automatically assigns it to the matching LevelData entry. GameManager now preserves persistent scene lightmaps, appends the loaded level's captured maps, applies the recorded renderer bindings, and restores scene lighting before level swaps and destruction. The window can clear the temporary scene bake after all level captures.
- **Files created:**
  - `Assets/_EchoRoom/Scripts/Lighting.meta`
  - `Assets/_EchoRoom/Scripts/Lighting/LevelLightingData.cs`
  - `Assets/_EchoRoom/Scripts/Lighting/LevelLightingData.cs.meta`
  - `Assets/_EchoRoom/Scripts/Lighting/PrefabLightmapRuntime.cs`
  - `Assets/_EchoRoom/Scripts/Lighting/PrefabLightmapRuntime.cs.meta`
  - `Assets/_EchoRoom/Editor/PrefabBakedLightingWindow.cs`
  - `Assets/_EchoRoom/Editor/PrefabBakedLightingWindow.cs.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Managers/GameManager.cs`
  - `Assets/_EchoRoom/Scripts/Scriptable Object Scripts/LevelData.cs`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager` — runtime behavior changes through its existing GameManager component; the scene asset itself was not modified or saved.
- **Components/assets/settings:** Each Level now exposes a LevelLightingData bakedLighting reference. The existing `Assets/_EchoRoom/SObjects/LevelData.asset` has five compatible entries (Maze_A through Maze_E); captures populate these references. Default captured output is `Assets/_EchoRoom/Lighting/Prefab Lightmaps/<PuzzleId>/`. Captures preserve LightmapsMode and Texture2D references for color, direction, and shadow-mask data.
- **Decisions and assumptions:** Exact prefab-source matching is preferred. Live MCP inspection found the five MainScene Maze roots are not direct connected prefab instances, so the editor tool also accepts a unique scene-root name matching the configured prefab name; all five current roots match uniquely. The user must enable only the intended level, keep it at its final runtime transform, and disable other levels before baking. Prefab Mode remains the editing workspace, while MainScene is the baking workspace.
- **Verification:** Unity AssetDatabase refresh and script compilation completed successfully. MCP reflection confirmed LevelLightingData, PrefabLightmapRuntime, and PrefabBakedLightingWindow are loaded; the Tools/Echo Room/Prefab Baked Lighting menu is registered; Level.bakedLighting has the expected type; all five configured levels resolve through the unique-name fallback; and the live LevelData asset exposes bakedLighting on every entry. MainScene was clean before implementation and no scene or prefab was saved.
- **Known limitations:** No actual lighting bake/capture or Play Mode runtime application was performed because the live scene currently reports zero loaded lightmaps. Light probes, reflection probes, and occlusion data are not captured. Changing renderer hierarchy, sibling order, meshes, lightmap UVs, materials, baked lights, or the level transform requires rebaking and recapturing that level. The MCP plugin logs its existing warning that the project path contains spaces.
- **Follow-up:** Capture Maze A first, verify its generated asset and runtime appearance in Play Mode, then repeat for Maze B through Maze E. After all captures, clear the temporary MainScene bake to avoid shipping the last staging bake as persistent scene lighting.

### LIGHT-BAKE-GI-001 — Baked GI support without changing sonar reveal

- **Date:** 2026-07-14
- **Goal:** Prevent sonar-reveal materials from becoming incorrectly dark after a baked-lighting workflow, prepare all five level prefabs for lightmapping, and keep the existing material-reveal mechanics unchanged.
- **Result:** The `EchoRoom/EchoSonarReveal` forward pass now compiles lightmapped variants and samples baked GI through URP's `SAMPLE_GI` path for both geometric-normal and normal-mapped lighting. Baked GI is combined only inside the existing revealed-lighting calculation; the black unrevealed base, sonar pulse timing/radius/linger, leading ring, tint/fade, visibility mask, final masked blend, and realtime-light response remain intact. A bake-only Meta pass exports each material's base texture to the lightmapper without affecting runtime rendering. Static maze architecture now contributes GI and receives lightmaps, while doors, levers, buttons, indicators, text, particles, and other non-architecture renderers do not contribute GI and receive light probes. All 22 level point lights are Mixed, preserving realtime direct lighting while allowing baked indirect lighting. Lightmap UV generation is enabled for all five maze FBX importers.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab`
  - `Assets/_EchoRoom/Models/Maze_5x5_A.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_B.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_C.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_D.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_E.fbx.meta`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Affected Unity objects:**
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P1_D1/Lever_Body`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P1_D1/Lever_Pivot/Lever_Handle`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P2_A4/Lever_Body`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P2_A4/Lever_Pivot/Lever_Handle`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P3_E5/Lever_Body`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P3_E5/Lever_Pivot/Lever_Handle`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door Progress Indicators/Indicator 1 (Lever_P1_D1)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door Progress Indicators/Indicator 2 (Lever_P2_A4)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door Progress Indicators/Indicator 3 (Lever_P3_E5)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Writing placement/Level Intro Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door Instruction Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 1`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 1 (1)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 2`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Level Realtime Lights/Maze Point Light 3`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Frame`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P1_Lever/Lever_Body`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P1_Lever/Lever_Pivot/Lever_Handle`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P2_Lever/Lever_Body`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P2_Lever/Lever_Pivot/Lever_Handle`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P3_Lever/Lever_Body`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P3_Lever/Lever_Pivot/Lever_Handle`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door Progress Indicators/Indicator 1 (Auto_P1_Lever)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door Progress Indicators/Indicator 2 (Auto_P2_Lever)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door Progress Indicators/Indicator 3 (Auto_P3_Lever)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Writing placement/Level Intro Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door Instruction Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 1`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 1 (1)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 2`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Level Realtime Lights/Maze Point Light 3`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Frame`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P1_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P1_Button/Button_Object`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P2_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P2_Button/Button_Object`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P3_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P3_Button/Button_Object`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door Progress Indicators/Indicator 1 (Auto_P1_Button)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door Progress Indicators/Indicator 2 (Auto_P2_Button)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door Progress Indicators/Indicator 3 (Auto_P3_Button)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Writing placement/Level Intro Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door Instruction Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 1`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 1 (1)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 2`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 3`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Level Realtime Lights/Maze Point Light 4`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Frame`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Leaf`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P1_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P1_Button/Button_Object`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P2_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P2_Button/Button_Object`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P3_Button`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P3_Button/Button_Object`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door Progress Indicators/Indicator 1 (Auto_P1_Button)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door Progress Indicators/Indicator 2 (Auto_P2_Button)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door Progress Indicators/Indicator 3 (Auto_P3_Button)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Writing placement/Level Intro Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door Instruction Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 1 (1)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 2`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 3`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Level Realtime Lights/Maze Point Light 4`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Floor`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Floor (1)`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Roof`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Maze_Walls`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Entity/Vertical Circular Glow Smoke/Luminous Smoke`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Entity/Vertical Circular Glow Smoke/Smoky Red Eyes/Left Eye`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Entity/Vertical Circular Glow Smoke/Smoky Red Eyes/Right Eye`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Entity/Vertical Circular Glow Smoke/Smoky Red Eyes/Subtle Red Scatter`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Writing placement/Level Intro Text`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 1`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 2`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 3`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 4`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab :: Maze_5x5_E/Level Realtime Lights/Maze Point Light 4 (1)`
- **Components/assets/settings:** `EchoSonarReveal.shader` adds `LIGHTMAP_ON` and `DIRLIGHTMAP_COMBINED` variants, static-lightmap UV/SH varyings, two `SAMPLE_GI` calls, and a custom URP Meta pass. The Meta pass exports only `_BaseMap` albedo and zero emission; it deliberately ignores the dark runtime base color and cannot reveal surfaces at runtime. Across the prefabs, 45 referenced materials still use `EchoRoom/EchoSonarReveal`; 44 have an assigned `_BaseMap`, and no material asset or serialized material value was modified. Static architecture is selected by exact object role: `Door_Frame`, `Maze_Floor`, `Maze_Floor (1)`, `Maze_Roof`, `Maze_Walls`, and renderers under `Exit_Hallway`. Each level has 9 static lightmapped renderers. Non-architecture renderers receive light probes. Mixed-light totals are A=4, B=4, C=5, D=4, E=5. ModelImporter `generateSecondaryUV` is enabled for Maze_5x5_A through Maze_5x5_E.
- **Decisions/assumptions:** Material brightness values were not bulk-adjusted because the audit identified missing baked-GI shader support—not material tuning—as the systemic cause. The project lighting setting uses Mixed IndirectOnly, so Mixed point lights keep realtime direct illumination while contributing baked indirect illumination. Baked GI is evaluated only in the already-revealed shader branch. Moving/interactable geometry is excluded from lightmaps so prefab mechanics and transforms remain valid. The sonar controller, reveal globals, material values, textures, normals, metallic/AO controls, timing, ring, tint, and fade code were not changed.
- **Verification:** Unity reports `EchoRoom/EchoSonarReveal` supported with no shader errors and four active passes: ForwardUnlit, Meta, ShadowCaster, and DepthOnly. A full MCP prefab audit passed: every intended static renderer has Contribute GI enabled, receives lightmaps, and has a complete UV2 set; every other renderer has Contribute GI disabled and receives light probes; every level light is Mixed; and every maze importer generates lightmap UVs. Counts are A 9 static/12 probes/4 lights, B 9/12/4, C 9/12/5, D 9/12/4, and E 9/5/5. Source-level invariants confirmed the black pre-ping base, pulse inputs, ring inputs, reveal visibility mask, and final masked blend are unchanged. Unity was not compiling or updating, the final one-minute error query was empty, and `Assets/_EchoRoom/Scenes/MainScene.unity` remained loaded and clean.
- **Known limitations:** No new lightmaps have been baked or captured in this change, so visual confirmation awaits a bake. Any previously captured lightmap data must be regenerated after the UV/import and renderer-GI changes. The current runtime capture tool does not capture light-probe data; probe-driven moving objects will use probes only if the baking scene provides them, otherwise Unity falls back to ambient SH. One referenced sonar material has no assigned `_BaseMap` and therefore uses the shader's white default in the Meta pass.
- **Follow-up:** In MainScene, enable one level at its final runtime transform, bake lighting, use `Tools > Echo Room > Prefab Baked Lighting` to capture that level, then test its reveal in Play Mode/headset before repeating for the remaining levels.

## 2026-07-14 - LIGHT-INTERACTABLE-RECEIVERS-001

Goal: make lever and button renderers receive baked lighting from the prefab lightmap bake without changing the material reveal mechanics.

Resulting behavior: targeted lever/button mesh renderers in the level prefabs are now marked Lightmap Static/Contribute GI and their serialized renderer GI receiver mode is set to Lightmaps. Existing EchoSonarReveal materials, reveal shader values, EchoButtonInteractable components, colliders, animations, audio sources, and lever/button gameplay scripts were not modified. The currently open scene instances were also adjusted so the next bake in MainScene can include the interactables immediately; the scene was marked dirty but not saved.

Files created/modified/moved/deleted:
- Modified: Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab
- Modified: Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab
- Modified: Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab
- Modified: Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab
- Modified: Docs/PROJECT_MEMORY.md

Affected Unity objects:
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P1_D1/Lever_Body
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P1_D1/Lever_Pivot/Lever_Handle
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P2_A4/Lever_Body
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P2_A4/Lever_Pivot/Lever_Handle
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P3_E5/Lever_Body
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P3_E5/Lever_Pivot/Lever_Handle
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P1_Lever/Lever_Body
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P1_Lever/Lever_Pivot/Lever_Handle
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P2_Lever/Lever_Body
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P2_Lever/Lever_Pivot/Lever_Handle
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P3_Lever/Lever_Body
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P3_Lever/Lever_Pivot/Lever_Handle
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P1_Button
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P1_Button/Button_Object
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P2_Button
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P2_Button/Button_Object
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P3_Button
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Auto_P3_Button/Button_Object
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P1_Button
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P1_Button/Button_Object
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P2_Button
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P2_Button/Button_Object
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P3_Button
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Auto_P3_Button/Button_Object
- Assets/_EchoRoom/Scenes/MainScene.unity :: Button_Prefab
- Assets/_EchoRoom/Scenes/MainScene.unity :: Button_Prefab/Button_Object
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P1_D1/Lever_Body
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P1_D1/Lever_Handle
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P2_A4/Lever_Body
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P2_A4/Lever_Handle
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P3_E5/Lever_Body
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P3_E5/Lever_Handle

Important references and settings:
- Renderer serialized `m_ReceiveGI` set to `1` (`ReceiveGI.Lightmaps`) on targeted lever/button MeshRenderers.
- `StaticEditorFlags.LightmapStatic` added to targeted lever/button renderer GameObjects.
- `m_ScaleInLightmap` left at existing positive values, with zero-or-negative values normalized to `1` if encountered.
- Door progress indicator emissive renderers were excluded intentionally; they remain probe/emissive driven.
- No material assets and no source scripts were modified, preserving the sonar/material reveal behavior.

Decisions, assumptions, limitations, follow-up:
- Baked lighting will appear on these interactables only after rebaking and recapturing the prefab lightmaps. Existing captured lightmap files cannot contain objects that were excluded at bake time.
- Lever handles and animated button parts may carry static baked shading while moving. This is acceptable for the requested baked-light look, but if the motion exposes large hidden surfaces, those parts may need a hybrid probe/lightmap split later.
- MainScene was already dirty with a temporary baked maze instance; this change did not save the scene, so the user can decide when to rebake, capture, clear, or discard the temporary instance.

Verification:
- VERIFY targetedPrefabRenderers=24 bad=0
- Inspected Unity through MCP before changing: lever/button renderers had `lightmapIndex=-1` and were set to probe-based GI, explaining why they were not receiving baked light.

## 2026-07-14 - LIGHT-LEVER-UV2-001

Goal: fix lever meshes appearing dark after baked lighting even when placed near baked lights.

Resulting behavior: `Assets/_EchoRoom/Models/Lever.fbx` now generates secondary lightmap UVs on import, so `Lever_Body` and `Lever_Handle` can sample their assigned baked lightmap regions correctly. This does not change lever/button gameplay scripts, reveal shader timing, material reveal values, colliders, animations, or audio.

Files created/modified/moved/deleted:
- Modified: Assets/_EchoRoom/Models/Lever.fbx.meta
- Modified: Docs/PROJECT_MEMORY.md

Affected Unity objects:
- Assets/_EchoRoom/Models/Lever.fbx :: Lever_Body
- Assets/_EchoRoom/Models/Lever.fbx :: Lever_Handle
- Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A/Lever_P1_D1/Lever_Body
- Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A/Lever_P1_D1/Lever_Pivot/Lever_Handle
- Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A/Lever_P2_A4/Lever_Body
- Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A/Lever_P2_A4/Lever_Pivot/Lever_Handle
- Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A/Lever_P3_E5/Lever_Body
- Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_A/Lever_P3_E5/Lever_Pivot/Lever_Handle

Important references and settings:
- `ModelImporter.generateSecondaryUV = true` for `Assets/_EchoRoom/Models/Lever.fbx`.
- Secondary UV settings: angle distortion 8, area distortion 15, hard angle 88, calculated margin, minimum lightmap resolution 40, minimum object scale 1.
- Mesh `Lever_Body` verification: vertices 5371, UV2 5371.
- Mesh `Lever_Handle` verification: vertices 1329, UV2 1329.

Decisions, assumptions, limitations, follow-up:
- Diagnosis found active lever renderers had valid `lightmapIndex` assignments after the bake, but the source lever meshes had no UV2 channel. Without UV2, the runtime shader can sample the wrong/dark area of the lightmap even when the object sits under a baked light.
- The current bake still needs to be regenerated and captured again; old lightmap textures were produced before the lever model had secondary UVs.
- The sonar reveal remains black before ping by design. This fix targets the revealed/baked-lit state only.

Verification:
- MCP inspection before change: `Lever_Body` and `Lever_Handle` from `Assets/_EchoRoom/Models/Lever.fbx` had `uv2=0` and `generateSecondaryUV=False`.
- MCP verification after change confirmed imported lever meshes now contain UV2 coordinates matching their vertex counts.

## 2026-07-15 - LIGHT-DOOR-PBR-002

Goal: make maze door leaves receive baked lighting and make the metallic maze door frames respond to both baked GI and realtime point/spot lights across the maze prefabs.

Resulting behavior: EchoRoom/EchoSonarReveal PBR materials now add baked indirect specular for metallic surfaces and direct specular from URP additional lights. This fixes fully metallic Door_FrameIron materials being black: their diffuse term was correctly reduced to zero by metallic PBR, but the previous shader had neither baked indirect specular nor additional-light specular. Door leaves in Mazes A-D are now Lightmap Static/Contribute GI receivers so they are included in future bakes. The active Maze B staging instance was updated for the next bake and left unsaved.

Files created/moved/deleted:
- None.

Files modified:
- Assets/_EchoRoom/Shaders/EchoSonarReveal.shader
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab
- Docs/PROJECT_MEMORY.md

Affected Unity objects:
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door_Frame
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Door_Leaf
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Frame
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Door_Leaf
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Frame
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab :: Maze_5x5_C/Door_Leaf
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Frame
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab :: Maze_5x5_D/Door_Leaf
- Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_B/Door_Frame
- Assets/_EchoRoom/Scenes/MainScene.unity :: Maze_5x5_B/Door_Leaf

Important components, assets, settings, and dependencies:
- Door_FrameIron materials for Mazes A-D use EchoRoom/EchoSonarReveal, _RevealQuality=2, and _Metallic=1; material assets were inspected but not modified.
- The PBR branch retains non-metal diffuse lighting and adds indirectSpecular = specColor * normalAmbient * metallic.
- The PBR branch now evaluates specular for URP additional lights, which includes the maze point/spot lights.
- A-D Door_Leaf MeshRenderers now have StaticEditorFlags.LightmapStatic, serialized m_ReceiveGI=1 (Lightmaps), positive scale-in-lightmap, and complete UV2 channels.
- A-D Door_Frame renderers remain lightmapped with complete UV2; no frame prefab flags or material values were changed.
- No sonar pulse inputs, radius/speed/linger, reveal ring, visibility mask, base-black behavior, texture assignments, material values, colliders, animation, audio, or door gameplay scripts were modified.

Decisions, assumptions, known limitations, and follow-up:
- Door leaves move, so their baked shading and baked shadow remain tied to the closed bake pose after opening. This is the accepted tradeoff for the requested baked-light appearance.
- The active Maze B bake finished before its door leaf was changed to a lightmap receiver. Its frame currently has lightmapIndex=0, while the door remains lightmapIndex=-1; Maze B must be baked again before capture so the door receives a lightmap region.
- Assets/_EchoRoom/Scenes/MainScene.unity was already dirty and was not saved. Only the live Maze_5x5_B/Door_Leaf staging object was updated for the next bake.
- Maze E has no Door_Frame/Door_Leaf pair; its existing Maze_5x5_E/Exit_Hallway/Exit_Door was already a lightmapped receiver and was not changed.
- Visual confirmation still requires the Maze B rebake and a sonar reveal test in Scene/Game View or headset.

Verification:
- Unity Editor was not compiling, updating, or baking at final verification.
- EchoRoom/EchoSonarReveal is supported, has four passes, and reports zero shader compilation messages.
- Source invariants confirmed the black pre-ping base and existing sonar visibility-mask expression are unchanged.
- MCP audit passed for all A-D prefab pairs: door receiver OK, frame receiver OK, UV2 count matches vertex count, and no bad entries were found.
- Active MainScene audit: Maze B door receiver OK with pending lightmapIndex=-1; Maze B frame receiver OK with baked lightmapIndex=0.

## 2026-07-15 - MAZEB-LEVER-DEFAULT-OFF-001

Goal: independently test the report that one Maze B lever starts ON, double-confirm the saved and runtime defaults, and remove any configuration capable of starting a lever ON.

Resulting behavior: the three Maze B lever instances now inherit an Animator Controller whose `On` parameter defaults to `false`, matching `LeverInteractable.Awake()` and the controller's default `Off` state. This prevents an Animator from entering the On animation before or without the component's runtime initialization. No lever prefab transform, puzzle event, interaction setting, scene object, animation clip, or gameplay script was changed.

Files created/moved/deleted:
- None.

Files modified:
- Assets/_EchoRoom/Animations/Lever.controller
- Docs/PROJECT_MEMORY.md

Affected Unity objects:
- Assets/_EchoRoom/Prefabs/Level - 1 (echo puzzle).prefab :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P1_D1
- Assets/_EchoRoom/Prefabs/Level - 1 (echo puzzle).prefab :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P2_A4
- Assets/_EchoRoom/Prefabs/Level - 1 (echo puzzle).prefab :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P3_E5
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P1_D1
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P2_A4
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab :: Maze_5x5_A/Lever_P3_E5
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P1_Lever
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P2_Lever
- Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab :: Maze_5x5_B/Auto_P3_Lever
- Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Tutorial Lever
- Assets/_EchoRoom/Prefabs/Lever.prefab :: Lever
- Assets/_EchoRoom/Prefabs/Lever_P1_D1 Variant.prefab :: Lever_P1_D1 Variant
- Assets/_EchoRoom/Prefabs/PuzzleLever_Auto.prefab :: PuzzleLever_Auto
- Assets/_EchoRoom/Prefabs/Tutorial/Tutorial Lever.prefab :: Tutorial Lever
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P1_D1
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P2_A4
- Assets/_EchoRoom/Scenes/MainScene.unity :: Level - 1 (echo puzzle)/Maze_5x5_A/Lever_P3_E5

Important components, assets, settings, and dependencies:
- Shared controller: Assets/_EchoRoom/Animations/Lever.controller.
- Parameter `On`: default changed from `true` to `false`.
- Base Layer default state remains `Off`; the Off state uses `Lever_OnToOff.anim`, ending with the lever pivot at -35 degrees.
- `LeverInteractable.Awake()` already calls `SetAnimatorOn(false)`; `ResetElement()` also forces the logical and Animator states off.
- The controller dependency audit above was generated from Unity's loaded prefabs and open scene; only the controller asset itself was modified.
- Assets/_EchoRoom/Scenes/MainScene.unity was already dirty before this work and was deliberately not saved or otherwise modified.

Decisions, assumptions, known limitations, and follow-up:
- The reported persistent one-lever-on condition was not reproduced in an isolated pre-fix Play Mode instantiation: P1, P2, and P3 all reported logical `IsOn=false`, Animator `On=false`, and the same off pose.
- The mismatched shared controller default was still corrected because it was the only discovered default-on configuration and could produce an incorrect initial animation when component initialization is delayed, skipped, or observed before it resets the parameter.
- This shared correction intentionally affects every lever using the controller; all inspected lever gameplay code expects an off default.
- No headset input test was required because verification targeted initialization without player interaction.

Verification:
- Saved-asset inspection before change: all three Maze B lever transforms and components matched; all used Assets/_EchoRoom/Animations/Lever.controller. The controller default state was `Off`, but parameter `On.defaultBool` was incorrectly `true`.
- Pre-fix Play Mode probe: all three Maze B levers were logically off and visually at pivot Y=325 degrees (-35 degrees).
- Post-fix serialized check after save/reimport: `On.defaultBool=false`, default state `Off`, and all three Maze B lever instances still reference the corrected controller.
- Post-fix Play Mode probe: Auto_P1_Lever, Auto_P2_Lever, and Auto_P3_Lever each had `IsOn=false`, Animator `On=false`, active `Off` state, `Lever_OnToOff` clip, pivot Y=325 degrees, and `ok=true`; aggregate `bad=0`.
- Unity MCP log audit found no exceptions or asserts from the change. The only recent errors were the pre-existing Unity MCP startup warning that the project path contains spaces.

## 2026-07-15 - LOAD-MAZED-LIGHTMAP-RECOVERY-001

- **Goal:** Fix maze transitions becoming permanently covered by the VR loading screen when Maze D was already instantiated, and make the loading presentation more consistent across fast and slow prefab swaps.
- **Resulting behavior:** Maze D and Maze E now use the valid lightmaps mode value 1, matching the current MainScene lighting mode and their captured direction textures. Runtime lightmap application validates saved modes and falls back safely when legacy/corrupt data is encountered. Prefab-swap loading always hides and clears its busy flag through a `finally` block, uses staged visual progress, and remains visible for at least one second so fast swaps do not flash. GameManager level-transition coroutines now release `_isTransitioning` through `finally` blocks when a nested load fails. The baked-lighting capture window refuses to save an undefined lightmaps mode, preventing recurrence.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
  - `Assets/_EchoRoom/Scripts/Managers/GameManager.cs`
  - `Assets/_EchoRoom/Scripts/Lighting/PrefabLightmapRuntime.cs`
  - `Assets/_EchoRoom/Editor/PrefabBakedLightingWindow.cs`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Maze_D_Lighting.asset`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Maze_E_Lighting.asset`
  - `Docs/PROJECT_MEMORY.md`
- **Affected Unity objects:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
- **Important components, assets, settings, and dependencies:**
  - `Maze_D_Lighting.lightmapsMode` changed from invalid `-1` to valid value `1` (`Single, Dual` in this Unity version); it retains 8 captured lightmap sets and 16 renderer bindings.
  - `Maze_E_Lighting.lightmapsMode` changed from invalid `-1` to valid value `1`; it retains 3 captured lightmap sets and 8 renderer bindings.
  - `PrefabLightmapRuntime.ResolveLightmapsMode` accepts defined enum values and otherwise selects value 1 when direction maps exist or NonDirectional when they do not, with a warning.
  - `VRLoadingScreen.CoverPrefabSwap` reports staged progress 0/20/85/100 percent, enforces an effective minimum of one second, and calls `Hide()` from `finally`.
  - The capture workflow checks `Enum.IsDefined(typeof(LightmapsMode), mode)` before recording a bake.
  - Transition cleanup was added to tutorial-to-first-level, next-level, tutorial restart, level restart, and selected-level loading routines.
- **Decisions and assumptions:** Preserved the current directional-lightmap look by using numeric mode 1 because the active scene reports mode 1 and both affected assets contain direction textures. No rebake, texture replacement, prefab hierarchy change, Addressables conversion, or scene save was performed. The already-dirty MainScene was preserved unsaved to avoid overwriting unrelated staging work.
- **Known limitations and follow-up:** Level prefab instantiation and lightmap assignment are still synchronous main-thread work, so genuinely heavy levels can take longer than the one-second minimum and may briefly stall animation. A future optimization pass can standardize lightmap atlas sizes and preload the next maze or migrate level dependencies to an asynchronous loading system. Headset playtesting is still recommended for subjective transition smoothness.
- **Verification:** Unity MCP read-back confirmed Maze D and Maze E both store lightmaps mode 1. A temporary preview-scene test instantiated each configured level prefab and called `PrefabLightmapRuntime.Apply`: Maze D returned `applied=True` for 8 lightmaps/16 bindings and Maze E returned `applied=True` for 3 lightmaps/8 bindings, with no exception. Source read-back confirmed all four defensive code changes. Unity finished compiling and reported `playing=False, compiling=False, updating=False`. A fresh exception query was empty; the only error was the pre-existing Unity MCP warning about spaces in the project path. Direct scene inspection confirmed the affected objects at `GameManager` and `VR Loading Screen`; `MainScene.unity` remained dirty and was not saved.

## 2026-07-15 - PAUSE-UI-INPUT-LIFECYCLE-001

- **Goal:** Fix intermittent mouse hover/click failures when reopening the pause/restart menu and when the entity opens the captured/death menu.
- **Resulting behavior:** The shared world-space UI Toolkit menu now clears retained pointer capture and keyboard focus whenever it hides or changes state. When a pause, start, or captured screen is shown, buttons, its BoxCollider, and the two configured pointer roots remain disabled until two UI panel updates have produced valid geometry; input is then enabled together. This prevents a zero-sized first frame or a stale held pointer from leaving later pause/death screens non-responsive. Restart, return, pause timing, audio pause, and failure-copy behavior are unchanged.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs
  - Docs/PROJECT_MEMORY.md
- **Affected Unity objects:**
  - Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu
  - Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray
  - Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray
- **Important components, assets, settings, and dependencies:**
  - VRPauseMenu.SetVisible now stops any pending activation, releases pointer capture/focus, and gates all menu input while the selected visual tree is laying out.
  - EnableInputAfterLayout waits for two unscaled player-loop frames (yield return null continues while Time.timeScale == 0), repositions/refreshes the panel, then enables all bound UI Toolkit Button elements, the menu BoxCollider, and vrPointerRoots.
  - Pointer capture cleanup recursively covers pointer IDs 0-31 across the complete UIDocument.rootVisualElement tree.
  - OnDestroy also stops the activation coroutine and releases retained UI state.
  - The existing Resources/UI/VRMenu layout, styles, button callback bindings, and serialized scene references were not changed.
- **Decisions and assumptions:** The fix is centralized in the single VRPauseMenu shared by pause and captured/death states so both reports follow the same lifecycle. Two panel updates were chosen because the reproduced first-open frame reported root and button geometry as 0x0, while the next stable probe reported root 900x560 and button 320x78. No scene or prefab serialization was changed or saved; MainScene.unity remained clean after Play Mode.
- **Known limitations and follow-up:** Desktop mouse lifecycle and the shared death-menu path were verified through Unity MCP. A physical headset/controller test is still recommended. The current XRI setup logs an existing compatibility warning for the menu XRRayInteractor input mode; that separate controller-ray configuration was not changed by this mouse/death-menu fix.
- **Verification:**
  - Unity recompiled VRPauseMenu.cs with scriptCompilationFailed=false; the source change introduced no project compilation error.
  - Pause first-frame probe: state Pause, root 0x0, Continue disabled, collider disabled, both pointer roots disabled, Time.timeScale=0, and audio paused. Stable probe: root 900x560, Continue 320x78, button enabled, collider enabled, and both pointer roots enabled.
  - Stale-pointer regression: forced Continue-button capture reported captureBeforeHide=True; HideMenu(true) immediately changed it to captureAfterHide=False, state Hidden, Time.timeScale=1, and audio unpaused.
  - Captured/death first-frame probe: state Captured, root 0x0, Restart disabled, collider/pointers disabled. Stable probe: root 900x560, Restart 320x78, button/collider/pointers enabled.
  - Pause-to-death transition with an active pointer capture released the capture immediately (before=True, after=False), kept Restart disabled during transition, and enabled it only after stable layout.
  - The Restart button's registered Clickable callback was invoked in Play Mode: MainScene reloaded with one VRPauseMenu, state Hidden, Time.timeScale=1, and audio unpaused.
  - Final Editor state: playing=False, compiling=False; Assets/_EchoRoom/Scenes/MainScene.unity was open and dirty=False.

+### MERGE-LIGHTING-001 — Integrate baked-lighting branch without regressing Development

- **Date:** 2026-07-15
- **Goal:** Merge `origin/dev-LightingSetup-Usama` into `Development-Phase`, resolve the reported conflicts, preserve the newer gameplay/polish scene, and retain the baked-lighting pipeline and captures.
- **Result:** Integrated lighting commit `1891d64` with the two newer Development commits. The five prefab-level lightmap captures, runtime lightmap application, shader/GI changes, supporting assets, and the lighting branch journal are present. `Assets/_EchoRoom/Scenes/MainScene.unity` was resolved to the exact `Development-Phase` version because the automatic YAML hybrid had two orphaned parent references (`Entity` and `StartCheckpoint`) and reduced the scene from 504 serialized documents to 174; the chosen scene has 504 documents and zero unresolved parent references. The journal conflict was resolved as a union: the complete Development history is retained and all eight lighting-branch entries are appended intact.
- **Files created:**
  - `Assets/_EchoRoom/Editor/PrefabBakedLightingWindow.cs`
  - `Assets/_EchoRoom/Editor/PrefabBakedLightingWindow.cs.meta`
  - `Assets/_EchoRoom/Lighting.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-000_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-000_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-000_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-000_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-001_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-001_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-001_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-001_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-002_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-002_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-002_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-002_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-003_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-003_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-003_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-003_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-004_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-004_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-004_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-004_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-005_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-005_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-005_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-005_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-006_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-006_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-006_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-006_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-007_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-007_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-007_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Lightmap-007_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Maze_A_Lighting.asset`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_A/Maze_A_Lighting.asset.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_B.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_B/Lightmap-000_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_B/Lightmap-000_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_B/Lightmap-000_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_B/Lightmap-000_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_B/Maze_B_Lighting.asset`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_B/Maze_B_Lighting.asset.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_C.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_C/Lightmap-000_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_C/Lightmap-000_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_C/Lightmap-000_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_C/Lightmap-000_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_C/Maze_C_Lighting.asset`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_C/Maze_C_Lighting.asset.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-000_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-000_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-000_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-000_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-001_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-001_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-001_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-001_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-002_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-002_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-002_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-002_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-003_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-003_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-003_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-003_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-004_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-004_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-004_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-004_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-005_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-005_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-005_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-005_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-006_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-006_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-006_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-006_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-007_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-007_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-007_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Lightmap-007_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Maze_D_Lighting.asset`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_D/Maze_D_Lighting.asset.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-000_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-000_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-000_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-000_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-001_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-001_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-001_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-001_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-002_Color.exr`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-002_Color.exr.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-002_Direction.png`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Lightmap-002_Direction.png.meta`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Maze_E_Lighting.asset`
  - `Assets/_EchoRoom/Lighting/Prefab Lightmaps/Maze_E/Maze_E_Lighting.asset.meta`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E/NavMesh-Maze_5x5_E.asset`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-0_comp_dir.png`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-0_comp_dir.png.meta`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-0_comp_light.exr`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-0_comp_light.exr.meta`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-1_comp_dir.png`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-1_comp_dir.png.meta`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-1_comp_light.exr`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-1_comp_light.exr.meta`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-2_comp_dir.png`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-2_comp_dir.png.meta`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-2_comp_light.exr`
  - `Assets/_EchoRoom/Scenes/MainScene/Lightmap-2_comp_light.exr.meta`
  - `Assets/_EchoRoom/Scenes/MainScene/NavMesh-Maze_5x5_E.asset.meta`
  - `Assets/_EchoRoom/Scenes/MainScene/NavMesh-Maze_Floor.asset`
  - `Assets/_EchoRoom/Scenes/MainScene/NavMesh-Maze_Floor.asset.meta`
  - `Assets/_EchoRoom/Scripts/Lighting.meta`
  - `Assets/_EchoRoom/Scripts/Lighting/LevelLightingData.cs`
  - `Assets/_EchoRoom/Scripts/Lighting/LevelLightingData.cs.meta`
  - `Assets/_EchoRoom/Scripts/Lighting/PrefabLightmapRuntime.cs`
  - `Assets/_EchoRoom/Scripts/Lighting/PrefabLightmapRuntime.cs.meta`
  - `Assets/_Recovery/0 (24).unity`
  - `Assets/_Recovery/0 (24).unity.meta`
  - `Assets/_Recovery/0 (25).unity`
  - `Assets/_Recovery/0 (25).unity.meta`
  - `Assets/_Recovery/0 (26).unity`
  - `Assets/_Recovery/0 (26).unity.meta`
- **Files modified:**
  - `Assets/_EchoRoom/Animations/Lever.controller`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Iron.mat`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Red.mat`
  - `Assets/_EchoRoom/Materials/Lever/Lever_Stone.mat`
  - `Assets/_EchoRoom/Materials/MazeB/Door_FrameIron.mat`
  - `Assets/_EchoRoom/Materials/MazeB/Door_Wood.mat`
  - `Assets/_EchoRoom/Models/Lever.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_A.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_B.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_C.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_D.fbx.meta`
  - `Assets/_EchoRoom/Models/Maze_5x5_E.fbx.meta`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_A.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_B.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_C.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_D.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E.prefab`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/New Lighting Settings.lighting`
  - `Assets/_EchoRoom/SObjects/LevelData.asset`
  - `Assets/_EchoRoom/Scenes/MainScene/LightingData.asset`
  - `Assets/_EchoRoom/Scripts/Managers/GameManager.cs`
  - `Assets/_EchoRoom/Scripts/Scriptable Object Scripts/LevelData.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`
  - `Docs/PROJECT_MEMORY.md`
  - `ProjectSettings/NavMeshAreas.asset`
- **Files renamed:**
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E/NavMesh-Maze_5x5_E 1.asset.meta -> Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E/NavMesh-Maze_5x5_E.asset.meta`
  - `Assets/_EchoRoom/Prefabs/Level prefabs/Maze_5x5_E/NavMesh-Maze_5x5_E 1.asset -> Assets/_EchoRoom/Scenes/MainScene/NavMesh-Maze_5x5_E.asset`
- **Files deleted:**
  - None.
- **Files conflict-resolved but unchanged relative to the first parent:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
- **Unity objects affected:** The imported lighting branch’s exhaustive affected-object paths are preserved in the eight immediately preceding `LIGHT-*`, `MAZEB-*`, `LOAD-*`, and `PAUSE-*` journal entries. Conflict resolution itself changes no serialized Unity object relative to `Development-Phase`; it deliberately preserves that branch’s complete `MainScene` hierarchy.
- **Components/assets/settings:** Preserved `LevelLightingData`, `PrefabLightmapRuntime`, `PrefabBakedLightingWindow`, all five `LevelData.bakedLighting` references, the captured Maze A–E lightmaps, shader baked-GI/PBR support, level prefab GI/lightmap settings, loading recovery, and the shared lever-controller default correction. `MainScene` staging lightmap support files remain imported, but the scene YAML itself stays at the newer Development version.
- **Decisions and assumptions:** A whole-side choice was required for `MainScene`: retaining the automatic text merge would have shipped broken Unity PPtrs, while taking the lighting side would have discarded extensive newer gameplay and polish serialization. The project-local memory uses an append-only union rather than choosing either side. No branch content outside the scene conflict was intentionally discarded.
- **Verification:** No conflict markers remain, and `git diff --check` passes for the conflict-resolution worktree edits. The cached lighting commit retains Unity-generated trailing spaces after empty YAML values, so a whole-index `git diff --cached --check` reports those pre-existing source-branch lines; they were not mass-rewritten during the merge. Static integrity audit passed for all 159 first-parent changed paths: every added Unity asset has its `.meta`, every added `.meta` has its asset, all changed scene/prefab `m_Father` references resolve locally, and all five `LevelData.bakedLighting` GUIDs resolve. Unity successfully performed two AssetDatabase refreshes; the first domain reload compiled the merged scripts with zero assembly errors, and the final refresh imported the restored Development scene file. The Editor log confirmed the hybrid’s broken PPtrs before correction and no such error during the final file import.
- **Known limitations:** The Unity HTTP/MCP bridge stopped responding after the final domain reload, so the planned live renderer-binding/script/shader audit timed out without a result. Not inspected — Unity connection unavailable for a final live hierarchy readback. This is a verification-tool limitation, not a returned project audit failure.
- **Follow-up:** Reconnect or restart the Unity MCP bridge, reopen `MainScene`, and run the five-level renderer-binding/missing-script/shader audit before the next release build. A headset smoke test remains appropriate for the imported baked lighting and loading transitions.

## 2026-07-17 ? MAINMENU-LONGWALL2-LIGHTMAP-001

Goal: Fix `Long Wall (2)` in the Main Menu Scene tutorial T-junction copy so it receives baked lighting correctly.

Resulting behavior: `Long Wall (2)` now uses a generated scene-local mesh with secondary lightmap UVs, contributes to baked GI, receives baked GI, and has a normal lightmap allocation after rebake. The matching door-frame `Long Wall (1)` was fixed the same way because it had the same missing-UV2 lightmap issue.

Files created or modified:
- Modified: `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
- Modified: `Assets/_EchoRoom/Scenes/MainMenuScene/LightingData.asset`
- Modified/generated by Unity bake: `Assets/_EchoRoom/Scenes/MainMenuScene/Lightmap-*_comp_light.exr`
- Created: `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/MainMenu_LongWall2_LightmapUV.asset`
- Created: `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/MainMenu_LongWall2_LightmapUV.asset.meta`
- Created: `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/MainMenu_LongWall1_LightmapUV.asset`
- Created: `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/MainMenu_LongWall1_LightmapUV.asset.meta`
- Modified: `Docs/PROJECT_MEMORY.md`

Affected Unity objects:
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Long Wall (2)`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Long Wall (1)`

Important component, asset, setting, and dependency references:
- `Long Wall (2)` MeshFilter and MeshCollider now reference `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/MainMenu_LongWall2_LightmapUV.asset`.
- `Long Wall (1)` MeshFilter and MeshCollider now reference `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/MainMenu_LongWall1_LightmapUV.asset`.
- Both generated meshes have 24 vertices, UV0 count 24, and UV2 count 24 after `Unwrapping.GenerateSecondaryUVSet`.
- Both renderers remain static / Contribute GI and use Receive GI mode `Lightmaps` (`m_ReceiveGI=1`).
- Both renderers were set to `m_ScaleInLightmap=8` before rebaking to avoid the previous tiny atlas allocation.
- Lighting settings remain `Assets/_EchoRoom/Scenes/MainMenuScene_LightingSettings.lighting`.

Decisions, assumptions, known limitations, and follow-up work:
- Root cause was missing secondary lightmap UVs (`uv2=0`) on `Long Wall (2)`, which previously produced a tiny lightmap scale/offset allocation around `0.01 x 0.01` and made the wall appear unlit.
- `Long Wall (1)` was updated too because it shared the same missing-UV2 issue, even though the user reported `Long Wall (2)` specifically.
- No tutorial scripts or gameplay extras were added back to the Main Menu copy; this only changes render/collider mesh references and baked lighting data.

Verification performed:
- Unity MCP inspection after bake: `Lightmapping.isRunning=False`, active scene `MainMenuScene`, scene dirty `False`, lightmap count `1`.
- `Long Wall (2)` verification: mesh path `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/MainMenu_LongWall2_LightmapUV.asset`, `uv2=24`, `lightmapIndex=0`, `lightmapScaleOffset=(1.7379, 1.7391, -0.4340, -0.4274)`, `m_ScaleInLightmap=8`, `m_ReceiveGI=1`.
- `Long Wall (1)` verification: mesh path `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/MainMenu_LongWall1_LightmapUV.asset`, `uv2=24`, `lightmapIndex=0`, `lightmapScaleOffset=(1.7379, 1.7391, -0.4340, -0.4343)`, `m_ScaleInLightmap=8`, `m_ReceiveGI=1`.

## 2026-07-17 ? MAINMENU-LONGWALL2-LIGHTMAP-001-CORRECTION

Correction to `MAINMENU-LONGWALL2-LIGHTMAP-001`: the generated mesh folder and its Unity meta file are also part of the change set.

Additional files created or modified:
- Created/modified: `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes/`
- Created/modified: `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes.meta`

Verification performed:
- Unity MCP/AssetDatabase confirmed `Assets/_EchoRoom/Scenes/MainMenuScene/GeneratedMeshes` is a valid AssetDatabase folder containing the generated long-wall lightmap UV mesh assets.

## 2026-07-17 ? MAINMENU-VISIBLE-LOCKED-001

Goal: Make `MainMenuScene` function as a stable menu environment: the tutorial T-junction should be visibly lit/materialed all the time, and the player should not move around from the menu start position. The existing completed lighting/bake was not to be changed.

Resulting behavior: The Main Menu tutorial T-junction renderers now use menu-specific URP Lit materials instead of the sonar-reveal materials, so the environment is visible continuously without needing gameplay echo/reveal behavior. Artificial player movement, teleportation, turn/body locomotion, sprint, and gameplay player-controller movement are disabled in `MainMenuScene`, while the dedicated menu UI rays remain active for menu interaction. Existing light objects and baked lightmap data were preserved; no rebake was started.

Files created:
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_Wall_Visible.mat`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_Wall_Visible.mat.meta`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_WallAccent_Visible.mat`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_WallAccent_Visible.mat.meta`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_Floor_Visible.mat`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_Floor_Visible.mat.meta`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_DoorBrown_Visible.mat`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_DoorBrown_Visible.mat.meta`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_DoorSilver_Visible.mat`
- `Assets/_EchoRoom/Materials/MainMenu/MainMenu_Env_DoorSilver_Visible.mat.meta`

Files modified:
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
- `Docs/PROJECT_MEMORY.md`

Files moved/deleted: None.

Unity objects affected:
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Floor`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Wall (1)`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Interaction Wall`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Wall With Corridor Face`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Long Wall`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Long Wall (2)`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Door`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Door/Knob`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Frame`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Door Frame/Door_3_Brown/Long Wall (1)`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu Environment/Tutorial T Junction Environment/Entity wall`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Locomotion System/Move`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Locomotion System/Teleportation`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/PlayerController`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Teleport Interactor`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Teleport Interactor`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray`

Important component, asset, setting, and dependency references:
- The 12 tutorial T-junction renderers now use `Universal Render Pipeline/Lit` menu-visible material assets in `Assets/_EchoRoom/Materials/MainMenu/`.
- Floor uses `MainMenu_Env_Floor_Visible`; primary walls use `MainMenu_Env_Wall_Visible`; interaction/entity walls use `MainMenu_Env_WallAccent_Visible`; door/frame pieces use `MainMenu_Env_DoorBrown_Visible`; the knob uses `MainMenu_Env_DoorSilver_Visible`.
- Disabled movement-related behaviours: `XRBodyTransformer`, `LocomotionMediator`, `DynamicMoveProvider`, `TeleportLocomotionController`, `EchoTeleportationProvider`, `PlayerController`, `DynamicSprintController`, `UnifiedLocomotionBridge`, and the teleport interactor `XRRayInteractor` / line visual behaviours.
- Kept menu UI ray objects active: `Left Controller/Menu UI Ray` and `Right Controller/Menu UI Ray` retain enabled `LineRenderer`, `XRRayInteractor`, and `XRInteractorLineVisual` components.
- Existing lighting was preserved: light count remained 9, including 6 baked spotlights, 1 baked directional light, 1 baked point light, and the inactive realtime camera point light. `LightmapSettings.lightmaps.Length` remained 1 and `Lightmapping.isRunning` remained false.

Decisions, assumptions, known limitations, and follow-up work:
- The visibility issue was treated as a Main Menu scene presentation problem caused by gameplay sonar-reveal materials being unsuitable for a static menu environment. Menu-only Lit material assets were used so gameplay materials in other scenes are not changed.
- The player lock disables artificial/controller locomotion and teleportation. It does not forcibly freeze real-world headset movement, because forcing the camera against physical HMD motion can be uncomfortable in VR.
- Lighting was intentionally not rebaked and no light component settings were intentionally modified.

Verification performed:
- Unity MCP verification reported `MainMenuScene` dirty `False`, `Lightmapping.isRunning=False`, and `lightmaps=1` after the change.
- Unity MCP verification reported 12 environment renderers active/enabled, 12 non-sonar material slots, and 0 `EchoRoom/EchoSonarReveal` material slots under the tutorial T-junction copy.
- Unity MCP verification reported light summary `total=9`, `spots=6`, `baked=8`, `realtime=1`; the six user-added spotlights are present and baked.
- Unity MCP verification confirmed movement/teleport/gameplay locomotion components are disabled while both `Menu UI Ray` objects remain active with enabled menu ray components.

## 2026-07-17 ? MAINMENU-SIZE-SIDEFADE-001

Goal: Make the `MainMenuScene` main menu slightly larger and add soft fading on the left and right sides of the menu without changing the completed menu environment lighting.

Resulting behavior: `Main Menu` is scaled up by about 15% (`localScale` from `(-0.001600, 0.001600, 0.001600)` to `(-0.001840, 0.001840, 0.001840)`). Two transparent side-fade visual quads are parented under the menu as non-interactive overlays, so the menu edges fade visually while controller ray interaction still goes to the original menu collider/buttons.

Files created:
- `Assets/_EchoRoom/Shaders/MenuSideFade.shader`
- `Assets/_EchoRoom/Shaders/MenuSideFade.shader.meta`
- `Assets/_EchoRoom/Materials/MainMenu/MenuSideFade_Left.mat`
- `Assets/_EchoRoom/Materials/MainMenu/MenuSideFade_Left.mat.meta`
- `Assets/_EchoRoom/Materials/MainMenu/MenuSideFade_Right.mat`
- `Assets/_EchoRoom/Materials/MainMenu/MenuSideFade_Right.mat.meta`

Files modified:
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
- `Docs/PROJECT_MEMORY.md`

Files moved/deleted: None.

Unity objects affected:
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu/Menu Side Fade Left`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu/Menu Side Fade Right`

Important component, asset, setting, and dependency references:
- `Main Menu` remains at position `(0.0000, 1.1500, -8.7500)` and rotation `(0.000, 180.000, 0.000)`; only local scale changed.
- `Main Menu` BoxCollider remains a trigger with local size `(900, 560, 4)`; world bounds after scaling verified as approximately `(1.6560, 1.0304, 0.0074)`.
- `Menu Side Fade Left` local position `(-385, 0, 2)`, local scale `(130, 640, 1)`, material `Assets/_EchoRoom/Materials/MainMenu/MenuSideFade_Left.mat`, no Collider.
- `Menu Side Fade Right` local position `(385, 0, 2)`, local scale `(130, 640, 1)`, material `Assets/_EchoRoom/Materials/MainMenu/MenuSideFade_Right.mat`, no Collider.
- Shader `EchoRoom/UI/MenuSideFade` is transparent, `ZWrite Off`, `ZTest Always`, `Cull Off`, and uses UV-based alpha gradient with `renderQueue=5100`.
- `VR Loading Screen` was intentionally left at its previous scale `(-0.001600, 0.001600, 0.001600)` because the request targeted the main menu.

Decisions, assumptions, known limitations, and follow-up work:
- A 15% increase was chosen as a 'little bigger' VR-safe bump without pushing the panel too far into the user's view.
- The fade is implemented as two visual-only overlay quads instead of editing the UI Toolkit document, so it does not alter button layout or block XR UI raycasts.
- Lighting was intentionally not modified or rebaked.
- Follow-up: headset check the edge fade intensity; adjust `_MaxAlpha` on the two fade materials if the sides feel too dark or too subtle.

Verification performed:
- Unity MCP reloaded `MainMenuScene` and verified scene dirty `False`, `Lightmapping.isRunning=False`, and `LightmapSettings.lightmaps.Length=1`.
- Unity MCP verified `Main Menu` local scale `(-0.001840, 0.001840, 0.001840)` and world collider bounds `(1.6560, 1.0304, 0.0074)`.
- Unity MCP verified both side-fade objects active, renderer enabled, shader `EchoRoom/UI/MenuSideFade`, renderQueue `5100`, `_MaxAlpha=0.78`, and no colliders.
- Unity MCP ray checks from both `Left Controller/Menu UI Ray` and `Right Controller/Menu UI Ray` still hit the `Main Menu` BoxCollider at approximately `(0.000, 0.820, -8.754)`.

## 2026-07-17 ? LOADING-FAKE-FIVE-SECONDS-001

Goal: Give every VR loading screen a fake 5-second display duration so transitions do not disappear instantly.

Resulting behavior: `VRLoadingScreen` now defaults to a 5-second minimum display time. Scene-loading transitions delay scene activation until both the real async load reaches ready state and the 5-second fake loading time has elapsed. In-scene prefab/level-swap loading overlays also hold for at least 5 seconds. Existing `VR Loading Screen` scene component instances in both menu and gameplay scenes were updated to serialize `minimumDisplayTime=5`.

Files created/moved/deleted: None.

Files modified:
- `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
- `Assets/_EchoRoom/Scenes/MainScene.unity`
- `Docs/PROJECT_MEMORY.md`

Unity objects affected:
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen`
- `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`

Important component, asset, setting, and dependency references:
- `VRLoadingScreen.minimumDisplayTime` default changed from `1f` to `5f`.
- `LoadSceneRoutine` now sets `AsyncOperation.allowSceneActivation=false`, updates progress using the lesser of load progress and elapsed fake-time progress, then sets progress to `100%` and allows activation after the 5-second minimum.
- `ShowThankYouThenLoad` uses the same delayed scene activation for the loading segment after the thank-you screen.
- `CoverPrefabSwap` now uses `Mathf.Max(5f, minimumDisplayTime)` so level/prefab swap overlays are also shown for at least 5 seconds.
- Serialized `minimumDisplayTime` set to `5` on `VR Loading Screen` in both `MainMenuScene` and `MainScene`.

Decisions, assumptions, known limitations, and follow-up work:
- 'Each loading screen' was interpreted to include Main Menu to gameplay scene loads, gameplay returns to main menu, thank-you-to-main-menu loading, and in-scene level/prefab swap loading overlays.
- This is a fake minimum display duration; it does not slow asset loading itself, only the visible transition timing/scene activation.
- Follow-up: headset/play-mode test each transition to make sure the 5-second wait feels intentional and not too long.

Verification performed:
- Unity MCP verification reported `compiling=False` and `updating=False` after the change.
- Unity MCP verification found script default `minimumDisplayTime = 5f`, two `operation.allowSceneActivation = false` assignments, two matching `operation.allowSceneActivation = true` assignments, and `Mathf.Max(5f, minimumDisplayTime)` in the prefab-swap path.
- Unity MCP verification opened both scenes and reported `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen minimumDisplayTime=5 dirty=False` and `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen minimumDisplayTime=5 dirty=False`.
- Unity MCP console query after the change showed existing project/MCP path-space errors and existing warnings, but no new C# compile error from `VRLoadingScreen.cs`.

## 2026-07-17 ? MAINMENU-PLAYER-CLOSER-001

Goal: Move the Main Menu scene player spawn a little closer to the main menu panel.

Resulting behavior: `XR Origin (XR Rig)` in `MainMenuScene` now starts 0.75m closer to the menu, moving from `(0.0000, -0.1800, -12.0000)` to `(0.0000, -0.1800, -11.2500)`. The player still faces forward toward the T-junction/menu, and the camera-to-menu flat distance is now approximately 2.5m instead of 3.25m.

Files created/moved/deleted: None.

Files modified:
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
- `Docs/PROJECT_MEMORY.md`

Unity objects affected:
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)`
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Main Camera` (world position changes through parent transform)
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray` (world position changes through parent transform)
- `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray` (world position changes through parent transform)

Important component, asset, setting, and dependency references:
- `XR Origin (XR Rig)` rotation remains `(0.000, 0.000, 0.000)` and scale remains `(1, 1, 1)`.
- `Main Menu` remains at `(0.0000, 1.1500, -8.7500)` and was not moved.
- Lighting was not changed or rebaked; verification reported `Lightmapping.isRunning=False` and `LightmapSettings.lightmaps.Length=1`.

Decisions, assumptions, known limitations, and follow-up work:
- 'Little closer' was interpreted as 0.75m forward along the hallway, reducing menu distance to a comfortable VR reading/interacting distance of about 2.5m.
- Follow-up: headset check comfort/readability; if the menu still feels far, move another small step forward rather than jumping too close.

Verification performed:
- Unity MCP reloaded `MainMenuScene` and reported scene dirty `False`, `Lightmapping.isRunning=False`, and `lightmaps=1`.
- Unity MCP verified `XR Origin (XR Rig)` position `(0.0000, -0.1800, -11.2500)` and `Main Camera` position `(0.0000, 0.8200, -11.2500)`.
- Unity MCP verified camera-to-menu flat distance `2.5`.
- Unity MCP ray checks confirmed both `Left Controller/Menu UI Ray` and `Right Controller/Menu UI Ray` still hit the `Main Menu` BoxCollider at approximately `(0.000, 0.820, -8.754)` with hit distance about `2.496`.
- One intermediate MCP verification helper failed to compile because it omitted the UnityEditor namespace for `Lightmapping`; the corrected verification helper passed and did not require scene changes.

## 2026-07-18 — MCP-CONNECTION-CONFIG-001

Goal: Correct the project-local Codex MCP configuration so Codex connects to the Unity MCP server that is already running, instead of attempting to launch a second server on the same port.

Resulting behavior: The enabled `unity-mcp` entry now connects to the live streamable-HTTP endpoint at `http://127.0.0.1:8080/`. The stale `ai-game-developer` entry targeting the unavailable port `27734` is disabled, preventing redundant connection failures. A new Codex task/session can load the corrected configuration and expose the Unity MCP tools.

Files created/moved/deleted: None.

Files modified:
- `.codex/config.toml`
- `Docs/PROJECT_MEMORY.md`

Unity objects affected: None; this change only updates Codex MCP connection settings.

Important component, asset, setting, and dependency references:
- `mcp_servers.unity-mcp.enabled = true`
- `mcp_servers.unity-mcp.url = "http://127.0.0.1:8080/"`
- Removed the conflicting command/arguments that launched `Library/mcp-server/win-x64/gamedev-mcp-server.exe` with `port=8080` and `client-transport=stdio`.
- `mcp_servers.ai-game-developer.enabled = false`; its unavailable legacy URL remains documented in the configuration but is no longer started.
- The running endpoint identifies itself as `gamedev-mcp-server` version `9.0.0.0` using MCP protocol version `2025-03-26`.

Decisions, assumptions, known limitations, and follow-up work:
- The existing server process was treated as authoritative because it is the project-bundled `gamedev-mcp-server.exe`, is listening on loopback port `8080`, and successfully completes MCP initialization.
- MCP configuration is loaded when a Codex task/session starts; the current task does not dynamically gain newly configured Unity tools. Open a new task or restart/reload Codex for the tool list to refresh.
- No Unity scene, prefab, asset, component, or Editor state was changed.

Verification performed:
- Confirmed the listener on port `8080` is `H:/Echo room/Echo-Room-VR/Library/mcp-server/win-x64/gamedev-mcp-server.exe` running with `client-transport=streamableHttp`.
- Sent an MCP `initialize` request to `http://127.0.0.1:8080/`; received HTTP `200`, an MCP session ID, and a valid `gamedev-mcp-server` initialization response.
- Re-read `.codex/config.toml` and confirmed the active Unity URL and disabled legacy connector.
- Normalized `.codex/config.toml` to UTF-8 without BOM and consistent CRLF line endings.

## 2026-07-18 — TUTORIAL-LIGHT-CAPTURE-001

Goal: Allow the existing prefab baked-lighting tool to capture the baked `T_Junction_Tutorial` staging instance and make the saved capture available when `GameManager` instantiates the tutorial at runtime.

Resulting behavior: `Tools > Echo Room > Prefab Baked Lighting` now accepts either a configured puzzle level or the tutorial prefab referenced by the `GameManager` in the selected root's scene. A tutorial capture is saved under `Assets/_EchoRoom/Lighting/Prefab Lightmaps/T_Junction_Tutorial/`, assigned to the new `LevelData.tutorialBakedLighting` slot, and applied to the instantiated tutorial through the existing `PrefabLightmapRuntime` pipeline. `Bake Selected Level & Capture` no longer incorrectly requires an already-loaded bake before it can start; `Capture Existing Bake` still validates that lightmaps are loaded.

Files created/moved/deleted: None.

Files modified:
- `Assets/_EchoRoom/Editor/PrefabBakedLightingWindow.cs`
- `Assets/_EchoRoom/Scripts/Managers/GameManager.cs`
- `Assets/_EchoRoom/Scripts/Scriptable Object Scripts/LevelData.cs`
- `Docs/PROJECT_MEMORY.md`

Unity objects affected:
- `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager`
- `Assets/_EchoRoom/Scenes/MainScene.unity :: T_Junction_Tutorial`
- `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial`

Important component, asset, setting, and dependency references:
- `LevelData.tutorialBakedLighting` stores the persistent `LevelLightingData` reference after capture.
- `GameManager.TutorialLevelPrefab` exposes the configured tutorial prefab read-only so the editor window can match the selected staging instance.
- `GameManager.LoadTutorialLevel` applies `levelData.tutorialBakedLighting` immediately after the tutorial instance is activated and before the player is moved to its spawn.
- Tutorial matching prefers the exact prefab source, then permits a unique prefab-name match among `GameManager` components in the selected root's loaded scene.
- Expected capture asset: `Assets/_EchoRoom/Lighting/Prefab Lightmaps/T_Junction_Tutorial/T_Junction_Tutorial_Lighting.asset`.

Decisions, assumptions, known limitations, and follow-up work:
- The tutorial lighting reference was added to the shared `LevelData` asset instead of the `MainScene` GameManager component so the capture can save its assignment through `AssetDatabase.SaveAssets` without requiring a scene save.
- Existing Maze A-E capture behavior and references are unchanged.
- No capture was executed as part of this code fix, so `tutorialBakedLighting` remains unassigned until the user selects `T_Junction_Tutorial` and presses `Capture Existing Bake` (or rebakes with `Bake Selected Level & Capture`).
- Existing pipeline limitations remain: light probes, reflection probes, and occlusion data are not captured, and hierarchy/sibling-order, UV, material, lighting, or transform changes require recapture.

Verification performed:
- Unity MCP confirmed `MainScene` is loaded and clean, `GameManager` exists at the exact root path above, and the active `T_Junction_Tutorial` staging instance is active with its inspected hierarchy and baked-light group.
- Unity MCP compilation completed with `IsCompiling=False` and `IsUpdating=False`.
- A non-destructive MCP dry test resolved the selected scene root to `Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab`, reported 3 loaded lightmaps and 19 valid baked renderer bindings, and confirmed the compiled tutorial data slot and runtime prefab property.
- MCP source read-back confirmed tutorial capture assignment, runtime application, and corrected first-bake validation. An intermediate MCP patch retry and an early dry test produced tooling-only Console errors; the final source correction compiled successfully. The remaining fresh Editor error is the existing Unity MCP warning about spaces in the project path, not a C# compiler error.

## 2026-07-18 - TUTORIAL-CONTROLLER-PROMPT-001

Goal: Replace the tutorial's wall-mounted instructions with a larger, clearer prompt beside the right controller at the same presentation location used by the maze timer.

Resulting behavior: The tutorial now creates one world-space controller prompt at runtime and reuses it for the sonar, microphone, movement, interaction, and warning steps. The prompt follows the right controller using the maze timer's view-relative offset, faces the headset, fades between messages, and uses larger cyan-white bold text with a dark translucent background and strong outline. The five former wall-mounted TextMeshPro instruction objects were removed from the tutorial prefab and its MainScene staging instance. Tutorial sequencing, proximity gates, button/lever completion, warning timing, ending audio, and return-to-menu behavior are unchanged.

Files created/moved/deleted: None created or moved. The five tutorial wall-text GameObjects listed below were deleted from the prefab and scene hierarchy.

Files modified:
- Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs
- Assets/_EchoRoom/Scripts/Tutorial/TutorialRuntimeObserver.cs
- Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab
- Assets/_EchoRoom/Scenes/MainScene.unity
- Docs/PROJECT_MEMORY.md

Unity objects affected:
- Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System
- Assets/_EchoRoom/Scenes/MainScene.unity :: T_Junction_Tutorial/Sonar Wall Tip
- Assets/_EchoRoom/Scenes/MainScene.unity :: T_Junction_Tutorial/Microphone Wall Tip
- Assets/_EchoRoom/Scenes/MainScene.unity :: T_Junction_Tutorial/Direction Wall Tip
- Assets/_EchoRoom/Scenes/MainScene.unity :: T_Junction_Tutorial/Interaction Wall Tip
- Assets/_EchoRoom/Scenes/MainScene.unity :: T_Junction_Tutorial/Entity Wall Tip
- Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Sonar Wall Tip
- Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Microphone Wall Tip
- Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Direction Wall Tip
- Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Interaction Wall Tip
- Assets/_EchoRoom/Prefabs/Level prefabs/T_Junction_Tutorial.prefab :: T_Junction_Tutorial/Entity Wall Tip

Important component, asset, setting, and dependency references:
- TutorialDirector creates a runtime world-space Canvas named Tutorial Controller Prompt under Tutorial System, containing a dark Image background, CanvasGroup, and TextMeshProUGUI Prompt Text child.
- Prompt positioning matches MazeLevelTimer's view-relative right-controller offset: (-0.13, 0.035, 0.015). The panel is 700 x 260 canvas units at 0.0005 world scale.
- Prompt text is bold, centered, cyan-white, auto-sized from 25 to 36, word wrapped, and uses a 0.22 dark outline. The background alpha is 0.90 and does not receive raycasts.
- TutorialRuntimeObserver now monitors the single runtime prompt through tutorialPromptText and TEXT_PROMPT instead of searching for removed wall labels.
- Assets/_EchoRoom/Lighting/Prefab Lightmaps/T_Junction_Tutorial/T_Junction_Tutorial_Lighting.asset remains assigned with 3 lightmaps and 19 renderer bindings; no rebake or recapture was performed.

Decisions, assumptions, known limitations, and follow-up work:
- "Same place as the timer" was implemented by copying MazeLevelTimer's right-controller/head-relative positioning calculation. The maze timer remains disabled during the tutorial, so the two displays do not overlap.
- A single reusable prompt avoids duplicate visible instructions and keeps all presentation close to the player's controller.
- Interaction Wall and Entity wall remain in the prefab as geometry/proximity landmarks; only their TextMeshPro instruction children were removed.
- No Play Mode or headset visual pass was performed. Prompt size, offset, binocular readability, and hand occlusion should be checked in headset and tuned if needed.
- The previously identified active MainScene tutorial staging root versus runtime-instantiated tutorial duplication risk was not changed by this presentation task.

Verification performed:
- Unity compilation completed with EditorUtility.scriptCompilationFailed=False, EditorApplication.isCompiling=False, and EditorApplication.isUpdating=False.
- MCP source verification found the new controller-prompt creation, positioning, fade, and diagnostic code and found no remaining references to the five wall text names or the old SwapWallText/BuildWallInstructions path.
- MCP prefab inspection reported prefabWallTexts=0 for all five removed objects.
- MCP live MainScene inspection reported sceneWallTexts=0 on the T_Junction_Tutorial staging instance.
- The tutorial lighting capture still reports lightmaps=3 and rendererBindings=19.
- MainScene was saved through MCP after prefab propagation and finished clean.


## 2026-07-18 - TUTORIAL-PROMPT-SIZE-001

Goal: Make the new right-controller tutorial text 50 percent smaller.

Resulting behavior: Tutorial Controller Prompt text now uses an initial and maximum font size of 18 instead of 36, with its auto-size minimum reduced from 25 to 12.5. The prompt position, 700 x 260 panel size, dark translucent background, outline, fades, wording, and tutorial sequence are unchanged.

Files created/moved/deleted: None.

Files modified:
- Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs
- Assets/_EchoRoom/Scenes/MainScene.unity
- Docs/PROJECT_MEMORY.md

Unity objects affected:
- Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System

Important component, asset, setting, and dependency references:
- TutorialDirector.CreateControllerPrompt configures TextMeshProUGUI fontSize=18, fontSizeMin=12.5, and fontSizeMax=18.
- PromptCanvasScale remains 0.0005 and PromptPanelSize remains 700 x 260.

Decisions, assumptions, known limitations, and follow-up work:
- "Text 50% smaller" was applied to all three font-size values only. The background panel and controller-relative placement were intentionally left unchanged.
- No Play Mode or headset visual pass was performed; controller-distance readability should be confirmed in headset.

Verification performed:
- MCP source readback confirmed fontSize=18, fontSizeMin=12.5, and fontSizeMax=18.
- Unity compilation completed with scriptCompilationFailed=False and isCompiling=False.
- MainScene was saved after Unity's script reload marked it dirty.


## 2026-07-18 - TUTORIAL-PROMPT-STYLE-001

Goal: Remove the right-controller tutorial prompt background and make its text slightly smaller.

Resulting behavior: Tutorial Controller Prompt now displays floating outlined text with no Image background or CanvasRenderer on the prompt canvas. Its initial and maximum font size changed from 18 to 15, and its auto-size minimum changed from 12.5 to 10.5. Controller-relative placement, panel layout area, cyan-white color, bold styling, outline, fades, wording, and tutorial sequence are unchanged.

Files created/moved/deleted: None.

Files modified:
- Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs
- Docs/PROJECT_MEMORY.md

Unity objects affected:
- Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System

Important component, asset, setting, and dependency references:
- TutorialDirector.CreateControllerPrompt now creates the world-space Canvas with CanvasGroup only; the Prompt Text child retains its TextMeshProUGUI and CanvasRenderer.
- TextMeshProUGUI settings are fontSize=15, fontSizeMin=10.5, fontSizeMax=15, bold, centered, cyan-white, and outlined.
- PromptCanvasScale remains 0.0005 and PromptPanelSize remains 700 x 260.

Decisions, assumptions, known limitations, and follow-up work:
- "A bit smaller" was interpreted as approximately 17 percent smaller than the prior size of 18.
- The background was removed completely rather than made transparent.
- No Play Mode or headset visual pass was performed; floating-text contrast against bright sonar reveals should be checked in headset.

Verification performed:
- MCP source verification confirmed the prompt Image/background setup is absent and the canvas creation ends with CanvasGroup.
- MCP source readback confirmed fontSize=15, fontSizeMin=10.5, and fontSizeMax=15.
- Unity compilation completed with scriptCompilationFailed=False and isCompiling=False.
- MainScene remained clean; no scene serialization change was required.


## 2026-07-19 - TUTORIAL-INTERACTION-END-001

Goal: Trigger the tutorial's final warning and existing audio/fade ending as soon as the player completes the button-and-lever interaction sequence.

Resulting behavior: When both Tutorial Button and Tutorial Lever have been activated in either order, TutorialDirector now enters Ending immediately, fades to the final warning prompt, holds the warning fully visible for two seconds, and then runs the existing warning fade, black screen fade, heartbeat, Entity sound, completion save, and return to MainMenuScene. The player no longer needs to approach or remain near Entity wall to trigger the ending.

Files created/moved/deleted: None.

Files modified:
- Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs
- Assets/_EchoRoom/Scripts/Tutorial/TutorialRuntimeObserver.cs
- Assets/_EchoRoom/Scenes/MainScene.unity
- Docs/PROJECT_MEMORY.md

Unity objects affected:
- Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System

Important component, asset, setting, and dependency references:
- TutorialDirector.Step no longer contains ReadWarning.
- WarningHoldDuration is 2 seconds and is applied with WaitForSecondsRealtime after SwapPrompt(WarningMessage).
- TryCompleteInteractionLesson starts EndAfterInteraction only after buttonActivated and leverActivated are both true.
- PlayEnding remains the existing ending implementation with the 2.5-second warning fade, five-second screen fade, Entity Sound, Heartbeat, TutorialProgress.Complete, and MainMenuScene load.
- TutorialRuntimeObserver no longer reflects or logs warningWall, warningReadTimer, readTimerField, or warning-wall distance.

Decisions, assumptions, known limitations, and follow-up work:
- The final warning retains a two-second fully visible reading period before the existing ending fade begins.
- Entity wall geometry remains in the tutorial prefab but is no longer an ending trigger or diagnostic dependency.
- No Play Mode or headset run was performed; the combined interaction-to-warning-to-audio timing should be confirmed in a user playtest.

Verification performed:
- MCP source verification confirmed TryCompleteInteractionLesson starts EndAfterInteraction after both interaction flags are true.
- MCP source verification confirmed EndAfterInteraction swaps to WarningMessage, waits WarningHoldDuration, and yields to PlayEnding.
- MCP source and observer verification found no remaining ReadWarning, warningWall, warningReadTimer, or readTimerField dependency.
- Unity compilation completed with scriptCompilationFailed=False, isCompiling=False, and isUpdating=False.
- MainScene was saved after the script reload and finished clean.

### MCP-MEMORY-001 — Record the Ivan Murzak Unity-MCP connection

- **Date:** 2026-07-16
- **Goal:** Make the local Ivan Murzak Unity-MCP connection settings durable and easy for future project sessions and agents to find.
- **Result:** Added a current-index connection section and active decision describing how to start the Unity-side HTTP server and which project-scoped endpoint Codex uses.
- **Files created/moved/deleted:** None.
- **Files modified:**
  - `Docs/PROJECT_MEMORY.md`
- **Unity objects affected:** None — documentation-only change; no scene or prefab object was changed.
- **Components/assets/settings:** Unity Connection window: Custom server URL `http://localhost:26566`, HTTP transport, authorization `none`; Codex project configuration: `.codex/config.toml` section `[mcp_servers.ai-game-developer]`, endpoint `http://localhost:26566/p/a679b99a`.
- **Decisions and assumptions:** The loopback endpoint is local to the machine and does not require a token. The `/p/a679b99a` suffix is the already-configured project route used by the MCP client, while the Unity window displays the base server URL. No credentials or secrets were added to project memory.
- **Verification:** Read the supplied Unity Connection screenshot, which shows `Unity: Connected`, `MCP server: Running (http)`, base URL `http://localhost:26566`, HTTP transport, and authorization `none`. Read `.codex/config.toml` and confirmed the enabled `ai-game-developer` MCP entry uses `http://localhost:26566/p/a679b99a`.
- **Known limitations:** The screenshot's orange AI agent status indicates no client was attached at the instant captured; this documentation change does not itself start Unity or establish a live client session. The project route may need to be regenerated if the Unity-MCP package/configuration is reset.
- **Follow-up:** When connecting, start the server in Unity first, then launch/reload the Codex project session and confirm the AI agent indicator becomes connected or run a Unity-MCP readiness probe.

### UI-W3-HEAD-RELATIVE-001 — Keep the loading cover centered on the active XR camera

- **Date:** 2026-07-23
- **Goal:** Complete UI work item W3 / Issue 13 by preventing the persistent loading screen from remaining at a main-menu hallway coordinate after the player is moved to a maze spawn.
- **Result:** `VRLoadingScreen` now has one placement model: while visible, it follows the currently active `Camera.main` at the configured 1.15-metre forward distance and zero height offset. The persistent screen re-resolves the destination scene camera and is correctly repositioned after scene activation and player teleport. The main menu and pause menu retain their separate fixed-placement behavior.
- **Files created:**
  - `Docs/QA/w3-head-relative-main-scene.png`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
  - `Docs/UI_FIX_PLAN.md`
  - `Docs/HANDOFF.md`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen` (runtime behavior through `VRLoadingScreen`; scene serialization unchanged)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen` (runtime behavior through `VRLoadingScreen`; scene serialization unchanged)
- **Components/assets/settings:** Removed `VRLoadingScreen.SceneAnchor`, `sceneAnchors`, `useFixedWorldPlacement`, `fixedWorldPosition`, `fixedWorldEulerAngles`, `activeSceneName`, and `ResolveSceneAnchor()`. `PlaceInFrontOfPlayer()` always uses the active camera pose, `distanceFromCamera=1.15`, `heightOffset=0`, camera rotation plus 180° yaw, and the existing `(-worldScale, worldScale, worldScale)` UI Toolkit scale. W2's persistent singleton, scene-camera re-resolution, transition watchdog, pause-menu suppression, and UI-ray suppression remain intact.
- **Decisions and assumptions:** Removed the obsolete mode instead of changing its initializer because live/source inspection confirmed both scene instances serialize the old boolean as true; serialized overrides would have defeated a default-only change. The scene files were not saved, matching W3's script-only scope. Their legacy unrecognized YAML keys are ignored by the compiled component and may be removed automatically by a future intentional Unity scene save. The W2 test invoked `VRMainMenu.BeginGameplay(0)` through a temporary runtime reflection harness instead of pressing New Game, so no existing save data was deleted or changed.
- **Verification:** Unity rebuilt `Library/ScriptAssemblies/Assembly-CSharp.dll`; at verification it was 29.4 seconds old, `EditorUtility.scriptCompilationFailed=False`, and the Editor was neither compiling nor updating. Reflection plus `SerializedObject` readback in both scenes found all obsolete placement properties absent and `distanceFromCamera=1.15`, `heightOffset=0`. Before W3, a logged W2 Play Mode run kept one loading-screen instance in `DontDestroyOnLoad` for about 5.2 seconds across `MainMenuScene` → `MainScene`, with both UI rays disabled and the pause menu closed. After W3, every visible sample before and after scene activation/player spawn measured distance `1.1500 m`, position error `0.00000 m`, and rotation error `0.0000°`; the same persistent instance survived the swap and W2 completed normally. `Docs/QA/w3-head-relative-main-scene.png` was captured in `MainScene` after activation and visually confirms panel centering. The final five-minute Console query returned zero Errors and zero Exceptions. Live Unity inspection confirmed both exact object paths above, and both scenes remained clean.
- **Known limitations:** `PENDING-HEADSET` for continuous head-tracking smoothness, possible late-update jitter, stereo comfort, and whether the 1.44 m × 0.896 m panel covers enough peripheral headset field of view. A screenshot proves centering but not motion comfort.
- **Follow-up:** Run the W3/W2 transition once on Quest while moving the head gently. Continue the plan with W4 (menu wall occlusion and viewing distance) after that comfort check, or proceed with the Editor-safe portion and retain the headset gate.

### UI-W4-MENU-OCCLUSION-DISTANCE-001 — Keep menus clear of walls and bring the main menu closer

- **Date:** 2026-07-23
- **Goal:** Complete UI work item W4 / Issues 4 and 7b by preventing the wide pause panel from clipping walls and reducing the main menu's excessive fixed viewing distance without changing the out-of-scope UI resolution.
- **Result:** Fixed and dynamic `VRPauseMenu` placement now share an oriented panel-volume cast, wall padding, and final overlap validation. Side/corner obstacles missed by the former centre ray pull the panel toward the camera. If a full-size panel has no clear depth, the menu retries at progressively smaller scales and restores its authored scale the next time it opens. The main-menu panel remains at its fixed hallway anchor but is now 1.500 m forward / 1.536 m centre-to-centre from the authored camera instead of 2.500 m forward / 2.522 m centre-to-centre. Gameplay pause distance remains 2.1 m.
- **Files created:**
  - `Docs/QA/w4-main-menu-distance.png`
  - `Docs/QA/w4-pause-clear-main-scene.png`
  - `Docs/QA/w4-pause-side-wall-main-scene.png`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRPauseMenu.cs`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/UI_FIX_PLAN.md`
  - `Docs/HANDOFF.md`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen` (obsolete W3 serialized keys normalized during the intentional scene save; runtime behavior unchanged)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen` (obsolete W3 serialized keys normalized during the intentional scene save; runtime behavior unchanged)
- **Components/assets/settings:** `VRPauseMenu.wallMask` changed from all layers (`-1`) to `Default | Echoable` (`1025`); `QueryTriggerInteraction.Ignore` remains, so trigger volumes are excluded. The live pause panel collider is 900 × 560 × 4 at `(-0.002, 0.002, 0.002)` scale, producing oriented half-extents of approximately `(0.9, 0.56, 0.005)` metres for placement. `wallPadding=0.15`, `minimumDistanceFromCamera=0.45`, and `distanceFromCamera=2.1` remain. Both `PlaceMenuAtFixedWorldAnchor()` and `PlaceMenuInFrontOfPlayer()` use the shared placement path. Main-menu `fixedWorldPosition` and the authored `Main Menu` transform changed from `(0, 1.15, -8.75)` to `(0, 1.15, -9.75)`; its dormant `distanceFromCamera` fallback changed from 2.1 to 1.5. `VRMainMenu` runtime scale remains `0.0016`, so panel resolution and physical size were not changed.
- **Decisions and assumptions:** Used the existing `Default` and `Echoable` geometry layers instead of adding a dedicated project layer and relayering every level/prefab, which would have expanded W4 substantially. The XR body collider is on `Ignore Raycast`, so mask 1025 excludes it. Pause distance was deliberately left at 2.1 m because its 1.8 m-wide panel is larger than the 1.44 m-wide main menu. The closer main-menu anchor preserves its existing height and hallway alignment. The `keepInFrontOfWalls` toggle now bypasses all cast, overlap, and scale behavior when disabled. Saving the scenes removed the W3-era `useFixedWorldPlacement`, fixed-anchor, and `sceneAnchors` YAML keys from `VRLoadingScreen`; those members no longer exist and Unity was expected to normalize them on the next intentional save.
- **Known limitations and follow-up:** Mask 1025 is explicit but not semantically geometry-only; solid props/interactables on `Default` may still shorten placement. Pause placement is world-locked after opening and is not continuously revalidated against moving geometry. The emergency fallback may reduce the panel to 10% linear scale; this avoids intersection but may be unreadable. If no clear pose exists even then, the prior pose is retained and an Error asks the player/tester to move away from the wall and reopen. The static main menu has no runtime occlusion cast; this was accepted because locomotion is disabled and the authored anchor was verified clear. `PENDING-HEADSET` for the new main-menu comfort/readability/controller reach and emergency scale-to-fit behavior.
- **Verification:** Unity rebuilt `Library/ScriptAssemblies/Assembly-CSharp.dll`; final readback reported `EditorUtility.scriptCompilationFailed=False`, the assembly was 41.8 seconds old, and reflection found `PlaceMenu`, `GetOcclusionAdjustedPosition`, and `IsPanelPositionClear`. Deterministic Play Mode cases passed for clear space, centred wall, side/corner-only wall, fixed-anchor obstruction, close wall, ignored trigger, excluded layer, and avoidance disabled; every selected panel volume had zero wall-mask overlaps. A 24-pose sweep across three player positions and eight yaw angles returned 24/24 clear volumes; deliberately cramped cross-corridor poses exercised scale-to-fit, and one extreme pose remained at 0.43 m because no scale met the 0.45 m minimum. Normal pause placement remained full-size at 2.101 m with zero overlaps. Runtime main-menu readback measured camera `(0, 0.82, -11.25)`, panel `(0, 1.15, -9.75)`, 1.500 m forward depth, 1.536 m centre distance, and runtime scale `(-0.0016, 0.0016, 0.0016)`. All three QA images were captured and visually inspected. Final Console query for the last minute returned zero Errors. Live readback confirmed both scene values and exact object paths, both scenes were clean, and `MainMenuScene` was restored active.

### UI-W5-EDITOR-AUTHORING-PARITY-001 — Make Edit Mode UI match runtime authoring

- **Date:** 2026-07-23
- **Goal:** Complete UI work item W5 / Issue 1 by making UI Toolkit styles, default visibility, document assets, dimensions, and main-menu scale available in serialized authoring data instead of depending on `Awake()`.
- **Result:** The menu and loading UXML assets now load their matching stylesheets directly. Their existing hidden-state classes therefore work outside Play Mode, replacing the former flat stack of all screens with one fully styled representative Start screen. The four scene UIDocuments now match their runtime asset/layout matrix, and MainMenuScene's authored main-menu scale matches `VRMainMenu.worldScale=0.0016`. Runtime screen switching and loading behavior remain unchanged.
- **Files created:**
  - `Docs/QA/w5-after-editmode-mainmenu.png`
- **Files modified:**
  - `Assets/_EchoRoom/Resources/UI/VRLoadingScreen.uxml`
  - `Assets/_EchoRoom/Resources/UI/VRMenu.uxml`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/UI_FIX_PLAN.md`
  - `Docs/HANDOFF.md`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
- **Components/assets/settings:** `VRMenu.uxml` embeds `VRMenu.uss`; `VRLoadingScreen.uxml` embeds `VRLoadingScreen.uss`. Both loading UIDocuments use `VRLoadingScreen.uxml`, `Position.Absolute`, fixed 900 × 560 size, centred pivot, sorting order 1000, and `VRMenuPanelSettings.asset`. Both menu UIDocuments use `VRMenu.uxml`, the same position/size/pivot/panel settings, and sorting order 100. MainScene's pause UIDocument previously referenced `VRGameplayMenu.uxml`. MainMenuScene's `Main Menu` local scale is now `(-0.0016, 0.0016, 0.0016)` while its W4 world position remains `(0, 1.15, -9.75)`.
- **Decisions and assumptions:** Used asset-first authoring and did not introduce `[ExecuteAlways]`, avoiding edit-time lifecycle behavior and scene dirtying. Existing UXML `hidden` classes and USS rules were sufficient once the stylesheets were embedded. Runtime asset/document setup remains as an idempotent fallback; complete-tree inspection confirmed it does not duplicate either stylesheet. No C#, PanelSettings resolution, or runtime menu-state logic changed. The shared menu layout exposes one representative Start screen in Edit Mode. The established negative-X/180° orientation convention remains because a positive-scale conversion previously flipped the readable face and requires separate collider/ray/headset work. `VRGameplayMenu.uxml` was not deleted.
- **Verification:** Unity imported both UXMLs with `m_ImportedWithErrors=false`, `m_ImportedWithWarnings=false`, and one linked stylesheet each. A live cross-scene Edit Mode audit opened both scene assets through Unity and verified all four exact objects, source assets, shared PanelSettings, Absolute/Fixed 900 × 560 settings, centred pivots, sorting orders, and one stylesheet reference across each full visual tree. After a real UI Toolkit repaint, Start resolved to `Flex`; Level/Warning/Pause/Settings/Captured, both loading roots, and Thank You resolved to `None`. The after-image `Docs/QA/w5-after-editmode-mainmenu.png` was captured with `EditorApplication.isPlaying=false` and visually compared with `Docs/QA/issue-01-before-editmode-stacking.png`. Play Mode tests exercised Start/Level/Settings switching, loading 0%→50%→hidden, a real MainMenuScene→MainScene transition with exactly one persistent loading owner, and Pause/Settings/Captured/Time Up/hidden cleanup. Delayed pause input activated after layout, and hiding restored collider, time, and audio. Runtime stylesheet counts remained one per document. The final fresh one-minute Console query returned zero Errors and zero Exceptions. Both scenes were read back clean, and `MainMenuScene` was restored active and clean.
- **Known limitations:** `PENDING-HEADSET` for physical controller-ray hover/trigger behavior, stereo readability, perceived scale, and transition comfort. MainScene's Edit Mode representative Start screen is not its runtime Pause state. W5 does not resolve the pre-existing negative-scale collider warning or Issue 7a's deferred 1920 × 1080 reflow.
- **Follow-up:** Run the existing W1/W4/W5 Quest checks, address the transform convention separately if controller targeting remains incorrect, and continue the UI plan with W6.

### UI-W6A-TUTORIAL-WORLD-LOCK-001 — World-lock each tutorial prompt at presentation

- **Date:** 2026-07-23
- **Goal:** Complete UI work item W6a / Issue 3 by replacing the controller-following tutorial prompt with a head-relative pose captured once whenever a message appears.
- **Result:** The existing runtime uGUI/TMP prompt is placed 0.85 m forward and 0.10 m below the resolved Main Camera when the initial prompt or a replacement message is shown, for a centre distance of approximately 0.856 m. It then retains that world pose instead of following the headset or Right Controller every frame. The Right Controller is no longer a prompt dependency. Missing-camera placement leaves the prompt hidden and emits one explicit Error. The ending fade now requires a stereo camera instead of falling back to `ScreenSpaceOverlay`. Issue 11 styling was not changed and remains pending user choice.
- **Files created:**
  - `Docs/QA/w6-issue3-world-locked-prompt.png`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs`
  - `Docs/UI_FIX_PLAN.md`
  - `Docs/HANDOFF.md`
  - `Docs/PROJECT_MEMORY.md`
  - `Logs/TutorialRuntime.log` (ignored runtime verification trace; not committed)
- **Files created and deleted during verification:**
  - `Library/W6Issue3LiveQA.txt` (ignored temporary harness result; removed after readback)
- **Files moved/deleted as part of the product change:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System/Tutorial Controller Prompt` (runtime-created)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System/Tutorial Controller Prompt/Prompt Text` (runtime-created)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial Ending Fade` (runtime-created)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Main Camera` (inspected read-only pose/camera source)
- **Components/assets/settings:** `PromptOffsetFromView` is `(0, -0.10, 0.85)`; `PromptCanvasScale=0.0005` and `PromptPanelSize=700×260` are unchanged. `ShowPrompt()` and `SwapPrompt()` call `TryPlacePromptAtHeadPose()` once before showing each message. Per-frame prompt placement and the `rightController` field/lookup were removed. `TutorialDirector.Update()` remains because it owns the `Move → Button` proximity gate. `tutorialPromptText` remains a `TextMeshProUGUI`, preserving `TutorialRuntimeObserver` reflection and `TEXT_PROMPT` diagnostics. `CreateFadeOverlay()` now uses the resolved Main Camera only and returns null with an Error when a stereo camera is unavailable.
- **Decisions and assumptions:** Full headset orientation axes are used, matching the existing camera-relative UI convention. World-locking means the player can deliberately turn away after a message appears; each subsequent message recentres from the then-current head pose. No wall cast was added, so presentation while facing very close geometry may still obscure the prompt. The 0.856 m centre distance is provisional pending headset comfort verification. The prompt remains bare floating uGUI text because Issue 11 is a separate design/architecture choice.
- **Verification:** Unity synchronous import and compilation completed with `EditorUtility.scriptCompilationFailed=False`. A deterministic temporary fixture passed exact offset/rotation/scale, eight repeated `Update()` calls without pose drift after head/controller movement, fresh placement on a later show, `Camera.main` preference, XR-rig fallback, no right-controller dependency, and fail-closed prompt/fade behavior without a camera. A real `MainScene` Play Mode run loaded `T_Junction_Tutorial` through `GameManager`; a corrected runtime harness passed initial placement, world-lock after player/head/controller motion, reanchor on `SwapPrompt()`, post-swap lock, observer TMP compatibility, the preserved `Move → Button` gate, and creation of a camera-bound `ScreenSpaceCamera` fade. The initial and swapped prompt positions were `(1.76000, 0.95000, -0.33000)` and `(2.69154, 1.03000, -0.81116)`. The saved tutorial trace reported all ten observer references `OK`, zero `[ERROR]` entries, initial Sonar text, the QA swap, `Ping → Button`, and Interaction text. `Docs/QA/w6-issue3-world-locked-prompt.png` was captured at the open tutorial start and visually inspected; it proves the Interaction prompt rendered without obvious clipping, not motion locking by itself. A first dynamic harness draft failed to compile because of harness-only omissions; no product code or serialized Unity state changed, and the corrected harness passed. The deliberate no-camera fixture emitted its expected Errors, so no blanket zero-Console-Error claim is made. Independent code, observer, and QA reviews found no Issue 3 blocker. Play Mode was stopped, both pre-test tutorial PlayerPrefs keys were restored exactly (`Status=2`, `Requested=0`), and clean `MainMenuScene` was restored active.
- **Known limitations:** `PENDING-HEADSET` for stereo comfort, apparent distance, readability, jitter/swimming, turn-away-and-return behavior, independent physical controller movement, nearby-geometry occlusion, and seated/standing reanchors. The saved run did not exercise natural Ping events, `SecondReveal`, `Move`, real button/lever completion, `Warning`/`Ending`/`Done`, ending audio, observer detach, or return to `MainMenuScene`. Rapid overlapping `SwapPrompt()` coroutines and lack of an automatic retry when the initial camera is absent and no later message occurs are pre-existing/non-blocking lifecycle risks. W6b styling and the required five-state styling screenshots remain open.
- **Follow-up:** Decide W6b / Issue 11 styling. Before or during W7, run the complete natural tutorial sequence once and confirm each prompt reanchors once, plus warning hold, ending audio, stereo fade, completion persistence, scene return, and observer detach. Run the recorded headset checks on Quest before accepting the 0.856 m distance as comfortable.

### UI-W6B-TUTORIAL-TOOLKIT-001 — Port the complete tutorial prompt to UI Toolkit

- **Date:** 2026-07-23
- **Goal:** Complete UI work item W6b / Issue 11 in UI Toolkit, per the user's explicit direction, while preserving W6a world-lock behavior and the tutorial's runtime diagnostics and full sequence.
- **Result:** The runtime tutorial prompt is now one fixed-size, noninteractive world-space `UIDocument` styled in the existing Echo Room UI Toolkit language. The former prompt Canvas, CanvasGroup, CanvasRenderer, and TextMeshProUGUI are gone. Five authored messages use separate title/body labels; long Interaction/Warning copy receives a fixed compact treatment, and Warning uses a thin danger-colored divider plus matching frame/title accents. W6a's one-shot `(0, -0.10, 0.85)` head-space placement and world locking remain. `TutorialRuntimeObserver` now consumes a typed director-owned snapshot instead of reflection/TMP and preserves its established log categories and `active/alpha/text` payload. The separate camera-bound uGUI ending fade remains unchanged.
- **Files created:**
  - `Assets/_EchoRoom/Resources/UI/VRTutorialPrompt.uxml`
  - `Assets/_EchoRoom/Resources/UI/VRTutorialPrompt.uxml.meta`
  - `Assets/_EchoRoom/Resources/UI/VRTutorialPromptStyles.uss`
  - `Assets/_EchoRoom/Resources/UI/VRTutorialPromptStyles.uss.meta`
  - `Docs/QA/w6b-01-sonar.png`
  - `Docs/QA/w6b-02-microphone.png`
  - `Docs/QA/w6b-03-move.png`
  - `Docs/QA/w6b-04-interaction.png`
  - `Docs/QA/w6b-05-warning.png`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs`
  - `Assets/_EchoRoom/Scripts/Tutorial/TutorialRuntimeObserver.cs`
  - `Docs/UI_FIX_PLAN.md`
  - `Docs/HANDOFF.md`
  - `Docs/PROJECT_MEMORY.md`
  - `Logs/TutorialRuntime.log` (ignored runtime verification trace; not committed)
- **Files moved/deleted during implementation:** The initially created uncommitted `Assets/_EchoRoom/Resources/UI/VRTutorialPrompt.uss` and `.meta` were moved to the final `VRTutorialPromptStyles.uss` names. This gives the stylesheet a unique Resources key; no committed product asset was deleted.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System/Tutorial UI Toolkit Prompt` (runtime-created and inspected live)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial Ending Fade` (runtime-created; exercised unchanged during the full ending)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Main Camera` (inspected read-only as the placement and stereo-fade camera)
- **Components/assets/settings:** The runtime prompt GameObject uses a normal `Transform`, `UIDocument`, and Play Mode-generated `UIRenderer`. It deliberately has no Canvas, CanvasGroup, TMP, collider, or XR UI manager. The document reuses `Resources/UI/VRMenuPanelSettings`, loads `Resources/UI/VRTutorialPrompt` and `Resources/UI/VRTutorialPromptStyles`, uses Fixed 700 × 260 sizing, centred pivot, Absolute positioning, and sorting order 32767, and retains the readable UI Toolkit transform convention `(-0.0005, 0.0005, 0.0005)` plus 180° yaw correction. `VRTutorialPrompt.uxml` links exactly one `VRMenu.uss` and one prompt stylesheet. The 700 × 260 root, 600 × 96 body, fixed title/body font sizes, `compact-copy`, and `warning-state` replace TMP auto-sizing/outline behavior for the five fixed messages. All visual elements use `PickingMode.Ignore`; root display/opacity controls presentation while the document GameObject remains active. `OnDisable()` stops prompt coroutines before hiding to prevent a late fade frame from re-showing a disabled director.
- **Decisions and assumptions:** The user explicitly overruled the earlier recommendation to retain uGUI/TMP because the rest of the project uses UI Toolkit. The observer, fixed-copy layout, and orientation were migrated together rather than accepting broken diagnostics or clipping. The document remains active to keep cached `VisualElement` references valid. A unique stylesheet basename is required because `Resources.Load<StyleSheet>("UI/VRTutorialPrompt")` resolved the UXML-generated inline stylesheet when the UXML and USS shared a Resources path. The first Warning design incorrectly applied shared `.danger-line` to the entire divider container; independent visual review caught the thick bar, the class toggle was removed, and the evidence was recaptured with two thin lines. No scene or prefab asset required serialization.
- **Verification:** Unity refreshed/imported the UXML and USS without import errors or warnings and compiled the final scripts. A deterministic Unity fixture verified a UI Toolkit-only prompt, fixed 700 × 260 document configuration, the shared PanelSettings reference, exactly one shared and one prompt stylesheet, ignored picking throughout the visual tree, title/body splitting, compact and warning class cleanup, the W6a offset/scale/facing, eight Update calls without pose drift, typed diagnostics, the XR-rig camera fallback, and fail-closed no-camera hiding. A final lifecycle fixture verified the corrected thin Warning divider and disable-time hiding after `StopAllCoroutines()`. A full Play Mode tutorial run used the real Sonar emission implementation, the public microphone `RequestPing` path after cooldown, the real five-metre movement gate, and the actual tutorial button/lever public APIs. It passed Sonar → Microphone → Move → Interaction → Warning, the authored warning hold, prompt fade, entity/heartbeat start, stereo ending fade, tutorial completion (`Status=2`, `Requested=0`), observer `DETACH`, and return to `MainMenuScene`. The observer trace reported all typed references OK, panel attached, every exact prompt transition, button/lever events, ending audio, no NULL prompt state, and detach. All five 1920 × 1080 captures were visually inspected; an independent QA reviewer passed the final set after the Warning recapture. Independent code and observer closure reviews found no remaining source issue. The deliberate no-camera test emitted its expected placement Error; early harness-only assumptions about Edit Mode `UIRenderer` and an arbitrary-camera fallback were corrected without product changes. Pre-test tutorial PlayerPrefs were restored exactly to `Status=2`, `Requested=0`, Play Mode was stopped, and clean `MainMenuScene` was restored.
- **Known limitations:** `PENDING-HEADSET` for stereo readability/orientation in both eyes, apparent scale and comfort at the provisional 0.856 m centre distance, jitter/swimming and turn-away behavior, reanchor comfort, physical controller-ray non-interception, nearby-geometry occlusion, seated/standing behavior, and ending-fade comfort. Interaction and Warning desktop captures are intentionally dark and prove readable prompt rendering, not corridor-clearance geometry. The decorative status line is not expected to be reliably readable at headset distance. Rapid overlapping prompt swaps and the lack of an automatic retry when the initial camera is absent and no later message appears remain non-blocking lifecycle risks. Existing unrelated XR/tooling warnings were not changed; no blanket warning-free claim is made.
- **Follow-up:** Continue with W7 timer fairness. Run the recorded W6 Quest checks before accepting physical comfort, stereo orientation, and geometry clearance.

### UI-W7-TIMER-FAIRNESS-001 — Make timed-maze pressure visible and teachable

- **Date:** 2026-07-23
- **Goal:** Complete UI work item W7 / Issue 10 by preserving the full authored countdown, surfacing escalating time warnings without removing manual reveal, and teaching the reveal control through the existing UI Toolkit tutorial prompt.
- **Result:** Maze A–D now begin at the full 180 seconds only after the loading transition clears, then show their four-flash introduction. One-shot automatic reveals fire at exact downward crossings of 60, 30, and 10 seconds for 3.0, 3.75, and 4.5 seconds. The existing heartbeat clip escalates in volume, pitch, and playback length. Long-frame threshold crossings coalesce to the most urgent cue; expiry takes priority and hides `TIME 00:00` before the time-up menu opens. Pause freezes countdown and active warning presentation together. Manual A / Gamepad South / keyboard `T` reveal remains independent. The UI Toolkit MOVE prompt now teaches the timer control. Maze E remains untimed.
- **Files created:**
  - `Docs/QA/w7-tutorial-timer-reveal.png`
  - `Docs/QA/w7-10-second-warning.png`
  - `Docs/QA/w7-time-expired.png`
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/Managers/MazeLevelTimer.cs`
  - `Assets/_EchoRoom/Scripts/Tutorial/TutorialDirector.cs`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/UI_FIX_PLAN.md`
  - `Docs/HANDOFF.md`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Maze Timer Display` (runtime-created and inspected live)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: Tutorial System/Tutorial UI Toolkit Prompt` (runtime-created and inspected live)
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Pause Menu`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: GameManager`
- **Components/assets/settings:** `MazeLevelTimer` serializes `automaticRevealSeconds=3`, `warningAudioVolume=0.65`, and `warningAudioDurationSeconds=1.1`. `warningAudioClip` references the existing `Assets/Audio/universfield-fast-heartbeat-151928.mp3` (6.896 seconds, mono, 44.1 kHz); runtime playback stops it early at 1.1/1.35/1.6 seconds with volume `0.4225/0.53625/0.65` and pitch `0.90/1.05/1.20`. The runtime `Maze Timer Display` owns a 2D, non-looping `AudioSource`. Warning masks progress `1/3/7`. Timer start waits on `VRLoadingScreen.Instance.IsTransitioning`; pause-safe warning presentation uses scaled `Time.time`, `Time.deltaTime`, and `WaitForSeconds`. The exact MOVE copy is `MOVE\nIn timed mazes, press A to reveal the timer.\nGo straight, then turn left.`
- **Decisions and assumptions:** Maze E remains untimed because giving it a duration is a separate design choice. No new tutorial step or gate was introduced; the instruction fits the existing MOVE prompt. Manual reveal does not play the warning cue or consume automatic thresholds. The existing lightweight controller readout remains TMP because it is not a menu/prompt panel and the broader objective/wrist UI conversion is explicitly deferred under Issue 12/14. The heartbeat mix is a defensible desktop starting point, not accepted headset comfort. A missing warning clip degrades to visual warnings and logs once. Re-enabling a manually disabled timer component waits for a later level-start event rather than resuming an interrupted countdown; no current gameplay path toggles the component.
- **Verification:** Unity synchronously imported and compiled the scripts with `EditorUtility.scriptCompilationFailed=False`; reflection found the final timer methods and warning fields. Live serialized readback found the exact timer component and heartbeat asset on MainScene, and the scene was clean after save. A deterministic fixture passed Maze A–D duration mapping, untimed Maze E, exact 60/30/10 crossings, no repeats, masks `1/3/7`, 65→5-second hitch coalescing to only the ten-second cue, paused countdown stability, and manual reveal isolation. Live Play Mode passed loading-cover gating, the full 180-second post-cover start, visible introduction flash, all three warnings with real `AudioSource` playback/stop and urgency escalation, expiry/menu/audio cleanup, restart, and Maze E cleanup. A focused pause regression held the 60-second warning across 2.5 real seconds at `timeScale=0`: remaining time stayed `59.00`, the full 3.00-second reveal remained, and its cue coroutine stayed pending until resume. A disable regression cancelled the delayed-start coroutine, stopped the timer, and hid the display. A natural tutorial run reached MOVE through the real sonar/microphone paths and read back the exact UI Toolkit copy; pre-test tutorial preferences were restored to `Status=2`, `Requested=0`. All three 1920×1080 images were inspected. Independent visual review caught the original expiry readout overlapping Restart; the product was fixed, the image recaptured, and the reviewer passed it. Independent code review caught pause-consumed warning and disable-race cases; both were fixed and re-reviewed with no blocker. The compressed-audio `GetData` probe and an early assumption that `AudioSource.clip` was assigned before first warning produced harness-only errors, so no blanket zero-Console-error claim is made for the entire work session. Play Mode was stopped, `MainScene` was unloaded cleanly, and clean `MainMenuScene` was restored active.
- **Known limitations:** `PENDING-HEADSET` for physical A-button mapping, stereo/controller-distance readability, moving-hand occlusion, and warning-cue loudness/comfort. Maze E timing remains an open design choice. Existing unrelated XR/tooling warnings were not changed.
- **Follow-up:** Complete W9 interaction-layer reconciliation and rename-safe reference cleanup, then run the consolidated Quest checklist.

### UI-W9-INTERACTION-REFERENCE-CLEANUP-001 — Reconcile UI picking layers and remove rename-fragile scans

- **Date:** 2026-07-23
- **Goal:** Complete UI work item W9 by reconciling the two scenes' UI Toolkit world-picking masks and replacing UI/timer GameObject-name searches with explicit or type-safe references that remain correct across scene loads.
- **Result:** Both scene EventSystems now use `4294967291` / `0xFFFFFFFB` (`Physics.DefaultRaycastLayers`) for `PanelInputConfiguration.m_InteractionLayers`. `VRMainMenu` serializes exact Right then Left Menu UI Ray GameObjects. `MazeLevelTimer` serializes its own Right Controller transform and the XR Main Camera. The old `"Menu UI Ray"` and `"Right Controller"` full-Transform scans are gone. The persistent loading screen keeps only a typed scan for active `XRRayInteractor`s whose `enableUIInteraction` is true, clearing unloaded refs and rescanning the destination scene. Normal async loads let the loader own capture/suppression/restoration; the menu's exact refs handle initialization and the no-loader synchronous fallback.
- **Files created:** None.
- **Files modified:**
  - `Assets/_EchoRoom/Scripts/UI/VRMainMenu.cs`
  - `Assets/_EchoRoom/Scripts/UI/VRLoadingScreen.cs`
  - `Assets/_EchoRoom/Scripts/Managers/MazeLevelTimer.cs`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity`
  - `Assets/_EchoRoom/Scenes/MainScene.unity`
  - `Docs/UI_FIX_PLAN.md`
  - `Docs/HANDOFF.md`
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None. `Assets/_EchoRoom/Scripts/UI/VRFrontEndMenu.cs`, `Assets/_EchoRoom/Resources/UI/VRGameplayMenu.uxml`, and `Assets/_EchoRoom/Scripts/Controller/EchoPuzzleController.cs` were deliberately not changed or deleted.
- **Unity objects affected:**
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: EventSystem`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: Main Menu`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainMenuScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: EventSystem`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: VR Loading Screen`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Right Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Left Controller/Menu UI Ray`
  - `Assets/_EchoRoom/Scenes/MainScene.unity :: XR Origin (XR Rig)/Camera Offset/Main Camera`
- **Components/assets/settings:** `UnityEngine.UIElements.PanelInputConfiguration.m_Settings.m_InteractionLayers` is `4294967291` in both scenes. This is a physics `LayerMask` for UI Toolkit world picking, not `XRInteractionLayers`; current panel layer 0 remains included and `Ignore Raycast` remains excluded. MainMenu's `VRMainMenu.vrPointerRoots` contains GameObjects `1102790405` then `1779060063` (Right/Left). MainScene's `MazeLevelTimer.rightController` references Transform `508977651`; `head` references Transform `1790717474`. Both rays are `XRRayInteractor`s with UI interaction enabled. Saving both scenes through Unity removed stale serialized `uiPointerFallbackName` and `rightControllerName` keys.
- **Decisions and assumptions:** Fixed pointer refs belong on the scene-local main menu, but not on the `DontDestroyOnLoad` loading screen because destination scene objects are destroyed and recreated. The loader therefore retains a typed active-object scan and never matches literal object names. Missing/partial main-menu refs log a clear Error once and do not silently discover replacements. The timer's rename-safe compatibility fallbacks are its own `transform` and `Camera.main`. Normal loader-driven transitions no longer pre-hide the main-menu rays, allowing the loader to record and restore them if a load fails or the watchdog ends; the synchronous no-loader fallback still hides the exact refs. W9 does not alter the rays' XR Interaction Layer masks, delete unused assets, or expand into Issue 7a / Issue 12 / Issue 14.
- **Verification:** Unity synchronously imported and compiled the final scripts with `EditorUtility.scriptCompilationFailed=False`; reflection found the new serialized fields and confirmed both removed string fields absent. Live serialized readback found one EventSystem panel configuration per scene, both masks `4294967291`, exact Right/Left menu refs in that order, and exact timer self/head refs; both scenes saved clean. Source and saved-scene searches found no targeted object-name literals or stale fields. In Play Mode, both rays, the Right Controller, and Main Camera were deliberately renamed while decoys used the old names. Menu hide/show, loader suppress/restore, decoy non-interference, and timer reference stability all passed; original names and transient decoys were restored/removed. Real `MainMenuScene → MainScene → MainMenuScene` Single-mode transitions completed with the persistent loader hidden and non-busy at arrival, MainScene UI rays inactive, both destination menu refs active, timer refs exact, and the reconciled mask intact. A persistent observer repeated both transitions: started/completed `2/2`, observed `2,121` transition frames, maximum active UI-enabled rays `0`, violation frames `0`, and both final menu refs active. Independent source, scene, architecture, and QA reviews found no blocker. After clearing diagnostic noise, a final focused pointer smoke left the Unity Console count at `0`, `scriptCompilationFailed=False`, and `isCompiling=False`. Earlier W9 diagnostic scripts had harness-only compile/property-inspection errors, so this is a fresh final-state claim rather than a claim that the complete historical Console was pristine. Play Mode was stopped and clean `MainMenuScene` was restored as the only open active scene.
- **Known limitations:** `PENDING-HEADSET` for physical hover/trigger behavior in both menus and any transition-frame pointer flash visible only in Quest. Main-menu refs fail loudly instead of supporting legacy automatic name discovery. Timer re-enable behavior remains as documented in W7. The deferred unused/duplicate assets remain in the project.
- **Follow-up:** Run the consolidated Quest checklist in `Docs/HANDOFF.md` §6. All in-scope W0–W9 code work is complete; do not begin Issue 7a or Issues 12/14 without renewed user direction.

### DOC-QUEST-INTERACTIVE-CHECKLIST-001 — Add a self-saving headset QA checklist

- **Date:** 2026-07-23
- **Goal:** Give the user a directly interactive checklist for the physical Quest and Unity Editor acceptance checks instead of requiring them to edit chat checkboxes manually.
- **Result:** Added one standalone, responsive HTML checklist that opens locally without installation or a server. It contains 55 Quest/Editor checks grouped into seven sections. Every item has Pass/Fail/Skip controls and a note field. Progress, section counts, result filters, tester/build/device metadata, automatic browser-local saving, reset confirmation, clipboard copy, and downloadable Markdown results are included.
- **Files created:**
  - `Docs/QUEST_TEST_CHECKLIST.html`
- **Files modified:**
  - `Docs/PROJECT_MEMORY.md`
- **Files moved/deleted:** None.
- **Unity objects affected:** None — documentation/QA utility only.
- **Components/assets/settings:** The checklist is dependency-free HTML/CSS/JavaScript and makes no network requests. Device-local state uses the browser `localStorage` key `echo-room-vr-quest-checklist-v1`. The default build reference is `Development-Phase / 8f69007`. Export produces `echo-room-quest-results-YYYY-MM-DD.md` for direct handoff to the lead session.
- **Decisions and assumptions:** Kept the tool local because the user asked for a file, not a hosted application. Used a single portable file so it can be opened by double-clicking and shared independently of Unity. Pass/Fail/Skip buttons can be toggled back to unmarked. Quest tests and the separate Editor-only XR Device Simulator checks are clearly distinguished. State remains local to the browser/profile used to open the file; clearing browser site data or using a different browser does not transfer saved progress, so Markdown export is the durable handoff.
- **Verification:** Extracted JavaScript passed `node --check`; the file contains all 55 authored test rows, has zero external HTTP references, and `git diff --check` passed. Browser interaction/visual QA was not performed because it was not requested; the file uses standard current-browser APIs with a clipboard fallback.
- **Known limitations:** A browser may treat each moved local-file path as a separate storage origin, so keep the file in its project location while testing. The checklist cannot itself inspect the headset or Unity; results are user-entered. Clipboard behavior depends on browser permissions, while Markdown download remains available.
- **Follow-up:** Open `Docs/QUEST_TEST_CHECKLIST.html`, complete the build checks, download the Markdown report, and send it to the lead session for triage.
