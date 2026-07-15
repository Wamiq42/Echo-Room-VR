# POLISH-06 — Comfort Vignette Option

## Goal

Add an optional tunneling vignette during artificial smooth movement and turning.

## Dependency

POLISH-04 must provide the persisted vignette setting.

## Verified asset

XRI 3.3.1 Starter Assets already include
`Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/TunnelingVignette/` and its prefab.

## Scope

- Vignette prefab integration with the XR camera rig
- New settings bridge/controller if needed
- XR rig prefab serialization through Unity MCP
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Reuse the installed XRI 3.3.1 vignette assets unless inspection identifies an incompatibility.
2. Drive aperture from actual smooth movement/turning, not merely from an enabled component.
3. Do not vignette while stationary or during teleport travel unless deliberately justified.
4. Respect the setting immediately and on startup.
5. Ensure UI, hands, and controller pointers remain readable and the effect renders correctly in
   both eyes.

## Acceptance and verification

- On: vignette closes during smooth locomotion/turning and opens when motion stops.
- Off: no vignette is visible.
- No material/shader errors or duplicate camera overlays.
- Final stereo rendering and comfort require headset verification.
