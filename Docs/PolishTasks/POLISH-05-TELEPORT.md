# POLISH-05 — Teleport Locomotion Option

## Goal

Provide functional teleport locomotion as a persistent alternative to smooth movement.

## Dependency

POLISH-04 must provide the persisted locomotion setting and change notification.

## Scope

- New locomotion mode controller under `Assets/_EchoRoom/Scripts/Controller/`
- XR rig prefab providers/interactors/visuals
- Teleportable maze and tutorial surfaces, preferably via deliberate floor objects/layers rather
  than adding components indiscriminately to every collider
- Input Action configuration if required
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Use XRI 3.3.1 locomotion APIs and the rig's existing mediator architecture.
2. Smooth mode enables current movement and hides/disables teleport affordances.
3. Teleport mode disables smooth movement/sprint input and enables a usable teleport ray/arc.
4. Only valid walkable floor surfaces accept teleport; walls, props, doors, and hazards reject it.
5. Preserve player orientation and CharacterController/XROrigin integrity.
6. Apply the persisted choice on scene start and immediately when changed.

## Acceptance and verification

- Both modes can be selected repeatedly without duplicate providers or stuck input.
- Every shipped maze and tutorial has sufficient valid destinations and no obvious escape points.
- Preference survives scene changes/restart.
- Compile/Console checks are clean; final comfort and arc usability require headset testing.
