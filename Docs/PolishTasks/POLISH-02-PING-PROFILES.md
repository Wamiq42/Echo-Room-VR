# POLISH-02 — Button and Microphone Ping Profiles

## Goal

Give the basic control ping and microphone shout distinct, explicit profiles. Start with the GDD
values of 10 m / 2.5 s for basic ping and 16 m / 4 s for microphone ping.

The microphone path is push-to-talk: hold the left controller Y button while speaking. The
microphone must not capture while the button is released, and one hold may request at most one
microphone ping. Editor testing uses the V key.

## Verified current state

- Both paths enter `PingEmitter.EmitPing` and share `_nextPingTime`.
- `maxEchoDistance` is currently 20 m and only limits the directional audio spherecast.
- Visible reveal range is controlled in `EchoSonarReveal.shader` by material `_RevealRadius`
  (default 8 m), not by `PingEmitter.maxEchoDistance`.
- The global pulse buffer currently contains only origin and start time, so input-specific visual
  range cannot be produced by temporarily changing `maxEchoDistance`.

## Scope

- `Assets/_EchoRoom/Scripts/PingEmitter.cs`
- `Assets/_EchoRoom/Scripts/MicPingTrigger.cs`
- `Assets/_EchoRoom/Scripts/Controller/SonarRevealController.cs`
- `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader`, only if required for per-pulse range
- Relevant XR rig prefab/scene serialization after live inspection
- `Docs/PROJECT_MEMORY.md`

## Requirements

1. Introduce explicit serialized basic and microphone profile values for visual range, echo cast
   distance, and cooldown.
2. Avoid temporarily mutating shared fields during an emission.
3. Preserve `LastPingInputSource` tutorial behavior.
4. Pass input-specific reveal range to the shader per pulse; do not globally change all active
   pulses when one new ping is emitted.
5. Define cooldown interaction clearly. A microphone ping must not allow immediate button bypass,
   and repeated microphone input must respect four seconds.
6. Return an accurate wait duration to `MicPingTrigger`.
7. Keep editor Space/controller paths classified as `SonarControl`.
8. Start microphone capture only while left-controller Y is held, stop it immediately on release,
   and require a fresh hold for another microphone ping attempt.

## Acceptance and verification

- Button reveal stops near 10 m and mic reveal near 16 m on equivalent reveal materials.
- Button cooldown is 2.5 s; mic cooldown is 4 s.
- Tutorial first-ping source gating still works.
- Holding Y (or Editor V) enables capture; releasing it ends capture; sustained speech cannot emit
  more than one ping per hold.
- Shader reports supported with zero compilation messages.
- Unity compilation and focused Play Mode checks produce no new errors.
