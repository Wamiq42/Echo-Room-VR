# Echo Room — Game Design Document

> **This is the living, canonical GDD.** It supersedes the original one-page PDF.
> Sections marked **✅ developed** have been worked through in depth; **📄 from original**
> are the original pitch content, preserved and awaiting expansion.

## Document control

| | |
|---|---|
| **Version** | 0.5 (working draft) |
| **Last updated** | 2026-06-17 |
| **Author** | Wamiq Uddin (design + dev) |
| **Status** | Active development — branch `Development-Phase`; targeting **Early Access ~June 25, 2026** |
| **Engine / target** | Unity 2022.3.56f1 LTS (URP), **Meta Quest 2/3** (OpenXR) |

**Changelog**
- v0.5 — Developed **§4 Core Mechanics & Tuning** (concrete numbers, stealth AI, asset inventory), **§5 VR, Comfort & Accessibility** (new), **§7 Audio** (Meta XR Audio spatializer + concepts), **§8 Technical & Performance** (Quest budget, save, cert), **§10 Development Plan & Production** (June-25 reality, EA scope cut, launch channel, risk register), **§11 Supporting Material & Glossary** (new). Renumbered to fit the two new sections.
- v0.4 — Developed **§3 Gameplay, Structure & Progression**.
- v0.3 — Completed **§2** (theme, lore, the alter-ego guiding voice).
- v0.2 — Added **§2 Narrative & Setting**.
- v0.1 — Original pitch content imported from `Echo Room GDD.pdf`.

---

## Contents

1. [Game Concept](#1-game-concept) — 📄 from original
2. [Narrative & Setting](#2-narrative--setting) — ✅ developed
3. [Gameplay, Structure & Progression](#3-gameplay-structure--progression) — ✅ developed
4. [Core Mechanics & Tuning](#4-core-mechanics--tuning) — ✅ developed
5. [VR, Comfort & Accessibility](#5-vr-comfort--accessibility) — ✅ developed
6. [Art Direction](#6-art-direction) — 📄 from original (see also `GAME_OVERVIEW.md`)
7. [Audio Design](#7-audio-design) — ✅ developed
8. [Technical & Performance](#8-technical--performance) — ✅ developed
9. [Monetization](#9-monetization) — 📄 from original
10. [Development Plan & Production](#10-development-plan--production) — ✅ developed
11. [Supporting Material & Glossary](#11-supporting-material--glossary) — ✅ developed

> **Still to be added** (gaps from the GDD review): design pillars & USP, competitive
> analysis. *(Done: narrative §2; structure/progression/tutorial §3; mechanics/tuning §4;
> VR/comfort/accessibility §5; audio depth §7; performance/technical §8; production/risk
> §10; supporting material §11.)*

---

## 1. Game Concept

📄 *From original — to be expanded with design pillars, USP, and competitive analysis.*

- **Title:** Echo Room
- **Genre:** VR Puzzle / Exploration
- **Platform:** **Meta Quest 2/3** (primary; SteamVR a later/possible track)
- **Target Audience:** Puzzle lovers, experimental VR gamers, solo escape-room fans

**Core idea:** A first-person VR puzzle game where players navigate dark / visually obscured
environments using sound-based cues. The primary mechanic is **echolocation** — the player
"pings" (controller button or by making noise into the headset mic) and a sonar pulse sweeps
outward, briefly revealing geometry. The player explores, solves puzzles, and escapes using
only the feedback from their own sound emissions.

**Tagline:** *"Play blind. See with sound."*

*(Working design pillars live in [`GAME_OVERVIEW.md`](GAME_OVERVIEW.md): 1) Darkness is the
default; 2) Sound is sight; 3) Calm, eerie, readable.)*

---

## 2. Narrative & Setting

✅ *Developed 2026-06-17. Established canon; level design, audio, and naming flow from it.*

### 2.1 Logline

> *Blinded by the accident that emptied this place, you click your way out through the
> dark — one room at a time — learning what happened here as you go.*

### 2.2 The questions, answered

| Question | Canon |
|---|---|
| **Where is "here"?** | A sealed, powered-down research facility. It is lightless because the power is dead — the darkness is *physical and explained*, never an arbitrary stylistic choice. |
| **Who are you?** | A person caught in the accident here — not a hero, not armed, not trained. Alone and vulnerable. |
| **Why can't you see?** | The accident took your sight. |
| **Why can you echolocate?** | The *same* accident rewired how you perceive. Sound now paints the world for you. **You are the sensor** — there is no device. |
| **Who guides you?** | A second self — an *alter ego* the same accident created. It is the part of you that adapted to sound-sight; it perceives what blind "you" cannot, and it is on your side. (See §2.5.) |
| **Why do you move forward?** | To escape, one sealed door at a time — and, as you go, to learn what this place did to you. |

### 2.3 Theme

The game is, underneath the puzzles, about **trusting an unfamiliar part of yourself** and
**what "seeing" really means.** Two halves of one person — the frightened, blind self and
the calm, perceiving self the accident created — cooperate to get one body out of the dark.
Supporting threads: *adaptation and survival*, and *the gift that costs everything*. The arc
bends toward **integration** — accepting the new self rather than escaping it.

### 2.4 The accident, the facility & the lore

The facility was a research institution studying **sensory substitution — seeing with
sound:** turning the brain's hearing into sight (human echolocation, augmented). The stated,
fundable goal was noble — *giving sight to the blind* — but it was pushed recklessly, with a
darker underbelly to how subjects were used. **Working tone flavor: medical-with-a-dark-
underbelly** (sympathy + dread). *(Alternative on file: a colder military angle. Switch later
if the tone wants it.)*

The player was part of the program (a subject, or present when it went wrong). The experiment
**succeeded and failed in the same instant:** the player can now see with sound, **but the
process burned out their eyes** — and spawned a second perceptual self (§2.5). The event was
catastrophic: it scattered or killed the staff, killed the power, and sealed the facility.
The player wakes alone in the black. The ability, the blindness, and the guiding voice are
**one wound, one cause** — the mechanic *is* the story, not bolted-on sci-fi.

Lore is revealed by *progression through the building's changing purpose* (offices → labs →
the core where it happened) and by recovered audio logs (§2.9).

### 2.5 The guiding voice — your other self (the alter ego)

There are **no other living characters.** The only voice is **a second self the accident
created** — and crucially, **it is the part of you that can actually see.** Blind "you" is the
ordinary, frightened person who woke in the dark; the alter ego is the new perceptual self
that adapted to sound-sight. It guides you because **it literally perceives what you cannot.**

- **Diegetic reveal:** when the sonar sweep paints a room, that is the alter ego *showing you
  what it sees.* The reveal shader is not a UI effect — it is your other self lending you its
  eyes. This ties the guide directly to the signature mechanic.
- **Its nature — a trustworthy partner.** It genuinely wants you both out alive: the steady
  half that keeps you moving when the blind half wants to freeze. It teaches ("make a sound —
  listen"), reassures, and warns of hazards. **Tension comes from the facility and the
  mystery, not from the voice.**
- **Framing (important craft note):** present it as a **neurological consequence of the
  rewiring — a new perceptual self**, *not* a "split personality / madness" trope. Treat the
  voice as real and meaningful (cf. *Hellblade*), which keeps it respectful and avoids stigma.
- **Onboarding payoff:** because the alter ego can talk and react, it solves the hardest
  teaching problem (echolocation with no HUD) in-fiction — it simply tells you what to do.
- **Voice production:** it is *your* voice, processed/pitched — calm where you are panicked.

### 2.6 Why this fits the mechanic perfectly

- **The mic mechanic becomes canon.** *You* make the sound; the world answers. A blind person
  in the dark making noise to sense a room is exactly what a real person would do. The
  shout-into-the-void-and-it-answers moment is the game's signature, and the story makes it
  meaningful rather than a tech demo.
- **No new art is needed** to justify the ability (no device to model), which suits the
  "no detailed models / blocky primitives" art direction.

### 2.7 How the world threads through existing content

| Built element | In-fiction role |
|---|---|
| Rooms | Sections of the facility you escape through, one bulkhead at a time. |
| Mazes (5×5) | Service / maintenance corridors between major rooms — they justify "you're lost in the dark." |
| Buttons + door | Restoring power to / unlocking each bulkhead. |
| Lever | A heavier manual override — a different *feel* of interaction. |
| Mic ping | The player's own voice becoming their eyes — the emotional core. |
| Sonar reveal | The alter ego showing you what *it* perceives — the guide made visible. |

### 2.8 Tone

Tense, lonely, atmospheric — **not jump-scare horror.** The fear is *isolation and the
unknown*, not monsters. The alter ego adds a thread of **warmth and hope within the dread** —
you are alone, but not *hostile*-alone. The overall feel is **lonely but hopeful**, not bleak.
Matches the design pillar: *calm, eerie, readable.*

### 2.9 Story delivery (cheap, fits the art)

No cutscenes or on-screen characters — story is told **environmentally and through audio**:

- A flickering-then-dead intercom; changing room *purpose* as you go deeper (offices → labs →
  the thing at the center).
- **Audio logs** triggered by reaching a location. Example:
  > *[crackling recording] "...subject perception mapping at ninety-four percent. Optic
  > response is... gone. Completely gone. But she's navigating the test chamber with her eyes
  > shut. God help us, it works. We have to shut it d—" [cuts to static]*
- **Opening moment (~60s):**
  > *Black. Silence. You hear your own ragged breathing. Instinctively you call out — and for
  > a heartbeat the room answers: a teal sweep paints a wall, a doorway, a fallen chair, then
  > fades back to nothing. You're learning, in real time, that your voice is now your eyes.*

### 2.10 Decisions made & open threads

**Decided:** Guide = an **alter ego** (a trustworthy partner; the seeing half of you). Ending
bends toward **integration / reconciliation**.

**Open (later):** tone flavor medical (leaning) vs. military; protagonist identity & name;
facility name; whether the alter ego has its own name; specific final-ending beats.

---

## 3. Gameplay, Structure & Progression

✅ *Developed 2026-06-17. Scope and the maze fail-model are decided (§3.6, §3.7).*

### 3.1 Controls

- **Trigger:** Emit "ping" — short press = focused/basic ping; the **headset mic** (shout /
  click) = a louder, wider, longer-range reveal (see §4.2).
- **Grip:** Interact — press buttons, pull the lever (direct touch).
- **Joystick:** Smooth locomotion (teleport offered as an accessibility option).
- **Off-hand radial menu:** Select ping mode / ability — **appears only once you have 2+ ping
  types** (Act 2 onward). Replaces the original "A/B toggle"; scales as abilities unlock.
  *(Radial is the leaning choice; implementation TBD.)*

### 3.2 Core game loop

```
PING → REVEAL (sonar sweep lights nearby geometry) → REMEMBER (world fades to dark)
   → MOVE / INTERACT → SOLVE (open the bulkhead) → next room
```

The cooldown (§4.2) is what makes "REMEMBER" real: the world goes dark between pings, so the
player must hold a mental map rather than keep everything permanently lit.

### 3.3 Room types — chambers vs. connective mazes

- **Chambers** (the "rooms"): **Logic** (echo the correct panels → door — *built*), **Pattern**
  (memorize & mimic sound sequences), **Stealth** (avoid moving entities by listening — §4.4).
- **Mazes** (connective tissue): navigation segments *between* chambers. The 5 built mazes
  (A–E) are the campaign's 5 navigation segments — **the game is the escape, not a maze
  gauntlet** (see §3.6).

### 3.4 Ping economy & cooldown

**Decided model: cooldown only — no energy/charges meter** (keeps the HUD minimal and the
darkness pillar intact). Full numbers in §4.2. In short: re-ping is gated until the previous
reveal has mostly faded, so the world is *dark* between pings by design; mic-shout = bigger
reveal but longer cooldown; advanced abilities carry longer cooldowns.

### 3.5 Ability progression — unlocked *with the room type that needs it*

Each ability arrives when a room first demands it; each unlock is a story beat where the alter
ego teaches a new way to perceive.

| Order | Ability | Introduced with | Why there |
|---|---|---|---|
| 1 | Basic ping (button + voice) | Tutorial | The core sense. |
| 2 | Grip interact (buttons, lever) | First logic room | Open the first bulkhead. |
| 3 | Directional / focused ping | First maze | Long corridors need a focused, longer-range call. |
| 4 | Multi-ping (360° burst, long cooldown) | First large/open room | Panic tool + big-space reveal. |
| 5 | Noise filter | First stealth/pattern room | Isolate the signal from ambient noise & decoys. |

### 3.6 Campaign structure & progression map

**Scope (decided): a focused ~60–120 min campaign** — 3 acts, ~12–15 rooms — plus
**Endless/Procedural Maze mode** and **Hardcore mode** for longevity. Shippable solo;
expandable post-launch with the DLC environments in §9.

| Act | Zone | Teaches / introduces | Content |
|---|---|---|---|
| **1** | **Waking** (offices) | ping, move, grip; meet the alter ego | tutorial room → simple maze → first logic room |
| **2** | **The Labs** | directional ping, multi-ping | mazes 2–3, pattern rooms, first stealth encounter |
| **3** | **The Core** | noise filter | hardest mazes 4–5, stealth + **integration climax** |

- **When a maze/room ends:** a bulkhead opens, the alter ego comments, the next segment loads.
- **When the last maze ends:** that *is* the ending — reach the core / escape, the
  **integration climax** plays, and **Endless + Hardcore modes unlock.**

### 3.7 Fail states, checkpoints & respawns

- **Checkpoints:** auto-save at every bulkhead / room entrance.
- **Stealth rooms (entity catches you):** respawn at the **start of the current room.** No
  permadeath — frustration hits harder in VR.
- **Mazes (decided: literal countdown timer):** each maze has a countdown; exceeding it = fail
  → restart the maze from its checkpoint. **Tuning caveat:** set it *generous* with a clear
  audible low-time warning — a tight clock fights the careful, meditative nature of navigating
  by sound. Validate in playtest. *(Time-Attack/Endless & Hardcore can tighten it.)*

### 3.8 Difficulty, pacing & scoring

- **Difficulty rises via:** maze size/complexity, less ambient light, tighter timers,
  faster/more stealth entities, noisier rooms requiring the noise filter.
- **Pacing:** alternate tension (stealth) with calm (logic/exploration).
- **Per-room objective:** open the bulkhead. **Optional medals:** time remaining, **fewest
  pings** (efficiency), **audio logs found** (collectibles).

### 3.9 Onboarding / tutorial

Solved **in-fiction by the alter ego** (§2.5) talking you through your first minute:
> *"You can't see — but listen. Make a sound."* → you ping → the room flickers into view →
> *"That's it. That's how we'll get out."*

- **Always-on subtitles for every alter-ego line — build this regardless of voice acting.** An
  accessibility win *and* the fallback if no voice is available.
- **Voice production:** treat the **subtitle system as the source of truth**, VO as a layer on
  top, so "no voice actor" never blocks development. Free VO to try: ElevenLabs free tier, or
  open-source local TTS (Piper / Coqui / Bark / Tortoise). If none pan out, ship text-only.

---

## 4. Core Mechanics & Tuning

✅ *Developed 2026-06-17. Numbers are **starting points to tune in playtest**, not final.*

### 4.1 Echolocation

- Each ping reflects off nearby geometry; a sonar sweep reveals it visually.
- **Echo delay = distance**, **pitch shift = material**, **echo volume = object size/surface**
  (the latter two are GDD goals, not yet built — see `GDD_PROGRESS.md`).
- **Speed-of-sound design note:** the code uses the real 343 m/s, so a 10 m echo returns in
  ~0.029 s — *imperceptible*. Right now the **visual sweep** conveys distance, not the audio
  delay. If you want the *audio* delay to read as distance (as the original GDD claims),
  exaggerate the in-game speed of sound to **~40 m/s** (10 m → ~0.25 s, audible). Open decision.

### 4.2 Concrete tuning values

| Parameter | Start value | Notes |
|---|---|---|
| Ping reveal range | **10 m** | enough to see a corridor + next junction (tune 6–15) |
| Visual sweep speed | **~10 m/s** | sweep expands over ~1 s so the eye can read it |
| Reveal hold + fade | **~0.5 s hold, ~2 s fade** | world re-darkens → forces re-ping |
| Basic ping cooldown | **2.5 s** | ≈ the reveal's lifetime |
| Mic-shout ping | **16 m range, 4 s cooldown** | louder = see more, wait longer |
| Multi-ping (360°) | **8 m range, 5 s cooldown** | |
| Walk speed | **2.0 m/s** | comfortable VR smooth locomotion |
| Sprint speed | **3.5 m/s** | |
| Snap turn | **30–45°** | comfort default (+ smooth-turn option) |
| Interaction | **direct touch, ~0.6 m reach** | buttons/levers via hand collider |
| Frame rate | **72 Hz floor** | dropped frames = nausea + cert fail (§8.2) |

### 4.3 Advanced abilities

**Directional / focused ping**, **multi-ping** (360° burst), **noise filter** (background-noise
isolation). Unlock order and pairing with room types: see §3.5. Each is a longer-cooldown
variant of the basic ping, selected via the off-hand radial menu (§3.1).

### 4.4 Stealth entity / hazard design

**Decided: entities hunt by *sound*, not line-of-sight.** In a pitch-black, sound-based world,
the strongest tension is that **seeing makes you prey** — your pings and footsteps give you
away. This makes the core mechanic itself the risk, and the Noise Filter ability (§4.3) the
counter.

Simple AI state machine:
```
Patrol (wander a path)
  → (hears a ping/footstep) Alert → move toward the last sound
  → Search (investigate the area)
  → Chase (locked on, faster)
  → reaches player = caught → respawn at room start (§3.7)
Stand still + don't ping = invisible to it.
```
- Pings emit a large "sound event" at the player's position; footsteps a smaller one; the
  entity reacts to events within its hearing radius.
- Needs a **visual tell when near** (accessibility, §5.5).
- **Act-2+ content — not in the June-25 slice** (§10).

### 4.5 Replayability

- Procedural room layouts (seed-based) — the blueprint generator is already seed-based.
- Randomized sound puzzles.
- **Endless / Time-Attack mode** (the maze timer becomes a real clock + leaderboards) and
  **Hardcore mode** (reduced/no visual feedback) — unlocked on campaign completion (§3.6).

### 4.6 Asset inventory

**Have:** mazes A–E, lever (animated + textured), door + frame, echo buttons, 2 ping sounds
(`Ping.ogg`, `Radar Ping.ogg`), sonar shader/material, XR rig, particle effects.

**Slice still needs:** a main-menu scene; alter-ego voice lines (text first, §3.9); an ambient
bed loop; a button-press / solve / door SFX set; stylized hands (§5.3).

*Working approach: source assets as needed ("on the go"), but the list above is the slice's
finish line so "done" is defined.*

---

## 5. VR, Comfort & Accessibility

✅ *Developed 2026-06-17.*

### 5.1 Play mode

- **Seated *or* standing — player's choice.** This **constrains design:** every interaction
  must be reachable **seated** — no floor-level reaching, no required physical 360° turning.
- **Stationary** play space (not room-scale): simpler boundary, less sim-sickness; design
  around **stick locomotion**, not physical walking. Respect the guardian/boundary.
- **Height-offset slider** so seated players get a correct floor height.

### 5.2 Locomotion & comfort

- Smooth locomotion (joystick) + **teleport** as an accessibility option.
- **Snap turn (30–45°) default** + smooth-turn option.
- **Comfort vignette / tunneling** during smooth movement (toggle).

### 5.3 Input & hands

- **Controllers as input** (we need the grip/trigger buttons — not finicky hand-tracking).
- **Hide the controller models;** render simple **stylized hands or glowing fingertips** driven
  by the controllers. In the dark, faint glowing hands double as a comfort anchor.

### 5.4 Haptics spec

| Event | Haptic |
|---|---|
| Ping emitted | short pulse synced to emission |
| Button press | crisp click |
| Wall touch | soft bump |
| Entity nearby (stealth) | rising rumble |

### 5.5 Accessibility

Core rule: **every essential cue must exist in *both* sound and vision.**

- **Deaf / hard-of-hearing:** the worry is a "pure audio" game locks them out — but every ping
  already produces a **visual reveal**, so Echo Room is well-positioned. Watch-outs: **Pattern
  Rooms** (sound sequences) must show a **visual pulse with each tone**; the **stealth entity**
  needs a visual tell (§4.4); alter-ego voice always has **subtitles** (§3.9).
- **Blind / low-vision (a marketing angle):** a VR game playable largely *by ear* is rare and
  meaningful — "a VR game you can play blind" is press-worthy goodwill. Aspirational, not MVP,
  but worth a line in the pitch.
- **Colorblind:** never rely on color alone — pair every state change with motion/shape
  (button ring *fills*, door seam *splits*). (See `GAME_OVERVIEW.md` §9.)

---

## 6. Art Direction

📄 *From original. See [`GAME_OVERVIEW.md`](GAME_OVERVIEW.md) for the full developed art
direction — palette with hex codes, wall/lighting guidance, and in-game color coding.*

### Visual style
- Mostly dark or foggy. When sound hits an object, a brief wireframe/sonar outline appears (via
  shader); no detailed 3D models needed; all geometry is blocky / primitive.

### UI/UX
- VR menu: floating radial menu for settings (and ability selection, §3.1).
- Cooldown / state feedback via **haptics** (§5.4) + subtle audio + the reveal itself.

---

## 7. Audio Design

✅ *Developed 2026-06-17. This is the make-or-break tech for an echolocation game.*

### 7.1 Spatializer — decision

**Switch from Unity's built-in to the Meta XR Audio SDK.** It's already in the project (Meta
XR SDK 77), built for Quest, and provides HRTF + acoustic reverb + occlusion at mobile-
appropriate cost. Unity's built-in is too weak for a game where *locating sound IS the
gameplay.* *(Steam Audio is even better for true echo-bouncing but heavier — keep it as a
future PC-VR upgrade, not for Quest.)*

### 7.2 Key concepts (what they mean and why they matter here)

- **HRTF** — filtering that makes a sound feel like it comes from a *specific point in 3D*
  (left/right **and** up/down **and** front/back), not just stereo pan. **Essential** — the
  player must be able to tell *where* an echo came from.
- **Occlusion** — a sound behind a wall is muffled/quieter. Makes walls feel solid; key for
  hearing the stealth entity through geometry.
- **Reverb zones** — a closet sounds tiny, a hall sounds huge. The **reverb of your ping tells
  the player the room's size by ear** before they see it. Big win for echolocation.

### 7.3 Echo approach — raycast for gameplay, reverb for room-tone

- **Audio raycasting** (current code): cast a ray, find the hit, play **one** echo after a
  delay. Cheap, simple, an approximation. **Right for Quest — keep it.**
- **Spatial audio bouncing / geometric acoustics** (Steam Audio style): the engine simulates
  sound physically reflecting off **all** surfaces. Beautiful but **expensive — risky on
  Quest's mobile CPU. Do not attempt on mobile.**
- **Plan:** keep the raycast echo for gameplay + use Meta XR Audio's **reverb** for ambient
  room-tone. (These two are *not* interchangeable, despite the original GDD's wording.)

### 7.4 Sound sources & tools

- **Sources:** echo sounds (procedural/layered), background ambient (drips/wind/hum), puzzle
  sounds (tones, error buzz, success chime), echo-absorbing surfaces (no return — challenge).
- **Tools:** Unity Audio Mixer for sound zones/reverb; open-source sound packs or Audacity for
  recording/editing.

---

## 8. Technical & Performance

✅ *Developed 2026-06-17. Target is **Meta Quest 2/3** (SteamVR not yet).*

### 8.1 Stack
Unity 2022.3.56f1 LTS (URP 14), OpenXR + Meta XR SDK 77, XR Interaction Toolkit 2.6.4, XR Hands
1.6, ProBuilder 5.2. One scene; levels are prefabs loaded at runtime from a `LevelData` SO.

### 8.2 Performance budget (Quest)

| Target | Value |
|---|---|
| Frame rate | **72 Hz hard floor** (target 72; 90 is bonus). Dropped frames = nausea **and a cert fail**. |
| Draw calls | low (~100–200/frame); static batching + single-pass instanced stereo |
| Triangles | ~350k–750k visible/frame (Quest 2) |
| Audio voices | cap simultaneous echo sources (~32); echoes already self-destruct — good |
| Post-processing | **bloom drives the reveal glow but is pricey on mobile** — mobile-optimized URP path, keep minimal |

**Helps you:** darkness + fog + limited reveal radius naturally cull most of the scene.
**Real risks to profile early:** the **bloom** and the **sonar shader's overdraw**.

### 8.3 Save & persistence
Lightweight: current act/room + settings in PlayerPrefs or a small JSON. Auto-save at each
checkpoint (§3.7).

### 8.4 Localization
**English-only for EA**, but build the subtitle/string system so text isn't hardcoded
everywhere — cheap to localize later.

### 8.5 Platform & certification
- **Quest only** for now. Meta cert is a checklist: hold framerate, no crashes, correct
  permissions & boundary handling, comfort rating, complete store assets, privacy policy, IARC
  age rating.
- **Cert is a *later* track**, not a June-25 blocker, because EA launches on SideQuest first
  (§10.3). Meta submission runs in parallel.

---

## 9. Monetization

📄 *From original.*

- **Base game:** $4.99–$9.99; launch on SideQuest / itch first, Meta Store after (§10.3).
- **DLC / add-ons:** new environments (Underwater Temple, Space Station, …); Hardcore mode (no
  visual feedback); Echo Maze Generator tool (players create/share rooms).
- **Marketing:** Reddit (r/VRGaming), VR YouTubers, SideQuest Discord; emphasize uniqueness —
  *"Play blind. See with sound."* (Blind/low-vision-playable angle, §5.5, is a press hook.)

---

## 10. Development Plan & Production

✅ *Developed 2026-06-17. Supersedes the original 8-week table (flagged unrealistic).*

### 10.1 Team
**Solo** (Wamiq) — design, dev, art, audio — with Claude Code as the dev/design assistant.

### 10.2 The June-25 reality & EA scope cut
A full 1–2 hr campaign is **not** achievable by **June 25, 2026** (8 days out). **Early Access
of the Act-1 vertical slice is.** EA exists for exactly this: ship the slice, expand in public.

**EA slice scope:** intro + alter-ego onboarding (subtitles) + 1–2 mazes + 1 logic room + ping
cooldown + comfort options + a main menu + settings.

### 10.3 Launch channel strategy
- **SideQuest (and/or itch.io) on June 25** — no review gate; APK live within ~a day. This is
  the channel that can realistically hit the date.
- **Meta Store submission in parallel** — appears when review/VRCs clear (weeks). A slice *can*
  be published to Meta, but **slice size doesn't skip review/cert/store-asset time.**
- ⚠️ **Start Meta developer-org verification NOW** — it can take several days and is the silent
  deadline-killer.

### 10.4 The 3 blockers (must fix before any slice ships)
From `CODEBASE_ANALYSIS.md` — none done yet:
1. **Wire level progression** — `GameManager.LoadNextLevel()` after a room is solved + exited.
2. **Fix button/puzzle reset** — currently a stub; replays leave buttons stuck.
3. **Implement the ping cooldown** — using §4.2 values (2.5 s basic).

### 10.5 Slice milestones (8-day target)
1. Fix blockers ①②③ (playable loop end-to-end).
2. Tune darkness/reveal/cooldown (§4.2).
3. Build Act-1 content: intro + 1–2 mazes + 1 logic room; alter-ego lines + subtitles.
4. Comfort options (vignette, snap turn, teleport, height slider) + main menu + settings.
5. Stylized hands; basic haptics (§5.4); ambient bed + SFX.
6. Build APK, test on device (hold 72 Hz), upload to SideQuest; begin Meta submission.

### 10.6 Risk register
| Risk | Impact | Mitigation |
|---|---|---|
| 8-day deadline too tight | Miss launch | Cut hard to the slice (§10.2); SideQuest channel (§10.3) |
| Meta review won't clear by 25th | No Meta launch | Launch on SideQuest; Meta in parallel |
| Dev-org verification delay | Blocks Meta entirely | Start verification today |
| Bloom / sonar overdraw tank framerate | Nausea + cert fail | Profile early; mobile post path; cap overdraw |
| No voice actor | No VO | Subtitles-first (§3.9); free TTS optional |
| Scope creep | Slips everything | Slice scope is fixed; new ideas → post-launch backlog |

### 10.7 Post-launch roadmap & KPIs
- **Roadmap:** Act 2 → Act 3 → Endless/Hardcore → DLC environments (§9).
- **KPIs:** SideQuest downloads/ratings, Meta wishlists/reviews, completion rate of the slice,
  qualitative playtest feedback on "did echolocation click without confusion."

---

## 11. Supporting Material & Glossary

✅ *Developed 2026-06-17.*

### 11.1 Existing material (more than it feels like)
- **Level maps:** the maze blueprints **are** the level maps — `Assets/_EchoRoom/Design/
  Maze_5x5_A.svg` … `_E.svg` (top-down, grid-labeled, guaranteed-solvable).
- **Art bible:** [`GAME_OVERVIEW.md`](GAME_OVERVIEW.md) — palette (hex), wall/lighting, color
  coding. This *is* your style reference; no concept art is needed for this minimalist look.
- **Blueprint generator:** seed-based maze generator (`Downloads/maze_blueprint.js`).
- **Code/architecture reference:** [`CODEBASE_ANALYSIS.md`](CODEBASE_ANALYSIS.md);
  progress audit: [`GDD_PROGRESS.md`](GDD_PROGRESS.md); sonar tech: [`SONAR_SYSTEM.md`](SONAR_SYSTEM.md).
- *Gaps:* no concept art / mood boards (not required for this style); UI mockups TBD.

### 11.2 Glossary
- **Ping** — a sound the player emits (button or mic) that triggers the sonar reveal + echo.
- **Reveal / sonar sweep** — the brief visual lighting-up of geometry a ping causes; diegetically, the alter ego showing you what it sees.
- **Echo** — the delayed spatial audio return of a ping (delay ≈ distance).
- **Alter ego** — the player's second perceptual self (the guiding voice); see §2.5.
- **Bulkhead** — a sealed door between rooms; opened by solving a room.
- **Chamber** — a puzzle/stealth room (Logic / Pattern / Stealth).
- **Maze** — a connective navigation segment between chambers.
- **Noise filter** — ability that isolates the ping signal from ambient noise/decoys.
- **VRC** — Meta's Virtual Reality Checks (cert requirements).
- **The slice** — the Act-1 vertical slice targeted for Early Access (§10.2).
