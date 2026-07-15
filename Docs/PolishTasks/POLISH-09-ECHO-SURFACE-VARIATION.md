# POLISH-09 — Echo Variation by Surface and Size

## Goal

Make directional echoes communicate surface character through pitch and loudness while preserving
the stable core ping loop.

## Dependencies

POLISH-02 must finish first because both tasks change `PingEmitter.cs` and reveal/echo behavior.

## Scope

- New surface metadata/configuration under `Assets/_EchoRoom/Scripts/Sonar/`
- `PingEmitter.cs`
- `EchoSoundController.cs`
- Deliberately tagged representative surfaces in shipped prefabs
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Support at least metal, concrete, wood, glass, fabric/absorptive, and a safe default.
2. Configure pitch and volume before playback through an explicit `EchoSoundController` API; avoid
   racing its `Start()` method or playing the AudioSource twice.
3. Treat authoring volume as a multiplier on the prefab's base volume, not as an absolute value.
4. Include a documented size contribution derived conservatively from hit collider/renderer bounds,
   or rename the feature if size scaling is intentionally deferred.
5. Clamp pitch/volume to comfortable ranges and preserve 3D rolloff.
6. Surfaces without metadata must retain the current default sound.
7. Avoid tagging every maze object manually until representative materials prove the mapping works.

## Acceptance and verification

- Representative metal, concrete, and wood hits produce clearly different but non-cartoonish echoes.
- Untagged surfaces have no regression.
- Echo instances play once and self-destruct once.
- Compile/Console checks are clean; spatial mix and intelligibility require headset listening.
