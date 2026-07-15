# POLISH-04 — Settings UI, Persistence, and Turning Controls

## Goal

Add an in-headset settings screen accessible from both main and pause menus, with persistent
preferences and working snap/smooth turning controls.

## Verified current state

- Shared UI files are `Assets/_EchoRoom/Resources/UI/VRMenu.uxml` and `.uss`.
- The XR rig prefab already contains snap and continuous turn providers, currently serialized
  disabled, with starting values of 45 degrees and 60 degrees/second.
- No settings screen or settings service exists.

## Scope

- Shared UXML/USS
- `VRMainMenu.cs` and `VRPauseMenu.cs`
- New settings model/controller scripts under `Assets/_EchoRoom/Scripts/UI/`
- XR rig turning integration under `Assets/_EchoRoom/Scripts/Controller/`
- XR rig prefab and relevant menu scene objects after MCP inspection
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Add settings entry buttons to main and pause screens plus a shared settings screen and Back.
2. Persist namespaced values for locomotion mode, turn mode, vignette, height offset, and turn
   speed. Preserve room for future master/music/SFX/subtitle controls.
3. Make Back return to the screen that opened Settings.
4. Apply snap versus smooth turning and turn speed immediately, with snap as the comfort default.
5. Avoid static-event leaks; subscribe/unsubscribe symmetrically.
6. Keep settings readable even before a settings UI component has been instantiated.
7. Do not implement teleport, vignette rendering, or height movement here; expose stable settings
   consumed by POLISH-05/06/07.
8. Ensure the pause menu remains paused while browsing Settings.

## Acceptance and verification

- Both menus open Settings and return correctly.
- Snap/smooth selection actually enables the correct provider and persists across restart.
- Turn speed changes apply to the continuous provider.
- UI fits the existing 900x560 world-space document and works with controller pointers.
- Unity compile, Console, and focused Play Mode checks are clean.
