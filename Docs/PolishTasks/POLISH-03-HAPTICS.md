# POLISH-03 — Haptic Feedback Foundation

## Goal

Add useful, restrained controller feedback for sonar emission, physical interactions, wall touch,
and Entity proximity.

## Scope

- New reusable haptic service/helper under `Assets/_EchoRoom/Scripts/Utility/`
- `Assets/_EchoRoom/Scripts/PingEmitter.cs`
- `Assets/_EchoRoom/Scripts/EchoButtonInteractable.cs`
- The active lever interaction script found by code and Unity inspection
- `Assets/_EchoRoom/Scripts/AI/PingAttractedEntity.cs`
- The hand/wall collision path found by inspection
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Start with: ping 0.3/0.15 s, button 0.5/0.1 s, lever 0.4/0.2 s.
2. Use the interacting hand for button/lever/wall feedback when the interaction data reliably
   identifies it. Use both hands only as an explicitly documented fallback.
3. Cache or refresh devices on connection changes; do not enumerate and allocate every frame.
4. Entity proximity should rise with closeness, be rate-limited to a controlled cadence, and stop
   promptly when out of range, captured, paused, disabled, or destroyed.
5. Never send a 0.1-second impulse every rendered frame.
6. Clamp amplitude/duration and fail safely when a device has no impulse support.
7. Preserve gameplay logic and existing audio behavior.

## Acceptance and verification

- Compile and Console checks are clean.
- A diagnostic or Play Mode check proves each event requests the intended haptic pattern without
  per-frame spam.
- Real tactile strength and comfort remain headset-only acceptance checks.
