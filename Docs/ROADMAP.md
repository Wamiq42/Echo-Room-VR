# Echo Room — Development Roadmap

> Dependency-ordered build plan from the current state (~30–35% built; core sensory loop
> works) to the full game in [`GDD.md`](GDD.md). Phases are sequenced so each builds on the
> last — doing them out of order causes rework. Legend: 🟥 critical path · 🟦 code · 🟪 design ·
> 🟨 art · 🟩 audio.
>
> Deadline-agnostic (per request). The Act-1 slice (Phases 0–4) is also the natural Early-
> Access cut whenever a date is chosen.

---

## Phase 0 — Stabilize the foundation 🟥
*Make the existing loop actually work end-to-end and be solid before building on it. These
are the known bugs from `CODEBASE_ANALYSIS.md`.*

- 🟦 **Wire level progression** — detect "room solved + player exited through the door" and
  call `GameManager.CompleteLevel()` → `LoadNextLevel()`. Add a door/exit trigger volume.
  *(Blocker ①. Nothing downstream is testable without this.)*
- 🟦 **Make puzzle/button reset real** — implement `EchoButtonInteractable.ResetElement()` and
  `EchoPuzzleController.ResetPuzzle()` so replays don't leave buttons stuck. *(Blocker ②.)*
- 🟦 **Re-cache renderers on level load** — `EchoPulseController` caches in `Awake` only, so
  prefab levels spawned later by `GameManager` never pulse. Re-cache on `OnLevelLoaded`.
- 🟦 **Hoist `OnPingEmitted` out of the overlap loop** in `PingEmitter` (currently fires once
  per nearby collider → N sonar pulses per ping).
- 🟦 **Startup/edge null-safety** — guard `PingEmitter.Awake` (`echoSoundPrefab`, `clip`),
  `Keyboard.current != null`, cache `Camera.main`, and `GameManager.Instance` access in
  `EchoButtonInteractable.Awake`.
- 🟦 **Clean up minors** — empty `HandPressCollider`, double null-check in
  `DynamicSprintController`, `Door` slerp hard-coded `5 *` factor.

**Exit criteria:** load → ping → solve a room → door opens → walk through → next level loads →
replay resets cleanly. End-to-end, no errors.

---

## Phase 1 — Core mechanic feel (ping economy) 🟥
*The loop works; now make it feel right and protect the "darkness is default" pillar.*

- 🟦🟪 **Implement the ping cooldown** — gate re-ping until the reveal fades (~2.5 s basic).
  *(Blocker ③.)*
- 🟪 **Tune reveal range / sweep speed / fade** to GDD §4.2 (10 m / ~10 m/s / ~0.5 s hold +
  ~2 s fade). Playtest until "remember between pings" feels right.
- 🟦 **Differentiate mic-shout** — bigger/wider reveal, longer cooldown (16 m / 4 s).
- 🟪 **Decide speed-of-sound** — keep real 343 m/s (visual carries distance) or exaggerate to
  ~40 m/s so the *audio* echo delay reads as distance (GDD §4.1). Test both.
- 🟦 **Basic ping haptic** — short controller pulse synced to emission.

**Exit criteria:** pinging has rhythm; the room genuinely goes dark between pings; mic vs
button feel distinct.

---

## Phase 2 — Audio foundation (make-or-break) 🟩🟥
*Echolocation lives or dies on spatial audio. Do this before content so rooms are built to
sound right.*

- 🟩 **Switch spatializer to the Meta XR Audio SDK** (off Unity built-in). Enable **HRTF** on
  the echo and key sources so direction is audible.
- 🟩 **Reverb per room size** + **occlusion** (sounds muffle through walls) via Meta XR Audio.
- 🟩 **Keep the raycast echo for gameplay; add reverb room-tone.** (Do *not* attempt geometric
  bouncing on Quest.)
- 🟩 **Ambient bed** — one looping low drips/wind/hum track.
- 🟩 **Puzzle SFX set** — button press, success chime, error buzz, door open. Reserve sounds
  for state changes.

**Exit criteria:** you can roughly tell *where* and *how big* a space is by ear alone.

---

## Phase 3 — The vertical slice: Act 1 end-to-end 🟥
*One complete, polished arc. This is the Early-Access cut.*

- 🟦🟪 **Dialogue + subtitle system** — alter-ego lines triggered by location/events;
  **subtitles are the source of truth** (VO optional later). Localization-friendly strings.
- 🟪🟩 **Opening sequence** — wake in the dark, first alter-ego lines, the "make a sound" beat.
- 🟪 **Tutorial room** — teach ping / move / grip via the alter ego (GDD §3.9).
- 🟦🟨 **Wire the first maze** (currently disabled) as Act-1's navigation segment + the
  **countdown timer** (generous; audible low-time warning).
- 🟦 **First logic room** as the Act-1 chamber (reuses existing button/door scripts).
- 🟦 **Checkpoints + minimal save** — respawn at room entrance; persist current room + settings.
- 🟪 **Playtest the full slice** — does echolocation "click" without confusion?

**Exit criteria:** a stranger can put on the headset and play Act 1 start→finish unaided.

---

## Phase 4 — VR comfort, input & UX 🟦
*Required for a shippable, comfortable build.*

- 🟦 **Comfort options** — teleport locomotion option, snap turn (30–45°) + smooth, comfort
  vignette/tunneling toggle, **height-offset slider** for seated play.
- 🟦🟨 **Hide controller models; show stylized hands / glowing fingertips** (controllers stay
  as input — no hand-tracking).
- 🟦 **Haptics spec** — button click, wall bump, entity-near rumble (GDD §5.4).
- 🟦🟨 **Main menu scene** + **settings menu** (volume, comfort toggles, subtitles, restart,
  quit) + pause menu.

**Exit criteria:** comfortable seated or standing; all settings adjustable in-headset.

---

## Phase 5 — Ability breadth & radial menu 🟦🟪
- 🟦 **Directional / focused ping** (long-range, narrow).
- 🟦 **Multi-ping** (360° burst, long cooldown).
- 🟦 **Noise filter** (isolate signal from ambient/decoys).
- 🟦🟨 **Off-hand radial menu** to pick ping mode (appears once 2+ types exist).
- 🟪 **Tie unlocks to progression beats** (alter ego "teaches" each — GDD §3.5).

---

## Phase 6 — Content: room types & Acts 2–3 🟪🟨
- 🟦🟪 **Pattern rooms** — memorize/mimic sound sequences + **visual pulse per tone**
  (accessibility, GDD §5.5).
- 🟦🟪 **Stealth rooms** — **sound-hunting entity AI** (Patrol→Alert→Search→Chase→caught→
  respawn) + a **visual tell** when near (GDD §4.4).
- 🟨 **Build out mazes B–E** as Act 2/3 navigation segments.
- 🟨🟪 **Build remaining chambers** (logic/pattern/stealth) and assemble **Acts 2 & 3** per the
  progression map (GDD §3.6).
- 🟩🟪 **Place audio logs** through the building (story delivery, GDD §2.9).
- 🟪🟩 **The integration climax / ending** (GDD §2.10 — design the specific beats here).

---

## Phase 7 — Replay modes 🟦🟪
- 🟦 **Endless / Procedural maze mode** — hook the seed-based generator to runtime spawning.
- 🟦 **Time-Attack** — maze timer → medals/leaderboard.
- 🟦🟪 **Hardcore mode** — reduced / no visual feedback.
- 🟦 **Scoring/medals** — time remaining, fewest pings, audio logs found.

---

## Phase 8 — Polish & art pass 🟨🟩
- 🟨 **Lighting/fog/darkness** tuned to the `GAME_OVERVIEW.md` palette; emissive wall edges.
- 🟨 **Color coding** — amber idle buttons → mint solved; rose locked door → mint opening
  (always paired with motion/shape for colorblind support).
- 🟩 **Echo refinements** — pitch-by-material, volume-by-object-size, echo-absorbing surfaces
  (GDD core-mechanic goals not yet built).
- 🟨 **Feedback/particle polish**, ping flash light tuning.

---

## Phase 9 — Performance & optimization (Quest) 🟦🟥
- 🟦 **Profile on device; hold 72 Hz.**
- 🟦 **Bloom mobile path; cut sonar-shader overdraw** (the two flagged risks).
- 🟦 **Draw-call/batching, poly budget, audio-voice cap** (GDD §8.2).

---

## Phase 10 — Ship prep 🟪🟨
- 🟪 **Meta developer-org verification** (start early — multi-day).
- 🟨 **Store assets** — icon, screenshots, trailer, description, privacy policy, IARC rating.
- 🟦 **VRC compliance pass** (framerate, crashes, permissions, boundary, comfort rating).
- 🟦 **Build + upload** — SideQuest/itch first; Meta Store submission in parallel.

---

## Phase 11 — Post-launch 🟪
- More acts/chambers; **DLC environments** (Underwater Temple, Space Station — GDD §9).
- **Maze Generator sharing tool**.
- Patch on KPIs + playtest feedback (downloads, ratings, completion rate, "did echolocation
  click").

---

## Critical path (the spine)
`Phase 0 (loop works) → 1 (ping feel) → 2 (audio real) → 3 (Act-1 slice) → 4 (comfort) →`
`9 (perf) → 10 (ship)`. Phases 5–8 and 11 add breadth/depth and can interleave once the spine
holds. **Start at Phase 0, task 1: wire level progression.**
