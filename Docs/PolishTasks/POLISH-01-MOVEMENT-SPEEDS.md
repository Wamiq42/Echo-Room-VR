# POLISH-01 — Movement Speed Correction

## Goal

Set smooth locomotion to the GDD starting values: 2.0 m/s walking and 3.5 m/s sprinting.

## Verified current state

- `DynamicSprintController.cs` defaults are 1.5 and 3.0.
- `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab` serializes overrides of 1.0 and 3.0,
  so changing source defaults alone will not alter runtime behavior.

## Scope

- `Assets/_EchoRoom/Scripts/Controller/DynamicSprintController.cs`
- `Assets/_EchoRoom/Prefabs/XR Origin (XR Rig).prefab`
- Any live scene instance that independently overrides those fields, only if Unity inspection
  proves such an override exists.
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Change source defaults to `normalSpeed = 2f` and `sprintSpeed = 3.5f`.
2. Through Unity MCP, set the authoritative prefab component to 2.0 and 3.5.
3. Inspect instantiated XR rigs for extra overrides; do not invent or assume object paths.
4. Do not change movement input, acceleration, turning, or sprint activation behavior.

## Acceptance and verification

- Unity finishes compiling with no new errors.
- MCP readback shows 2.0/3.5 on the actual `DynamicSprintController` component.
- If practical, Play Mode confirms the move provider switches between exactly 2.0 and 3.5.
- Headset comfort/feel remains a user acceptance check.
