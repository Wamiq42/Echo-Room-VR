# Decision log

Every call Claude made without asking Wamiq, and why. Reviewed after playtesting.
If a decision produced something you didn't want, find it here and we revisit that entry.

Newest at the bottom. Started 2026-07-24.

---

## D-001 — Entity named "THE AUDITOR"
**Part 1.** No canon name existed (GDD §4.4 calls it "stealth entity", §184 lists naming as open).
Picked *Auditor*: reads as facility-records vocabulary (a monitoring unit on a personnel file),
and its Latin root *audire* means "to hear". A blind protagonist naming the thing that hunts by
sound "the Auditor" is thematically exact.
**Reversible:** yes, trivially — it is three strings in TutorialDirector.

## D-002 — Lore delivered as 3 short beats, not one description panel
**Part 1.** Wamiq asked for "a panel of description". Changed to three sequential beats on the
existing prompt panel. A four-fact text wall at the end of a VR tutorial gets skipped, and it
deflates the dread the WARNING beat just built. Three beats keep the pacing and teach the same
rules. Also reuses the built, styled, tested prompt panel instead of new plumbing.
**Reversible:** yes — collapse to one longer body string.

## D-003 — Entity revealed by the player's own ping, not switched on
**Part 1.** Confirmed by Wamiq ("yes with ping sounds nice"). Guarded with
`pingEmitter.ResetCooldown()` before the gate and a 15 s timeout that reveals anyway, because a
forced-input gate with no escape hatch at the end of a tutorial is a softlock.

## D-004 — Pause menu shrunk and pulled close instead of wrist-mounted
**Part 3.** Wamiq asked for the menu on the left controller like the objective panel. Declined the
literal version: the objective panel is a 16 cm × 8 cm non-interactive glance panel; the pause menu
is 1.8 m × 1.12 m with six screens and sliders. Instead: 0.6 m panel at 0.7 m, anchored along the
head→hand direction then frozen. `1.8/2.1` and `0.6/0.7` are the same ratio, so it is **identical in
apparent size** but fits any corridor and stays stable to point at.
**Reversible:** yes — two serialized numbers.

## D-005 — Hold-B/Y pause enabled in ALL handedness modes, not just right-only
**Part 5.** Wamiq specified hold-B for right-hand-only mode. Widened it to every mode. If it only
existed in right-only mode the tutorial copy would have to branch on a settings value, and anyone
who switched handedness after the tutorial would never be taught the binding. One binding, one
lesson, always true. The Menu button keeps working everywhere as well; this is additive.
**Reversible:** yes.

## D-006 — Perf fixes applied before a baseline capture
**Part 6.** Stated earlier that I would measure before changing anything. Overriding that because
Wamiq is asleep and the four findings are unambiguously correct for Quest 2 *regardless* of what
profiling says (an always-on intermediate texture, HDR on a tiler, native render pass off, and 50 m
of shadow distance in a lightmapped game are wrong in every scenario). Making Wamiq run two headset
sessions — baseline, then verify — when one will do is a waste of his time.
Trade-off accepted: if the build is still slow we lose the clean before/after and profile deeper
using the fixed build as the new baseline.
**Reversible:** yes — every change is a serialized field, listed with before/after values below.

## D-008 — Perf changes actually applied (Part 6)
Every one is a serialized field; revert by setting the "Was" value back.

| Setting | File | Was | Now | Why |
|---|---|---|---|---|
| `m_RequireDepthTexture` | Default URP.asset | `1` | `0` | **Biggest win.** Verified nothing in the project samples `_CameraDepthTexture` / `SampleSceneDepth` — both sonar shaders are plain Opaque. This was forcing a full depth prepass every frame for nothing. |
| `m_IntermediateTextureMode` | Default URP_Renderer.asset | `1` Always | `0` Auto | Forced an offscreen target + full-screen blit every frame instead of drawing to the backbuffer. |
| `m_SupportsHDR` | Default URP.asset | `1` | `0` | FP16 buffers double bandwidth and break the Quest tile format. No post-processing volume exists to need HDR. |
| `m_UseNativeRenderPass` | Default URP_Renderer.asset | `0` | `1` | Lets URP keep work in tile memory on Vulkan. |
| `m_ShadowDistance` | Default URP.asset | `50` | `15` | Game is lightmapped; 50 m of realtime shadow range was paid for nothing. |
| `m_StoreActionsOptimization` | Default URP.asset | `0` Auto | `1` Discard | Avoids storing tile buffers that are never read back. |
| `enableDebugLogs` | PingAttractedEntity.cs | `true` | `false` | `Debug.Log` on device costs frames. Called on state changes, so minor — but free. |

**Known risk from the depth-texture change:** soft particles need the depth texture. The Auditor's
smoke may render with harder edges against geometry. If it looks wrong, set
`m_RequireDepthTexture` back to `1` — that is the one change most likely to cause a visual
regression, and it is also the biggest win, so check it deliberately.

## D-009 — Perf items deliberately NOT done yet
Holding these until there is a real frame-time number, because each costs visual quality and
none should be spent without evidence:
- **MSAA stays off.** 4× is near-free on a tiler and VR needs it, but turning it on while over
  budget muddies the measurement. Turn on once we are inside budget.
- **Texture sizes stay at 2048.** 14 maze textures, `maxTextureSize: 2048`, Automatic
  compression, no Android override. That is defensible; dropping to 1024 is a visible
  regression I will not make without proof it is needed.
- **Main light shadows stay supported.** Reducing the distance is the safe win; disabling
  outright could change how lit levels look.
- **`EchoSonarReveal` uses `Cull Off`**, which doubles fragment work on every wall. It is
  deliberate (ceilings must be visible from inside), so it stays — but it is a real cost and
  worth knowing about if we end up GPU-bound.

## D-010 — QuestPerformanceTuner: runtime guards, not `#if`, and self-installing
New file `Assets/_EchoRoom/Scripts/Managers/QuestPerformanceTuner.cs`.
1. **Requests 90 Hz, auto-falls back to 72 Hz once** if >25 % of frames miss the budget over a
   10 s window. 45 fps is exactly half of 90 — the signature of a missed 90 Hz target halving.
   This way you get the best rate the fixes actually bought, without touching a setting, and the
   log says which one it landed on.
2. **Enables Fixed Foveated Rendering** (level 2, dynamic). Verified `OVRPlugin.foveatedRenderingLevel`,
   `useDynamicFoveatedRendering`, `systemDisplayFrequency`, `systemDisplayFrequenciesAvailable`
   and the `FoveatedRenderingLevel` enum all exist in Meta XR SDK v77.
3. **No `#if UNITY_ANDROID` guard**, deliberately. Guarding those calls out of editor builds means
   a typo or SDK rename only surfaces on device. Compiled and verified error-free in the editor
   instead; OVRPlugin no-ops safely off-headset and it is wrapped in try/catch.
4. **Self-installs** via `RuntimeInitializeOnLoadMethod` + `DontDestroyOnLoad` (same pattern as
   `TutorialMenuBootstrap`), so there is no scene wiring to lose. If you add the component to a
   scene manually, that instance wins and your inspector values are used.
5. **Logs frame stats every 5 s** — capture with `adb logcat -s Unity` if you want raw numbers
   without OVR Metrics Tool.

## D-011 — Prompt placement: found a second cause you did not report
**Part 4.** You reported the *first* tutorial panel being very low. The obvious cause was real —
placement used the full head rotation including pitch, so looking down when a beat fired pinned the
panel low, permanently, since placement only ran once per prompt. Fixed with a yaw-only frame
(flattened forward + world up), eye-height anchoring, and a 35° deadzone before it eases round.

But that alone would not have fixed *panel one specifically*. `ShowPrompt` for the first beat runs
from `Setup()`, which can execute **before XR tracking reports a real eye height** — so the panel
gets placed relative to a head still sitting at the rig origin, i.e. the floor. That is almost
certainly why the first one was the bad one.

So the deadzone now also triggers on **height drift > 0.15 m**, not just yaw. That covers the
tracking-settle case, and as a bonus it covers standing up, sitting down, or recentring the
guardian part-way through — which is the "make it dynamic so it's good for users" you asked for.

Also fixed while in there: `OnDisable` called `StopAllCoroutines()` without clearing the recentre
handle, which would have left the follow logic permanently dead after a re-enable.

## D-012 — Auditor: destroyed the hunt behaviour rather than disabling it
**Part 1.** Plan said "disable `PingAttractedEntity` and `NavMeshAgent`". Changed to **destroy** both
at runtime. Unity runs `Awake` on a component even when that component is disabled, and
`PingAttractedEntity.Awake` touches its NavMeshAgent — on a tutorial level with no baked NavMesh
that is console spam at best. Order is forced: `PingAttractedEntity` carries
`[RequireComponent(NavMeshAgent)]`, so removing the agent first is refused.

## D-013 — Auditor placement is computed at reveal time, not authored
**Part 1.** You asked for it behind the player. Rather than hard-coding a spot, `PlaceAuditorBehindPlayer`
takes the player's actual facing at that moment, flips it, and **raycasts backward** — if a wall is
closer than 5 m it places the Auditor just in front of the wall instead. It can therefore never be
revealed inside geometry no matter where the player is standing or facing when they finish the lever.
Authored floor height from the prefab is preserved (root y = 0.37, smoke centre ≈ 1.7 m, so it looms
at roughly eye level for a standing player).

## D-014 — "LOOK BEHIND YOU" appears on BOTH the wall and the prompt panel
**Part 1.** You asked for it written on the wall. Implemented as a self-lit `TextMesh` stencilled onto
whatever surface a forward raycast hits — so it reads on a black wall and is diegetic.

But the prompt panel *also* carries the instruction ("BEHIND YOU / Something is standing in the dark.
Turn around."). Not redundancy for its own sake: the room is pitch black, the player could be facing
anything, and if the raycast finds no wall the atmospheric layer silently does nothing. The panel is
the guaranteed channel; the wall text is the atmosphere. If the doubled message feels heavy in
headset, delete `ShowAuditorWallText()` — the lesson still lands.

## D-015 — Tutorial Entity authored INACTIVE in the level prefab
**Part 1.** Added `Tutorial Entity` as a child of `T_Junction_Tutorial.prefab`, inactive, matching how
`Tutorial Button` / `Tutorial Lever` are handled. Authored active would risk the Auditor being visible
from the first second of the tutorial if `BuildAuditorLesson` ever failed to run. `Transform.Find`
locates inactive children, so the lookup still works.

## D-016 — Pause menu scale is now one number in one place
**Part 3A.** `dynamicMenuScale` used to be captured from whatever the scene authored on the transform.
It is now derived from a serialized `dynamicPanelWorldScale` (0.000667). Panel physical size is a
single value you can change in the inspector instead of a scene-transform side effect.
`menuHandAnchor` wired to `Left Controller` in MainScene and saved.

## D-017 — Caught: serialized scene values silently override new C# defaults
**Part 3A.** Changing `distanceFromCamera`'s default from 2.1 to 0.7 in code does **not** change an
already-serialized scene value. MainScene still held 2.1, which would have put the new 0.6 m panel
at 2.1 m — a ~16° postage stamp instead of the intended 46°. It would have looked like the change
made the menu dramatically worse. Found by reading the actual serialized values back rather than
trusting the source default; fixed in MainScene and saved.

Same trap applies to `PingAttractedEntity.enableDebugLogs` (D-008): the code default is now `false`,
but `Entity.prefab` still serializes `enableDebugLogs: 1`. Left as-is deliberately — it only logs on
state changes, so it is not worth dirtying the prefab. Flip it in the inspector if you want it off.

**Standing lesson:** after changing any `[SerializeField]` default, read the value back from the
scene/prefab. The default only applies to newly added components.

## D-018 — Render-through: material override was impossible, overlay camera used instead
**Part 3B.** The ZTest-Always material plan **did not work**, and the evidence is conclusive rather
than "we couldn't get it going": `PanelSettings` exposes no material property or field at all (only
7 private built-in `Shader` fields); the world-space panel draws through a `UIRenderer` whose
`sharedMaterials[0]` is **null** because materials are supplied per-draw-call internally; and
`Hidden/Internal-UIRDefault` has no `_ZTest` property to drive. `VRMenuPanelSettings.asset` was left
untouched, which matters — the tutorial prompt and objective panel share it.

Fallback shipped: new layer **8 `MenuOverlay`**, a `Menu Overlay Camera` stacked on Main Camera with
`clearDepth`, Main Camera culling mask narrowed. Verified with screenshots of the menu behind a wall.

**~134 lines deleted** from VRPauseMenu.cs: `GetOcclusionAdjustedPosition`, `IsPanelPositionClear`,
`GetPanelHalfExtents`, the candidate-search loop, the clearance warnings, and the
`keepInFrontOfWalls` / `wallPadding` / `minimumDistanceFromCamera` / `wallMask` fields. Confirmed
gone from the compiled type by reflection.

## D-019 — Menu UI ray masks had to be narrowed, or the menu would be visible but unclickable
**Part 3B.** Drawing over the wall does not make the wall stop blocking the pointer. Measured: with
the menu behind a wall the cast hit `Maze_Walls` at 3.71 m and never reached the panel. Both
`Menu UI Ray` casters were narrowed to MenuOverlay+UI. Those interactors are dedicated to the menu
(they are exactly `vrPointerRoots`), so this does not affect world interaction.

## D-020 — Overlay camera is switched off unless the menu is open
**Part 3B, my change on review.** The subagent left the overlay camera permanently enabled. That is a
render-pass setup and a depth clear **per eye, every frame**, for a panel visible a fraction of the
time — on the exact device we are fighting for frames on. Now toggled in `SetVisible`, and it
defaults to disabled in the scene. Costs nothing during play.

## D-021 — One flagged risk dismissed: post-processing on the overlay
The subagent warned the menu would lose post-processing. Moot — MainScene has **no post-processing
volume at all** (verified: zero matches for Bloom/VolumeProfile). Nothing to lose.

**Still open from Part 3B, needs headset:** XR was never exercised (no device attached, so every
capture was flat/mono). URP 17.3 source does propagate the XR pass to overlay cameras, but that is a
code read, not a measurement. **If the menu renders in only one eye, or at the wrong depth, this is
why.** Also: the `Menu UI Ray` interactors have no line visual or reticle — pre-existing, but worth
looking at while you are in there.

## D-007 — Work order: Part 6 → 2 → 4 → 3 → 1 → 5
Perf first because 45 fps makes every other fix feel bad and the fixes are cheap config. Part 5
(one-controller) last because it rewrites the panel-anchoring that Parts 1/3/4 establish; doing it
earlier means doing the panel work twice.

## D-022 — Handedness routed through one static table, not per-file bindings
**Part 5.** Bindings are built in code in four separate files. Rather than teach each one about the
setting, they all read `EchoRoom.Settings.HandedInput`
(`Assets/_EchoRoom/Scripts/Utility/HandedInput.cs`) — one property per gameplay control, plus the
player-facing labels. Adding a fifth binding site now costs one line, and it is impossible for one
system to keep listening to a controller the player put down.
**Both mode answers "Right"** everywhere, so the two-handed scheme is byte-for-byte what it was.

## D-023 — Haptics re-routed at the choke point, not at 20 call sites
**Part 5.** `EchoHaptics.Request` collapses every hand onto the active one in single-hand mode.
Callers still name a physical hand, which keeps the two-handed scheme readable, and a one-handed
player keeps cues authored for the other hand — including `PingAttractedEntity`'s `HapticHand.Both`
proximity rumble, which is a warning and must not silently vanish.

## D-024 — Hold-B/Y is bound on the ACTIVE hand only, not both
**Part 5, deviation from D-005.** D-005 says hold-B/Y in every mode. Taken literally that binds the
left hand's Y as well, and in Both mode Y *is* the microphone push-to-talk — a held control. Holding
Y to shout would open the pause menu every time. Bound to the active hand's secondary button instead:
B in Both and Right mode, Y in Left mode. One gesture, one lesson, still always true.
**Residual conflict, accepted:** in Both mode B is also the sonar ping. The ping fires on press and
the pause needs 750 ms, so a normal tap is safe, but a deliberate long press pings *and* pauses.
`holdPauseSeconds` is a serialized field on VRPauseMenu if that turns out to be annoying in headset.

## D-025 — Sonar and objective swap buttons in single-hand mode
**Part 5.** Per the agreed scheme: A/X = sonar, B/Y = objective panel when one-handed; A = objective,
B = sonar when two-handed. This is a swap, not a move, and it is deliberate — the sonar is the verb
the game is built on and belongs on the button the thumb rests nearest, and B/Y then carries both the
panel tap and the pause hold, which is one button to teach.

## D-026 — Stick-down turn-around is disabled in single-hand mode
**Part 5.** `ActionBasedSnapTurnProvider.enableTurnAround` is authored `true`, and the Sector
interaction fires it on stick-down. In single-hand play stick-down is "walk backwards", so the two
cannot coexist. `HandednessController` switches it off for Left/Right and restores the authored value
for Both. **Pre-existing issue left alone:** in Both mode, stick-down on the *left* stick is already
both "move backward" and "180 turn-around", because XRI binds Move and Snap Turn to the same stick on
both hands. That is current behaviour, not something this work introduced, and changing it would be a
silent change to the two-handed scheme.

## D-027 — The ping emitter follows the hand
**Part 5.** `Custom Objects Scripts` (PingEmiiter + EchoPulseController) is authored under the Right
Controller. In left-hand mode it is reparented at runtime, so the pulse and its forward echo cast
come out of the controller the player is actually pointing, and restored to the authored parent in
Both mode. It is a zero-offset reparent of a whole subtree; revert by clearing
`HandednessController.handMountedObjects`.

## D-028 — Wrist panels are pushed, not pulled
**Part 5.** `MazeLevelTimer` and `VRPauseMenu` each gained a setter
(`SetHandAnchor` / `SetMenuHandAnchor`), and `HandednessController` pushes the active hand into both
from `Start` and again on every setting change. Pull would have meant three copies of the same
resolution logic and three chances to miss the mid-session switch. **VRPauseMenu's placement code is
untouched** — only the transform it reads.

## D-029 — Menu pointer is filtered, not disabled
**Part 5.** `VRPauseMenu.SetVRPointersVisible` skips any pointer root parented under the idle hand.
Disabling the interactor GameObject would not have worked: the menu SetActive(true)s them itself
every time it opens, and would have undone it.

## D-030 — `rightController` on MazeLevelTimer renamed to `handAnchor`
**Part 5.** `[FormerlySerializedAs("rightController")]` carries the serialized value across. Verified
by reading MainScene back after the rename: `handAnchor = Right Controller`, unchanged. (D-017's
standing lesson applied.)

## D-031 — AMENDS D-005: hold-pause binds to the active hand only, and my original reasoning was partly wrong
**Part 5.** D-005 said "enable hold-B/Y in every mode". Taken literally that binds **left-Y too** — and
in Both mode Y *is* microphone push-to-talk, a held control. **Every shout would have opened the pause
menu.** Corrected to: B in Both/Right, Y in Left. One binding, still always true, no collision with the
microphone.

My stated rationale for D-005 was "otherwise the tutorial copy has to branch on a settings value".
That turns out to be moot — the copy now interpolates a label (`VRPauseMenu.HoldPauseButtonLabel`),
so branching was never the real cost. The *surviving* reason to keep it universal is the good one:
someone who switches handedness after finishing the tutorial would otherwise never learn the binding.

**Residual conflict, deliberately left alone:** in Both mode B is also the sonar ping. Ping fires on
press and pause needs 750 ms, so a tap is safe — but a deliberate long press pings *and* pauses. I did
NOT tune this blind. Raising `holdPauseSeconds` makes the essential binding sluggish for the
accessibility user who actually needs it, and that trade is only judgeable in a headset.
**This is the number one thing to feel-test.** Two escape hatches: raise `holdPauseSeconds`
(serialized on VRPauseMenu), or restrict hold-pause to single-hand modes only.

## D-032 — Tutorial copy made handedness-aware (bug Part 5 would otherwise have shipped)
**Part 1 + 5.** All four tutorial messages hard-coded button names — "Press B", "Hold Y", "Press A",
"RIGHT TRIGGER". In left-hand mode every single one becomes false: sonar is X, objective is Y,
microphone is the grip, trigger is on the left. A tutorial confidently teaching four wrong bindings
is worse than no tutorial.

Converted to computed properties reading `HandedInput`. Verified in-editor across all three modes:

| | Both | Right | Left |
|---|---|---|---|
| Sonar | B | A | X |
| Microphone | Y | GRIP | GRIP |
| Objective | A | B | Y |
| Trigger | RIGHT | RIGHT | LEFT |
| Pause | Hold B | Hold B | Hold Y |

**Both mode output is byte-identical to the old hard-coded copy**, so nothing changes for existing players.

## D-033 — Pause lesson is taught but not gated
**Part 5.** New beat during the corridor walk (6 s in, holds 5 s, then restores the direction prompt).
Deliberately **not** gated on the player performing the hold: succeeding would open the pause menu
mid-tutorial, which stops time and throws away the moment. This is a binding worth knowing, not a
skill worth drilling. `VRPauseMenu.HoldPausePerformed` exists if we later want to acknowledge it.
Guarded against racing the interaction-wall trigger — that transition now explicitly kills the lesson
coroutine rather than trusting a step check, since it could otherwise be mid-crossfade on the same panel.

## D-034 — SUPERSEDES D-013: Auditor is authored at the junction centre, not computed at runtime
**Part 1.** Wamiq's call, and he was right — the runtime raycast was solving a problem this room does
not have. A T-junction has one obvious stage.

Derived the centre from the prefab geometry rather than eyeballing it:

| | x | z |
|---|---|---|
| StartCheckpoint | 0.92 | -1.18 |
| Stem side walls | -2.95 and 4.79 → midpoint **0.92** | |
| Crossbar walls (Entity wall, Interaction Wall, Door Frame, Long Wall 1) | | 17.01 / 17.49 / 17.44 / 16.91 → **≈17.2** |

The corridor centreline is exactly the player's start x, which is a good sign the numbers are right.
Authored the Tutorial Entity at **(0.92, 0.37, 17.2)** local. y = 0.37 is unchanged, putting the smoke
column at ≈1.7 m — eye level for a standing player.

This also happens to make the "look behind you" beat literally true: the player finishes at the
button/lever wall (x ≈ -7.7) facing -X, so the junction is directly behind them. No dependence on
where they happen to be standing.

Deleted `PlaceAuditorBehindPlayer()` plus the `AuditorRevealDistance` / `AuditorWallPadding`
constants; replaced with `FaceAuditorAtPlayer()`, which only sets yaw. ~20 lines to 6.

**Note:** the entity's old position (-1.55, 0.37, -6.4) was identical to Tutorial Lever's — whoever
staged `Tutorial Entity.prefab` copied the lever's transform, so that value never meant anything.

**Unverified:** Unity's editor link dropped before this change, so it is **not compile-checked** —
unlike everything above it. The edit is small (one method removed, one smaller added, two unused
constants deleted) but treat a compile error here as plausible. The prefab YAML edit was verified by
reading it back.
