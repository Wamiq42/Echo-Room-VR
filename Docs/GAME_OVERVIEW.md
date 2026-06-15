# Echo Room VR — Game Overview & Art Direction

> A plain-language overview of what the game is and, importantly, **how it should
> look** — colors, walls, lighting, materials. Written so you can make confident
> visual decisions without a design background. When in doubt, follow the
> "Recommended" choices; they're picked to work together.

---

## 1. The game in one paragraph

You are in a pitch-black room. You can't see — but you can make sound. Press a
button, snap, or **shout into the mic**, and a pulse of sound ripples outward,
briefly lighting up the walls, edges, and objects it touches before fading back to
darkness. You explore, remember what you saw, find hidden switches, and press them
to open the door to the next room. **"Play blind. See with sound."**

## 2. Design pillars (the 3 things that must always be true)

1. **Darkness is the default.** The world is hidden until *you* reveal it. Light is
   a reward for acting, never free.
2. **Sound is sight.** Every reveal is tied to a sound the player or world made.
   The fantasy is echolocation — bat/dolphin/Daredevil.
3. **Calm, eerie, readable.** Not horror-scary. Tense and atmospheric, but the
   important things (buttons, doors) are always clearly readable when revealed.

## 3. Core loop

```
PING  ->  REVEAL (sonar sweep lights up nearby geometry)  ->  REMEMBER
   ->  MOVE / INTERACT  ->  SOLVE (press all buttons)  ->  DOOR OPENS  ->  next room
```

## 4. Level structure

Levels are prefabs listed in a `LevelData` ScriptableObject and loaded by
`GameManager`. Recommended early progression (all reuse existing button/door code):

| # | Room | Purpose |
|---|------|---------|
| 1 | Tiny tutorial room (1 button) | Teach: ping reveals, grip presses, door opens. |
| 2 | Maze 5x5 — Variant A | Teach exploration by echo. (`Design/Maze_5x5_A.svg`) |
| 3 | Maze 5x5 — Variant B | Harder spread of buttons. |
| 4 | Maze 5x5 — Variant C | Longest path. |

---

# ART DIRECTION

This is the part to lean on. The whole look rests on **one idea: high contrast
between a near-black "void" and a bright, emissive "reveal."** Get that contrast
right and the game looks good almost automatically.

## 5. Color palette

### ⭐ Recommended palette — "Sonar Teal on Void"

Classic echolocation look: cool teal pulse on near-black. Maximally readable in a
VR headset, easy on the eyes in darkness, and the gameplay colors (button/door)
pop without clashing.

| Role | Name | Hex | Where it's used |
|------|------|-----|-----------------|
| Void / background | Abyss | `#05080D` | Skybox solid color, fog color, ambient. The "nothing." |
| Base geometry (unlit) | Slate | `#0E1A24` | Wall/floor base color before a ping hits them. Barely visible. |
| **Primary reveal / sonar pulse** | Sonar Teal | `#2DE2E6` | The sweeping pulse edge, revealed wall glow. The signature color. |
| Reveal secondary | Deep Teal | `#0E6E72` | Mid-fade of the pulse, distant reveals. |
| Player ping flash | Cool White | `#DCEFFF` | The brief point-light flash when you ping. |
| Interactable (idle) | Amber | `#FFC23D` | Buttons before pressed. Warm = "touch me." |
| Success / solved | Mint | `#5BE584` | Button pressed, puzzle solved, door unlocking. |
| Locked / danger | Rose | `#FF5C7A` | Locked door seam, hazards. |

> **Why these work together:** teal + amber are near-opposites on the color wheel,
> so buttons stand out against revealed walls. Mint and rose are reserved *only*
> for state changes (solved / locked), so a color change always means something.

### Alternate palettes (if you want a different mood — pick ONE and commit)

**"Bio-Sonar" (warmer, organic, bat/dolphin):**
Void `#07060A` · Pulse `#B26CFF` (violet) · Idle button `#FFB02E` · Solved `#74F2A0` · Locked `#FF5C7A`

**"Deep Sea" (cold, for an underwater-temple theme):**
Void `#02080C` · Pulse `#2BD4FF` (ice blue) · Idle button `#FFD66B` · Solved `#74F2A0` · Locked `#FF6F91`

## 6. How the maze walls should look

The sonar shader reads best on **simple, flat, blocky geometry** — which is exactly
what the GDD calls for and what ProBuilder makes easy. Concrete guidance:

**Shape & scale (use the blueprint scale: 1 tile ≈ 2 m):**
- Corridor width: **~2 m** (1 tile). Comfortable for VR walking + room-scale.
- Wall height: **2.5–3 m** — tall enough to feel enclosing in a headset.
- Wall thickness: **0.2–0.3 m**. Thin reads as "panel," thick as "bunker." Thin is fine.
- Keep faces **large and flat**. Avoid noisy high-poly detail — the pulse sweep
  looks crisp on big planes and muddy on busy meshes.

**Material (the important bit):**
- Base color = **Slate `#0E1A24`**, matte (low/zero smoothness), almost no specular.
  Between pings the wall is *barely* visible — a hint, not a clear surface.
- Apply the **SimpleSonarShader** (already in the project) so each ping paints a
  bright **Sonar Teal `#2DE2E6`** band sweeping across the surface.
- Add a **faint emissive edge/seam** so corners and wall edges stay slightly visible
  even in darkness. Echolocation reads *silhouettes and edges* — emphasizing edges
  helps players judge depth and not feel hopelessly lost. Tune the brightness low;
  raise it if playtesters get lost, lower it if it feels "too lit."

**Surface texture (optional but high-impact):**
- A subtle **grid / scanline / hex pattern** (as an emissive or normal map) that the
  pulse lights up. This sells the "digital echo / sonar" feel *and* gives the eye
  reference points for distance. Keep it dark and low-contrast at rest.

**Floor vs. ceiling (so up/down is clear in VR):**
- Floor: slightly lighter/warmer than walls, with the grid pattern — helps orientation.
- Ceiling: the **darkest** surface (closest to Void). Players rarely look up; keep it cheap.

## 7. Lighting setup (Unity values to aim for)

The look is "no global light; light only where the player creates it."

- **Skybox:** solid color = Void `#05080D` (not a skybox texture).
- **Ambient light:** Color/Flat, set to Void, **very low intensity** (~0.0–0.05).
- **No directional sun.** Delete/disable the default directional light.
- **Fog:** enabled, Exponential, color = Void, density tuned so walls ~6–10 m away
  fade into black. This reinforces the "reveal radius" and hides un-pinged geometry.
- **Player ping flash:** the point light already in `PingEmitter` (`pingFlashLight`),
  color = Cool White `#DCEFFF`, short and punchy. This is the player's "heartbeat" of light.
- **Interactable glows:** buttons/doors use **emissive materials** (Amber / Rose) at
  low intensity so they're faintly visible even before a ping — they're your landmarks.

## 8. In-game color coding (keep it consistent with the blueprints)

So the player learns the language fast, match the blueprint legend:

| Element | Idle | On change |
|---------|------|-----------|
| **Button** | Amber `#FFC23D` ring/glow | Flash Mint `#5BE584` + "click" sound when pressed |
| **Door** | Rose `#FF5C7A` seam (locked) | Seam turns Mint and splits open when puzzle solves |
| **Revealed wall** | Slate, dark | Sonar Teal sweep on ping |

## 9. Accessibility (cheap to do, worth it)

- **Don't rely on red/green alone** (colorblind players). Always pair a color change
  with **motion or shape**: a button ring that *fills up*, a door seam that *splits*.
- Offer a **"brightness / contrast" toggle** later: some players want more ambient
  visibility, some want full darkness for challenge (this is also a GDD "hardcore mode").
- Keep **teleport locomotion** as an option alongside smooth movement (comfort).

## 10. Audio direction (short version)

- **Echo** = the core feedback; already spatial via `EchoSoundController`. Pitch/volume
  by material & size is a GDD goal not yet built (see `GDD_PROGRESS.md`).
- **Ambient bed:** low drips / wind / hum at the edge of hearing — sells the space and
  masks silence. One looping track per room mood.
- **Puzzle stingers:** soft *tone* on button press, *chime* on solve, *low buzz* on a
  wrong/locked interaction. Reserve these sounds for state changes (like the colors).

---

## TL;DR for the non-designer

- Background and walls: **near-black** (`#05080D` / `#0E1A24`).
- The sonar pulse and revealed edges: **bright teal** (`#2DE2E6`).
- Buttons: **amber** (`#FFC23D`); turn **mint** (`#5BE584`) when pressed.
- Doors: **rose** (`#FF5C7A`) when locked; **mint** when opening.
- Walls = simple flat blocks (ProBuilder), matte dark material + SimpleSonarShader +
  a faint glowing edge so you can still sense them between pings.
- No sun. Just darkness, fog, your ping flash, and the glow of buttons/doors.

Follow that and it will look coherent and on-theme.
