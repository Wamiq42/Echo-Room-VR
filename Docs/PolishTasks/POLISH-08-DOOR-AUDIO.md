# POLISH-08 — Door Open/Close Audio Wiring

## Goal

Ensure shipped gameplay doors play spatial open and close sounds through their existing `Door`
audio support.

## Verified current state

- Reusable clips already exist at:
  - `Assets/_Third Party Assets/Free Wood Door Pack/Audio/Door_Open.wav`
  - `Assets/_Third Party Assets/Free Wood Door Pack/Audio/Door_Close.wav`
- `Door_3_Brown.prefab` already references those clips.
- Serialized Maze A-D `Door` components have AudioSources but null open/close clips.
- Maze E and tutorial applicability must be established by live Unity inspection.

## Scope

- Every shipped maze/tutorial prefab containing the project `Door` component
- Door AudioSource spatial settings where currently unsuitable
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Find all relevant `Door` components through Unity, including nested prefab instances.
2. Assign existing open and close clips and a valid AudioSource.
3. Use appropriate 3D spatial settings and restrained volume; do not create one AudioSource per
   frame or duplicate playback components.
4. Preserve animator references, door state, puzzle wiring, and unrelated prefab overrides.
5. Document levels with no closable door rather than inventing objects.

## Acceptance and verification

- MCP readback confirms every shipped `Door` has valid source/open/close references.
- Opening plays once; closing plays once where closing is supported.
- No sound plays merely from initializing the closed state.
- Prefabs save cleanly and Console checks show no new errors.
- Final loudness/spatial feel requires headset listening.
