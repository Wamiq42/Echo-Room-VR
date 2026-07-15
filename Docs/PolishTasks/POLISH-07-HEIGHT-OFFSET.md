# POLISH-07 — Seated Height Offset

## Goal

Let seated and standing players adjust viewpoint height without corrupting locomotion origin state.

## Dependency

POLISH-04 must provide the persisted height setting.

## Scope

- New height settings bridge under `Assets/_EchoRoom/Scripts/Controller/`
- XR rig prefab serialization through Unity MCP
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Modify the `XROrigin.CameraFloorOffsetObject` or the supported XROrigin offset mechanism, not
   the tracked Camera and not the XR Origin root transform.
2. Capture and preserve the rig's baseline offset; apply the preference relative to it.
3. Avoid cumulative drift when the setting is applied repeatedly.
4. Use a conservative clamped range such as -0.5 to +0.5 m.
5. Reapply correctly after tracking-origin changes and scene reloads.
6. Confirm hands, CharacterController, interactions, menus, and timer display remain aligned.

## Acceptance and verification

- Repeated slider changes are deterministic and do not move the world origin.
- Preference persists and applies on startup.
- Seated interaction reach and floor relationship receive a headset/manual check.
- Compile and Console checks are clean.
