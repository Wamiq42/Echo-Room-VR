# Blender side — code that works (lab_blender_org `execute_blender_code`)

Blender is **5.1**. All code runs via `mcp__Blender__execute_blender_code` (assign a dict to `result` to return data).

## Pivot / hinge origin (for moving parts)

A moving part (e.g. lever handle) must be its **own object** with its **origin at the rotation point**, so Unity
can rotate the transform cleanly. Use the 3D cursor + `origin_set`:

```python
import bpy
sc = bpy.context.scene; vl = bpy.context.view_layer
h = bpy.data.objects["Lever_Handle"]
sc.cursor.location = (0, 0, 0.30)          # the hinge point in world space
bpy.ops.object.select_all(action='DESELECT'); h.select_set(True); vl.objects.active = h
bpy.ops.object.origin_set(type='ORIGIN_CURSOR'); sc.cursor.location = (0, 0, 0)
h.rotation_mode = 'XYZ'
```

## Separating a part out of a mesh (avoid the "grabs everything" bug)

Edit Mode opens with everything selected, so you MUST deselect first, then select only your verts (e.g. by a
geometric rule — radius from an axis, height threshold), flush, then separate:

```python
import bmesh
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='DESELECT')          # <-- critical
bm = bmesh.from_edit_mesh(g.data); bm.verts.ensure_lookup_table()
for v in bm.verts:
    w = mw @ v.co
    v.select_set(<your geometric test on w>)
bm.select_flush(True); bmesh.update_edit_mesh(g.data)
bpy.ops.mesh.separate(type='SELECTED'); bpy.ops.object.mode_set(mode='OBJECT')
```

After separating multi-piece props, weld + fill + recalc to clean the cut:
`bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=0.0015)`, then `mesh.select_non_manifold()` →
`mesh.fill_holes(sides=0)`, then `mesh.normals_make_consistent(inside=False)`.

## UV smart-unwrap (needs a VIEW_3D context override)

UV ops require a 3D viewport context. Grab one and override:

```python
area3d = region3d = None
for a in bpy.context.screen.areas:
    if a.type == 'VIEW_3D':
        area3d = a
        for r in a.regions:
            if r.type == 'WINDOW': region3d = r
        break

def smart_uv(obj):
    bpy.ops.object.mode_set(mode='OBJECT'); bpy.ops.object.select_all(action='DESELECT')
    bpy.context.view_layer.objects.active = obj; obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    with bpy.context.temp_override(area=area3d, region=region3d):
        bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=0.02)
    bpy.ops.object.mode_set(mode='OBJECT')
```

In materials, drive image textures from `TexCoord.UV → Mapping(scale) → Image (projection='FLAT')`.
Box projection (`'BOX'` + Object coords) looks great but does NOT export — only use it for in-Blender previews.

## FBX export (Unity-ready)

```python
bpy.ops.export_scene.fbx(
    filepath=fbx_path, use_selection=True, object_types={'MESH','EMPTY'},
    apply_scale_options='FBX_SCALE_ALL', bake_space_transform=False,   # False for animated meshes
    mesh_smooth_type='FACE', path_mode='COPY', embed_textures=False,
    axis_forward='-Z', axis_up='Y',
    bake_anim=True, bake_anim_use_all_actions=True, add_leaf_bones=False)
```

- FBX → `Assets/_EchoRoom/Models/`. Editable `.blend` → `_BlenderSource/` **outside `Assets/`**
  (`bpy.ops.wm.save_as_mainfile`) so Unity doesn't import the .blend as a duplicate model.
- `bake_anim_use_all_actions=True` exports every Action as its own Unity clip. Set `bake_anim=False` to ship no
  animation (e.g. when the user wants to animate in Unity themselves — the pivot is enough).

## Rendering a preview (then CLEAN UP)

The user never opens Blender, so any camera/lights you add for a render must be deleted immediately after
(see [[blender-keep-scene-clean]]). Pattern: add temp `_pc` camera + `_l` area lights → set
`scene.render.engine='BLENDER_EEVEE'` → render to a PNG under `Assets/_EchoRoom/Design/` → remove the helper
objects and purge orphan camera/light datablocks. Read the PNG to inspect.

## Blender 5.1 API gotchas

- `Action.fcurves` was removed (layered actions). Don't iterate it. `obj.keyframe_insert("rotation_euler", frame=n)`
  still works and defaults to smooth interpolation.
- Eevee engine id is `'BLENDER_EEVEE'` (not `'BLENDER_EEVEE_NEXT'`).
- `scene.view_layer` doesn't exist — use `bpy.context.view_layer`.
- Principled BSDF inputs are `"Base Color"`, `"Metallic"`, `"Roughness"`, `"Emission Color"`, `"Emission Strength"`,
  `"Normal"`. MixRGB is legacy — prefer `ShaderNodeValToRGB` (ColorRamp) / `ShaderNodeMix` / `ShaderNodeMapRange`.
