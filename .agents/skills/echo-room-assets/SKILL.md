---
name: echo-room-assets
description: >-
  The end-to-end pipeline for making textured 3D props/assets for the Echo Room VR project — modeling in Blender,
  getting real PBR textures (free Poly Haven via script + the in-project ADG wall set), UV-unwrapping, exporting FBX,
  and wiring up Unity URP materials. USE THIS whenever the task involves creating, texturing, or importing a 3D
  asset/prop/model for Echo Room (levers, doors, maze pieces, props), getting or applying textures/materials, going
  from Blender to Unity, or setting up URP materials — even if the user doesn't name "the pipeline". It captures where
  the textures live and the exact working code, so don't re-derive it from scratch.
---

# Echo Room — Asset & Texture Pipeline

Proven workflow for a realistic, Quest-ready prop (built and validated on the dungeon **Lever**).
Two MCP servers are involved: **Blender** (`mcp__Blender__execute_blender_code`, the `lab_blender_org`
extension — there is NO Poly Haven N-panel add-on) and **Unity** (`ai-game-developer`, the project is **URP**).

## The 6 stages

1. **Model in Blender** — build from primitives, clean + low-poly for Quest. Separate any **moving part**
   (e.g. a lever handle) into its own object and set its **origin at the pivot/hinge** (`origin_set` with the
   3D cursor at the hinge) so it rotates correctly in Unity. AI-generated meshes (Hunyuan/Meshy) are fine as a
   *look reference* but import shattered/high-poly/pixelated — hand-building clean usually wins for VR.
2. **Get real textures** — two sources (see `references/textures.md` for IDs + the download code):
   - **Poly Haven** (free CC0) downloaded **directly via Python `urllib`** inside Blender — no add-on needed.
   - **ADG_Textures** already in the project: `Assets/ADG_Textures/walls_vol1/` (wall01–18, full PBR). Use these
     for maze walls and to match props to the maze (the lever's stone base uses `wall03`).
3. **UV smart-unwrap** every object — textures must map via UV to survive FBX export (box/triplanar projection
   does NOT export). See `references/blender.md`.
4. **Export FBX** to `Assets/_EchoRoom/Models/`, and save the editable `.blend` to `_BlenderSource/`
   **outside `Assets/`** (so Unity doesn't import the .blend as a duplicate model). See [[blender-keep-scene-clean]].
5. **Unity URP materials** — the FBX only carries Diffuse + Normal, so roughness/metallic are set in Unity.
   Run a C# script through the Unity MCP to create URP Lit materials and remap them onto the FBX slots.
   Full working script in `references/unity.md`.
6. **Verify** — read back the renderer's `sharedMaterials` (BaseMap/BumpMap/_Metallic), and/or render with
   `screenshot-isolated` on a scene instance (asset-only screenshots come back blank — instantiate first).

## Where to read next

- `references/blender.md` — Blender code: pivot/origin, UV smart-project (needs a VIEW_3D context override),
  FBX export flags, and Blender 5.1 gotchas.
- `references/textures.md` — exact texture IDs used, the Poly Haven download function, and the ADG set layout.
- `references/unity.md` — the C# `script-execute` that sets NormalMap import type, builds URP Lit materials, and
  remaps them onto the FBX (`AddRemap` + `SaveAndReimport`).

## Gotchas that cost time (read before repeating work)

- **Box/triplanar projection looks great in Blender but does NOT export** — always UV-unwrap for Unity.
- **FBX materials only carry Diffuse + Normal.** Metallic/roughness MUST be set in Unity (stage 5), not relied on
  from the FBX.
- **Set normal textures to `NormalMap` import type in Unity** or normals render wrong.
- **`bake_space_transform=False`** when exporting an **animated** mesh (Blender's "Apply Transform" is broken with
  animation). For static meshes either is fine, but be consistent.
- **Blender 5.1 changed the Action API** — `action.fcurves` no longer exists; don't iterate it. `keyframe_insert`
  still works; default interpolation is already smooth.
- **Separating a mesh by selection:** Edit Mode opens with *everything selected*, so a naive `mesh.separate` grabs
  the whole mesh. Always `mesh.select_all(action='DESELECT')` first, then select your verts.
- **Keep the Blender scene clean** — the user only works in Unity; delete render-helper cameras/lights right after
  rendering (see [[blender-keep-scene-clean]]).
- **`screenshot-isolated` on a bare asset returns blank** — instantiate the prefab into the scene first, screenshot
  by name, then clean up the instance.
- **Unity Play Mode blocks scene edits** (`MarkSceneDirty`/instantiate won't persist) — must be in Edit Mode.
