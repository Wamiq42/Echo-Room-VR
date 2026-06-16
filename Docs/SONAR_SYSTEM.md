# Echo Room VR — Sonar System

> **Read this first when working on the sonar / echolocation feature.**
> It is the canonical reference for how the reveal effect is built, how the pieces
> connect, how to tune it, and what is still open. You should not need to search the
> whole project — everything about the sonar lives here or is linked from here.

---

## 1. Design intent

The core loop is **PING → REVEAL → REMEMBER**:

- The room is **dark** by default (surfaces are near‑black).
- The player **pings** (controller button, microphone noise, or `SPACE` in the editor).
- An **expanding ring** travels outward from the player's body. As it passes a surface,
  that surface **lights up in its real texture colour**, brightest near the player and
  fading with distance.
- The reveal **lingers briefly, then fades back to black**, so the player must navigate
  the maze from memory between pings.

Visual references the effect is modelled on (videos live in the user's `Downloads`):
Dying Light 2 "Survivor Sense" (body‑centred expanding wave) and the "Color Minis Sonar
Shader Reel" (surfaces reveal their real texture from a point in a dark room).

---

## 2. The active system at a glance

| Concern | File / Asset | Role |
|---|---|---|
| Visual (shader) | `Assets/_EchoRoom/Shaders/EchoSonarReveal.shader` | Draws the expanding ring + the revealed surface. URP **unlit**, `Cull Off`. |
| Driver | `Assets/_EchoRoom/Scripts/Controller/SonarRevealController.cs` | Records each ping as a pulse and uploads it to the shader. |
| Ping source | `Assets/_EchoRoom/Scripts/PingEmitter.cs` | Detects the ping (button / mic / SPACE), fires `OnPingEmitted`, handles echo audio + flash light. |
| Mic input | `Assets/_EchoRoom/Scripts/MicPingTrigger.cs` | Loud mic noise → requests a ping. |
| Glow | `Assets/DefaultVolumeProfile.asset` (Bloom) + camera post‑processing | Makes the HDR ring/edge bloom. Required for the look. |
| Revealable surfaces | Materials using `EchoRoom/EchoSonarReveal` | Any surface that should reveal must use this shader (see §7). |

**Data link:** the driver and shader communicate through a single **global shader array**
`_SonarPulses` (no per‑object wiring). Every material that uses the shader reacts to the
same pings automatically — including objects spawned at runtime.

---

## 3. Data flow — one ping, end to end

```
Player input (button / mic / SPACE)
        │
        ▼
PingEmitter.EmitPing()                         (PingEmitter.cs)
   • OverlapSphere → IEchoInteractable.OnPingHit()  (interactables flash/sound)
   • plays ping sound + flash light
   • EmitDirectionalEcho() → delayed echo sound at the bounce point
   • fires  OnPingEmitted(origin)   ← origin = player/controller world position
        │
        ▼
SonarRevealController.HandlePing(origin)        (SonarRevealController.cs)
   • writes { origin.xyz, startTime } into a 16‑slot ring buffer
   • Shader.SetGlobalVectorArray("_SonarPulses", buffer)
        │
        ▼
EchoSonarReveal.shader  (runs every frame on every revealable surface)
   • for each active pulse: radius = (now − startTime) × ExpandSpeed
   • bright ring at distance ≈ radius   (the travelling circle)
   • behind the ring: reveal real texture, fade by distance + time
   • everything fades to the dark base as the pulse ends
        │
        ▼
URP Bloom (DefaultVolumeProfile) → the ring/edges glow
```

The shader derives the radius from elapsed time on the GPU, so the CPU only writes **once
per ping** (no per‑frame work, no renderer caching).

---

## 4. Components in detail

### `SonarRevealController.cs`  (the driver)
- Lives on the scene GameObject currently **named `EchoPulseController`** (legacy name —
  it sits under `XR Origin (XR Rig)/…/Right Controller/Custom Objects Scripts/`).
- Serialized field **`pingEmitter`** → the scene `PingEmitter` (the object is named
  `PingEmiiter` [sic]). On `OnEnable` it subscribes to `PingEmitter.OnPingEmitted`.
- Keeps `Vector4[16] _pulses` (xyz = origin, w = start time). `Reveal(origin)` stamps a
  new pulse at the next ring‑buffer slot and calls `Shader.SetGlobalVectorArray`.
- `MaxPulses` (16) **must match** `ECHO_MAX_PULSES` in the shader.

### `EchoSonarReveal.shader`  (the visual)
- URP unlit, single forward pass, `Cull Off` (so ceilings / outward‑facing geometry
  render from inside the room).
- Declares `float4 _SonarPulses[16]` as a **global** (outside the per‑material CBUFFER —
  this disables SRP batching for the shader, which is fine for an FX shader).
- Reads `_Time.y` for "now"; pulse `w` is stamped with `Time.timeSinceLevelLoad`, so the
  two clocks match in play mode. A slot with `w <= 0` is treated as empty.

### `PingEmitter.cs`  (input + echo, NOT the sonar visual)
- Owns ping detection, cooldown, the echo spherecast/audio, the flash light.
- Fires `OnPingEmitted(origin)` **once per ping** (it was previously fired once per
  overlapped collider — fixed).
- Static `RequestPing` lets the mic trigger request a ping; static `PlayPingSound`.
- The **echo‑ripple FX is intentionally disabled**: `pingRippleFX` and `echoRippleFX`
  references were cleared (those produced expanding *rings on walls at bounce points*,
  which competed with the reveal). The `EchoHitEffect` / `Signal` prefabs still exist on
  disk; re‑assign those fields to bring them back.

### Bloom / post‑processing
- Bloom override added to `Assets/DefaultVolumeProfile.asset` (URP's global default
  volume, so it applies without a scene Volume): intensity ≈ 0.8, threshold ≈ 1.0.
- The XR rig camera (`Main Camera`) has **Post Processing enabled**, and the URP asset has
  **HDR on** — both required for the HDR ring colour to bloom.

---

## 5. Shader internals (how the reveal is computed)

Per fragment, per active pulse:

```
age      = _Time.y - startTime
radius   = age * _RevealSpeed                 // wavefront grows over time
d        = distance(worldPos, pulseOrigin)
fromEdge = radius - d                          // > 0 once the wave has passed this point

// (a) Area reveal — behind the wavefront, within _RevealRadius:
timeSince = fromEdge / _RevealSpeed            // seconds since the wave reached here
linger    = saturate(1 - timeSince / _RevealLinger)
distFall  = saturate(1 - d / _RevealRadius)
revealAmt = max(revealAmt, linger * distFall)

// (b) Leading ring — the visible travelling circle:
ring      = 1 - smoothstep(0, _RingWidth, abs(fromEdge))
ringLife  = saturate(1 - radius / _RevealRadius)
ringGlow += ring * ringLife
```

Final colour:

```
baseCol = albedo * _BaseColor                  // near‑black ambient
tintMul = lerp(1, _RevealTint, _TintAmount)    // subtle MULTIPLICATIVE tint (keeps true colour)
lit     = albedo * _RevealBrightness * tintMul
col     = lerp(baseCol, lit, revealAmt)        // reveal shows the REAL texture
col    += _RingColor * _RingBrightness * ringGlow   // glowing ring on top
```

Key point: the revealed surface shows its **own albedo** (real colour), not a white/teal
wash. An earlier version used additive tint + high brightness and read as washed‑out white;
that was replaced with the multiplicative tint + `lerp` compositing above.

---

## 6. Tuning — material properties

All knobs live on the **material** (Inspector) so each surface can differ. Defaults:

| Property | Default | Effect |
|---|---|---|
| `_BaseColor` | ~`(0.03–0.05)` grey | The dark ambient between pings. Keep low for a dark room. |
| `_RevealQuality` | Flat (0) | Shading of revealed surfaces: **Flat** (albedo), **Normals** (relief, pulse‑lit), **PBR** (relief + sheen + AO). |
| `_RevealAmbient` | 0.35 | (Normals/PBR) brightness for surfaces not facing the pulse, so they aren't pure black. |
| `_RevealSpeed` | 8 (m/s) | How fast the ring travels outward. Lower = slower, more readable sweep. |
| `_RevealRadius` | 8 (m) | How far the pulse reaches; reveal/ring fade out here. |
| `_RevealLinger` | 1.5 (s) | How long a revealed surface stays lit after the wave passes (the "remember" window). |
| `_RingColor` (HDR) | teal | Colour of the travelling ring; HDR so it blooms. |
| `_RingWidth` | 0.6 (m) | Thickness of the ring. |
| `_RingBrightness` | 1.5 | Ring glow strength. |
| `_RevealBrightness` | 1.2 | How brightly revealed surfaces show their texture. |
| `_RevealTint` / `_TintAmount` | teal / 0.1 | Subtle multiplicative sonar tint over the real colour. |

Global glow lives in `DefaultVolumeProfile` (Bloom intensity/threshold).

---

## 7. Making new geometry sonar‑revealable  ← important for new rooms

A surface only reveals if its material uses **`EchoRoom/EchoSonarReveal`**.

**Easiest way — the Reveal Converter window:** open **`Tools ▸ Sonar ▸ Reveal Converter`**,
drag your **FBX model(s) or prefab(s)** into the drop area, and click **Convert**. For each
asset it finds every material, **auto-detects** the best quality from that material's maps
(normal map → Normals; metallic/AO → PBR; base only → Flat), switches it to the reveal shader
carrying its maps over (base, normal, metallic/smoothness, AO + tiling), and darkens the base.
**FBX-embedded materials are extracted automatically** (to a sibling `Materials/` folder) so
they can be edited. Undoable. (Source: `Assets/_EchoRoom/Editor/SonarRevealTools.cs`.)

**Reveal Quality** is a dropdown on the material — flip it to compare the three modes live:
- **Flat** — albedo only. Cheapest; flat look.
- **Normals** — reads the Normal map and treats the expanding pulse as a moving light, so
  surface relief pops as the ring sweeps past.
- **PBR** — Normals + metallic/smoothness + AO, lit by the pulse (relief + sheen). Richest,
  highest cost on Quest.

Manual fallback: set the material's Shader to `EchoRoom/EchoSonarReveal`, set `_BaseColor`
dark, and assign maps to `_BaseMap` / `_BumpMap` / `_MetallicGlossMap` / `_OcclusionMap`.

The room geometry already converted: `Wall`, `Floor` (`Enviornment/`), `Carpet_Metallic`
(Billion Mucks), `Outdoor_Wall_T02` (Phoenix3D, the roof), plus the `_EchoRoom` custom
wall/floor materials.

> Leave **interactables as landmarks**: buttons, doors, knobs deliberately keep their own
> (lit/glowing) materials so they stay faintly visible even before a ping.

---

## 8. Legacy / disabled — do not confuse these with the active system

Kept on disk for reference/rollback, but **inert**:

- `Assets/_EchoRoom/Shaders/EchoPulseSonar.shader` — an earlier Dying‑Light‑style
  *expanding shell* attempt (bright sparkly band + afterglow). Reads `_PingOrigins[20]`.
- `Assets/_EchoRoom/Scripts/Controller/EchoPulseController.cs` — its driver. The component
  on the rig is **disabled**, and `PlayerController.echoPulseController` was **nulled**, so
  it is never invoked.

Already deleted (the truly old sonar): `EchoPulseShader.shadergraph`, its orphan
`EchoPulse_Mat.mat`, and the third‑party `MadeByProfessorOakie/SimpleSonarShader`.

---

## 9. Known limitations & gotchas

- **Unlit shader:** revealed surfaces ignore scene lights/shadows (intended for the dark
  room). `Cull Off` doubles fill on revealable surfaces (cheap, fine for this scene).
- **One shared pulse buffer:** all reveal materials read the same `_SonarPulses`. That's
  the design (a global sonar), not a bug.
- **Bloom + HDR required**, and bloom only shows in the **Game view / HMD**, not the Scene
  view. If the ring doesn't glow, check the camera's Post Processing toggle and the URP
  asset's HDR flag.
- **Reveal radius vs room size:** if a surface is farther than `_RevealRadius` from the
  ping origin it never reveals. Raise the radius for larger rooms.
- **Performance (Quest):** post‑processing/bloom has a real cost on standalone Quest. If
  framerate dips, lower Bloom `maxIterations`/`downscale` or the per‑material radius.

---

## 10. Next steps / open items

- [ ] **Apply the reveal shader to the new maze rooms.** `Maze_5x5_A/B/C.fbx`
  (`Assets/_EchoRoom/Models/`) use their **own embedded materials**, which are **not**
  `EchoSonarReveal` yet — those rooms will **not** reveal until converted. One‑click fix:
  drag the maze FBX into **`Tools ▸ Sonar ▸ Reveal Converter`** and click Convert (§7). This
  is the most likely "why doesn't the new room light up" question.
- [ ] Tuning pass per room scale (`_RevealSpeed`, `_RevealRadius`, `_RevealLinger`).
- [ ] Decide which props stay **landmarks** (lit) vs go dark‑reveal.
- [ ] (Optional) Rename the host GameObject from `EchoPulseController` →
  `SonarRevealController` to match the active component.
- [ ] From the GDD, not yet built: **ping‑mode toggle** (hi/low frequency), **haptics** on
  ping, multi‑ping balancing.

---

## 11. File map (quick reference)

```
Assets/_EchoRoom/
  Shaders/
    EchoSonarReveal.shader      ← ACTIVE sonar visual (Flat / Normals / PBR quality)
    EchoPulseSonar.shader       ← disabled legacy (DL2‑style shell)
  Editor/
    SonarRevealTools.cs         ← Tools ▸ Sonar ▸ Reveal Converter window (auto-converts FBX/prefab materials)
  Scripts/
    PingEmitter.cs              ← ping input + echo audio/flash (FX rings disabled)
    MicPingTrigger.cs           ← mic → ping
    Controller/
      SonarRevealController.cs  ← ACTIVE driver (writes _SonarPulses)
      EchoPulseController.cs    ← disabled legacy driver
      PlayerController.cs        ← bridge; its echoPulseController link is nulled
  Models/
    Maze_5x5_A/B/C.fbx          ← maze rooms (materials NOT yet on the reveal shader)
Assets/DefaultVolumeProfile.asset  ← Bloom (global) for the glow
Docs/SONAR_SYSTEM.md               ← this document
```
