# Echo Room VR — Quest Store Polish Work (Detailed Handoff)

Target: Ship-ready MVP at $4.99 on Meta Quest main store.
Total estimated effort: ~7-10 hours of paired work across 9 tasks.

This document contains implementation-level detail for each task so any session can pick it up cold and execute.

---

## Project Context

- **Engine:** Unity 2022.3.56f1 LTS, URP 14
- **VR SDK:** OpenXR + Meta XR SDK 77 + XR Interaction Toolkit 2.6.4 / 3.3.1
- **Scripts root:** `Assets/_EchoRoom/Scripts/`
- **UI system:** UI Toolkit (NOT Canvas). Menus use `UIDocument` + `VisualTreeAsset` (.uxml) + `StyleSheet` (.uss) + `XRUIToolkitManager` for VR pointer interaction.
- **Shared UXML:** `Resources/UI/VRMenu.uxml` — both `VRMainMenu.cs` and `VRPauseMenu.cs` load this same layout asset.
- **Shared USS:** `Resources/UI/VRMenu.uss`
- **Input:** New Input System via `PlayerInputManager.cs` (custom wrapper, NOT Unity's built-in PlayerInputManager). Reads ping, grip, move, sprint actions.
- **Namespace:** `EchoRoom.UI` for UI scripts. Most gameplay scripts are in the global namespace.

---

## Task 1: Fix Walk/Sprint Speeds

**Priority:** Quick win | **Effort:** ~10 min

### What
Walk speed is 1.5 m/s and sprint is 3.0 m/s. GDD specifies 2.0 and 3.5.

### File
`Assets/_EchoRoom/Scripts/Controller/DynamicSprintController.cs` (47 lines)

### Changes
Change the two `[SerializeField]` default values:
```csharp
// Line ~16-17, change:
[SerializeField] private float normalSpeed = 1.5f;
[SerializeField] private float sprintSpeed = 3f;

// To:
[SerializeField] private float normalSpeed = 2f;
[SerializeField] private float sprintSpeed = 3.5f;
```

### Important
These are serialized fields. If a scene or prefab already overrides these values, the code change alone won't take effect — the serialized override wins. After changing the script defaults, check the `DynamicSprintController` component in the scene inspector and verify the values show 2.0 / 3.5. If they still show 1.5 / 3.0, manually update them in the inspector and re-save the scene.

### Verification
Enter play mode, walk forward — should feel noticeably faster. Sprint (left stick click) should be visibly quicker than walk.

---

## Task 2: Separate Mic Ping Parameters

**Priority:** Quick win | **Effort:** ~30 min

### What
Mic-triggered pings currently call `PingEmitter.RequestPing()` which uses the same range and cooldown as button pings. GDD says mic shout should have **16m range** and **4s cooldown** (vs button's ~10m range and dynamic cooldown).

### Files
- `Assets/_EchoRoom/Scripts/MicPingTrigger.cs` (102 lines)
- `Assets/_EchoRoom/Scripts/PingEmitter.cs` (345 lines)

### Current Flow
1. `MicPingTrigger.CheckMicInput()` detects volume > threshold
2. Calls `PingEmitter.RequestPing?.Invoke()` — this is a static `Func<float>` delegate
3. `PingEmitter.TryEmitFromExternal()` runs the same `EmitPing()` as the button path
4. Returns the cooldown duration; `MicPingTrigger` pauses mic listening for that duration

### Implementation Plan

**Option A (Recommended — minimal changes):**

1. Add a new static delegate to `PingEmitter`:
```csharp
public static Func<float> RequestMicPing;
```

2. In `PingEmitter`, add mic-specific fields:
```csharp
[Header("Mic Ping Overrides")]
[SerializeField] private float micMaxEchoDistance = 16f;
[SerializeField] private float micCooldown = 4f;
```

3. Add a new method `TryEmitFromMic()` that:
   - Checks its own cooldown timer (`_nextMicPingTime`)
   - Calls `EmitPing(PingInputSource.Microphone)` but temporarily overrides `maxEchoDistance` to `micMaxEchoDistance`
   - Sets `_nextMicPingTime = Time.time + micCooldown`
   - Returns the cooldown duration

4. Register/unregister `RequestMicPing` in `OnEnable`/`OnDisable` (same pattern as `RequestPing`)

5. In `MicPingTrigger.CheckMicInput()`, change:
```csharp
// From:
float cooldown = PingEmitter.RequestPing?.Invoke() ?? 0f;
// To:
float cooldown = PingEmitter.RequestMicPing?.Invoke() ?? 0f;
```

### Verification
- Shout into mic → ping should reveal geometry further away than a button ping
- Mic ping should have a longer cooldown before you can shout again
- Button ping should remain unchanged

---

## Task 3: Add Haptic Feedback

**Priority:** Critical | **Effort:** ~1-2 hours

### What
Every haptic intensity in the project is 0. There is zero tactile feedback for any action. This is the single biggest VR polish gap.

### Haptic Events Needed

| Event | Amplitude | Duration | Controller |
|-------|-----------|----------|------------|
| Ping emitted | 0.3 | 0.15s | Both |
| Button press | 0.5 | 0.1s | Interacting hand |
| Lever toggle | 0.4 | 0.2s | Interacting hand |
| Entity proximity | 0.1–0.8 (distance-scaled) | Continuous | Both |

### How to Send Haptics in XRI

Use `UnityEngine.XR.InputDevice.SendHapticImpulse(channel, amplitude, duration)`:
```csharp
using UnityEngine.XR;
using System.Collections.Generic;

public static class HapticHelper
{
    static readonly List<InputDevice> devices = new List<InputDevice>();

    public static void PulseBoth(float amplitude, float duration)
    {
        Pulse(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, amplitude, duration);
        Pulse(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, amplitude, duration);
    }

    public static void Pulse(InputDeviceCharacteristics characteristics, float amplitude, float duration)
    {
        devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
        for (int i = 0; i < devices.Count; i++)
        {
            HapticCapabilities caps;
            if (devices[i].TryGetHapticCapabilities(out caps) && caps.supportsImpulse)
                devices[i].SendHapticImpulse(0, amplitude, duration);
        }
    }
}
```

Create this as `Assets/_EchoRoom/Scripts/Utility/HapticHelper.cs`.

### Wiring Points

**A. Ping haptic** — in `PingEmitter.EmitPing()` (line ~139 area), after `OnPingEmitted?.Invoke(origin)`:
```csharp
HapticHelper.PulseBoth(0.3f, 0.15f);
```

**B. Button haptic** — in `EchoButtonInteractable.CompletePress()` (line ~108 area), after `PlayButtonClip(buttonOnClip)`:
```csharp
HapticHelper.PulseBoth(0.5f, 0.1f);
```
Note: Ideally pulse only the interacting hand, but `EchoButtonInteractable` doesn't track which hand triggered it (uses `HandPressCollider` trigger detection without storing the hand reference). Pulsing both is acceptable for MVP; refine later if needed.

**C. Lever haptic** — in `LeverInteractable.SetOn()` (line ~72 area), after `PlayLeverAudio(...)`:
```csharp
HapticHelper.PulseBoth(0.4f, 0.2f);
```
Same note about hand tracking as buttons.

**D. Entity proximity haptic** — in `PingAttractedEntity.cs`. The entity already scales `proximityAudio` volume by distance in its `Update` loop. Add continuous haptic alongside it. Look for where `proximityAudio.volume` is set based on distance — add a haptic pulse there:
```csharp
float proximityT = 1f - Mathf.Clamp01(distanceToPlayer / maxAudioDistance);
if (proximityT > 0.05f)
    HapticHelper.PulseBoth(Mathf.Lerp(0.1f, 0.8f, proximityT), 0.1f);
```
This fires every frame the entity is within range, creating a continuous rumble effect. The short duration (0.1s) means it naturally stops when the entity leaves range.

### Verification
- Ping → feel a quick buzz in both controllers
- Press a button → feel a tap
- Pull a lever → feel a slightly longer buzz
- Walk near the entity in Maze E → feel increasing rumble as it gets closer

---

## Task 4: Build Settings Menu

**Priority:** Critical | **Effort:** ~2-3 hours

### What
No settings/accessibility menu exists. Players cannot configure comfort options.

### UI System
The project uses **UI Toolkit** for all menus, NOT Canvas/UGUI. Both `VRMainMenu.cs` and `VRPauseMenu.cs` use:
- `UIDocument` component with `WorldSpaceSizeMode.Fixed`
- `VisualTreeAsset` loaded from `Resources/UI/VRMenu.uxml`
- `StyleSheet` loaded from `Resources/UI/VRMenu.uss`
- `XRUIToolkitManager` component for VR pointer/ray interaction
- `BoxCollider` for ray hit detection (sized to match the panel: 900x560x4)
- Panel is flipped on X axis (`localScale.x` is negative) to face the player correctly

### Implementation Plan

**Step 1: Add a settings screen to the shared UXML**

Edit `Assets/Resources/UI/VRMenu.uxml` — add a new `<VisualElement name="settings-screen">` block alongside the existing screens (start-screen, level-screen, pause-screen, captured-screen). Include:
- Toggle: "SMOOTH LOCOMOTION" / "TELEPORT" (two-button toggle or a Toggle element)
- Toggle: "SNAP TURN" / "SMOOTH TURN"
- Toggle: "COMFORT VIGNETTE" (On/Off)
- Slider: "HEIGHT OFFSET" (range: -0.5 to 0.5, default 0)
- Slider: "TURN SPEED" (range: 30 to 90, default 45)
- Button: "BACK"

Style it in `VRMenu.uss` matching the existing dark panel aesthetic.

**Step 2: Create `VRSettingsMenu.cs`**

Place at `Assets/_EchoRoom/Scripts/UI/VRSettingsMenu.cs`. This script:
- Queries the settings-screen VisualElements from the shared `UIDocument`
- Reads/writes all preferences via `PlayerPrefs`:
  - `"EchoRoom_Locomotion"` — 0 = smooth (default), 1 = teleport
  - `"EchoRoom_TurnMode"` — 0 = snap (default), 1 = smooth
  - `"EchoRoom_Vignette"` — 0 = off, 1 = on (default)
  - `"EchoRoom_HeightOffset"` — float, default 0
  - `"EchoRoom_TurnSpeed"` — float, default 45
- Applies settings immediately when changed via a static `SettingsChanged` event
- Provides static getters like `VRSettingsMenu.UseTeleport`, `VRSettingsMenu.UseVignette`, etc.

**Step 3: Wire into existing menus**

- In `VRMainMenu.cs`: Add a "SETTINGS" button to the start-screen in the UXML. On click, show settings-screen, hide start-screen.
- In `VRPauseMenu.cs`: Add a "SETTINGS" button to the pause-screen in the UXML. On click, show settings-screen, hide pause-screen. The "BACK" button in settings reverses this.

### Key Pattern to Follow (from VRPauseMenu.cs)
```csharp
// Querying elements:
settingsScreen = menuRoot.Q<VisualElement>("settings-screen");

// Binding buttons:
void BindButton(string name, System.Action action)
{
    Button button = menuRoot?.Q<Button>(name);
    if (button != null) button.clicked += action;
}

// Showing/hiding screens:
static void SetElementVisible(VisualElement element, bool visible)
{
    if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
}
```

### Verification
- Main menu → Settings button visible → click opens settings panel
- Pause menu → Settings button visible → click opens settings panel
- Toggle locomotion → smooth/teleport changes (wired in Task 5)
- Change height offset → camera Y shifts
- Back button returns to previous screen

---

## Task 5: Teleport Locomotion Toggle

**Priority:** Critical | **Effort:** ~1 hour

### What
Add teleport locomotion as alternative to smooth stick movement, controllable from settings menu.

### Current Locomotion Setup
- `UnifiedLocomotionBridge.cs` (`Assets/_EchoRoom/Scripts/Controller/`) feeds input to `DynamicMoveProvider`
- `DynamicMoveProvider` is XRI's continuous move provider
- `DynamicSprintController.cs` adjusts `moveProvider.moveSpeed`

### Implementation Plan

1. **On the XR Origin in the scene**, add a `TeleportationProvider` component (from XRI). Ensure teleport interactors (ray interactors with teleport line visuals) exist on the controller objects — XRI Starter Assets samples include prefabs for this.

2. **Create `LocomotionToggle.cs`** at `Assets/_EchoRoom/Scripts/Controller/LocomotionToggle.cs`:
```csharp
public class LocomotionToggle : MonoBehaviour
{
    [SerializeField] DynamicMoveProvider smoothMoveProvider;
    [SerializeField] TeleportationProvider teleportProvider;
    [SerializeField] GameObject[] teleportInteractorRoots; // left/right ray visuals
    [SerializeField] UnifiedLocomotionBridge locomotionBridge;

    void OnEnable() => ApplyFromSettings();

    public void ApplyFromSettings()
    {
        bool useTeleport = VRSettingsMenu.UseTeleport;
        smoothMoveProvider.enabled = !useTeleport;
        locomotionBridge.enabled = !useTeleport;
        teleportProvider.enabled = useTeleport;
        foreach (var root in teleportInteractorRoots)
            if (root != null) root.SetActive(useTeleport);
    }
}
```

3. Subscribe `ApplyFromSettings` to `VRSettingsMenu.SettingsChanged` event.

### Scene Wiring Needed (requires human in Unity editor)
- Add `TeleportationProvider` component to XR Origin
- Add teleport ray interactors to left/right controllers (use XRI sample prefabs)
- Add `LocomotionToggle` component and assign references
- Add teleport anchor points or floor areas with `TeleportationArea` component on the floor/nav surfaces
- The floor colliders in mazes need a `TeleportationArea` component

### Verification
- Settings → switch to Teleport → stick movement stops, arc ray appears for teleporting
- Settings → switch to Smooth → teleport ray disappears, stick movement works
- Preference persists across sessions (PlayerPrefs)

---

## Task 6: Comfort Vignette

**Priority:** Critical | **Effort:** ~30 min

### What
Add a tunneling vignette effect during locomotion to reduce motion sickness.

### XRI Support
XRI includes `TunnelingVignetteController` in its Starter Assets samples. Check if it exists in:
`Assets/Samples/XR Interaction Toolkit/*/Starter Assets/`

### Implementation Plan

1. **Find or create the vignette prefab.** Look in XRI samples for `TunnelingVignette` prefab. If not present, create a simple fullscreen vignette post-process or overlay.

2. **Create `ComfortVignetteController.cs`** at `Assets/_EchoRoom/Scripts/Controller/ComfortVignetteController.cs`:
```csharp
public class ComfortVignetteController : MonoBehaviour
{
    [SerializeField] TunnelingVignetteController vignetteController;

    void OnEnable() => ApplyFromSettings();

    public void ApplyFromSettings()
    {
        if (vignetteController != null)
            vignetteController.enabled = VRSettingsMenu.UseVignette;
    }
}
```

3. Subscribe to `VRSettingsMenu.SettingsChanged`.

### Verification
- Settings → Vignette ON → walk around → edges of view darken during movement
- Settings → Vignette OFF → no vignette effect
- Should only activate during locomotion, not when standing still

---

## Task 7: Height Offset Slider

**Priority:** Important | **Effort:** ~30 min

### What
Allow seated players to adjust their camera height.

### Implementation Plan

1. **Create `HeightOffsetController.cs`** at `Assets/_EchoRoom/Scripts/Controller/HeightOffsetController.cs`:
```csharp
public class HeightOffsetController : MonoBehaviour
{
    [SerializeField] Transform xrOrigin; // the XR Origin root transform

    void OnEnable() => ApplyFromSettings();

    public void ApplyFromSettings()
    {
        float offset = VRSettingsMenu.HeightOffset;
        Vector3 pos = xrOrigin.localPosition;
        pos.y = offset;
        xrOrigin.localPosition = pos;
    }
}
```

2. Subscribe to `VRSettingsMenu.SettingsChanged`.

### Note
Adjust the XR Origin's `CameraFloorOffsetObject` Y position, NOT the Camera itself. The `XROrigin` component has a `CameraFloorOffsetObject` reference — that's the correct transform to shift.

### Verification
- Move slider up → viewpoint rises
- Move slider down → viewpoint lowers
- Value persists across sessions

---

## Task 8: Wire Door Open/Close SFX

**Priority:** Important | **Effort:** ~30 min

### What
`Door.cs` already has full audio support built in. It has `AudioSource asource`, `AudioClip openDoor`, and `AudioClip closeDoor` fields, and `SetOpen()` already plays them:
```csharp
AudioClip clip = open ? openDoor : closeDoor;
if (asource != null && clip != null)
{
    asource.clip = clip;
    asource.Play();
}
```

### File
`Assets/_EchoRoom/Scripts/Door.cs` (67 lines)

### What Needs To Happen
This is a **scene/prefab wiring task**, not a code task:
1. Find or create door open/close AudioClips. Check if any exist in `Assets/_EchoRoom/Audio/` or `Assets/_Third Party Assets/AN Interactive Physical Door Pack/`
2. On every Door instance in every maze prefab (Maze_5x5_A through Maze_5x5_E + Tutorial), ensure:
   - `asource` is assigned (or has an `AudioSource` component on the same GameObject)
   - `openDoor` clip is assigned
   - `closeDoor` clip is assigned
3. Save all modified prefabs

### How to Find All Door Instances
Use Unity MCP or the editor: search for all GameObjects with the `Door` component across scene and prefabs. Check each maze prefab in `Assets/_EchoRoom/Prefabs/Level prefabs/`.

### Verification
- Solve a puzzle → door opens with sound
- If a door can close (e.g. lever toggle off) → closing sound plays

---

## Task 9: Echo Pitch/Volume Variation Per Material

**Priority:** Nice-to-have | **Effort:** ~1-2 hours

### What
GDD specifies that echo sounds should vary by surface material (pitch shift) and object size (volume). Currently `PingEmitter.cs` spawns the same `echoSoundPrefab` with identical audio for every surface hit.

### Current Echo Flow
1. `PingEmitter.EmitDirectionalEcho()` does a `Physics.SphereCast` forward
2. On hit, calls `PlayEchoAfterDelay(hit.point, delay)`
3. This instantiates `echoSoundPrefab` at `hit.point`
4. `EchoSoundController.cs` on the prefab plays the clip with spatial audio settings
5. `EchoSoundController` already has `SetClip()` and `SetVolume()` methods

### Implementation Plan

**Step 1: Create a surface tag component**

Create `EchoSurface.cs` at `Assets/_EchoRoom/Scripts/Sonar/EchoSurface.cs`:
```csharp
public enum SurfaceType { Metal, Concrete, Wood, Glass, Fabric }

public class EchoSurface : MonoBehaviour
{
    public SurfaceType surfaceType = SurfaceType.Concrete;
    [Range(0.5f, 2f)] public float volumeMultiplier = 1f;
}
```

**Step 2: Add pitch mapping**

In `PingEmitter.cs` or a new config ScriptableObject, define pitch per surface:
```csharp
static float GetPitchForSurface(SurfaceType type)
{
    switch (type)
    {
        case SurfaceType.Metal:    return 1.3f;  // bright, ringing
        case SurfaceType.Glass:    return 1.5f;  // high, sharp
        case SurfaceType.Concrete: return 1.0f;  // neutral baseline
        case SurfaceType.Wood:     return 0.85f; // warm, dull
        case SurfaceType.Fabric:   return 0.7f;  // muffled, soft
        default:                   return 1.0f;
    }
}
```

**Step 3: Modify `PlayEchoAfterDelay` in `PingEmitter.cs`**

Change the method signature to accept the hit collider and apply surface variation:
```csharp
private IEnumerator PlayEchoAfterDelay(Vector3 position, float delay, Collider hitCollider)
{
    yield return new WaitForSeconds(delay);

    if (echoSoundPrefab != null)
    {
        GameObject echo = Instantiate(echoSoundPrefab, position, Quaternion.identity);
        EchoSoundController src = echo.GetComponent<EchoSoundController>();
        if (src != null)
        {
            EchoSurface surface = hitCollider.GetComponent<EchoSurface>()
                               ?? hitCollider.GetComponentInParent<EchoSurface>();

            if (surface != null)
            {
                src.SetVolume(surface.volumeMultiplier);
                // Set pitch on the AudioSource directly:
                AudioSource audio = echo.GetComponent<AudioSource>();
                if (audio != null) audio.pitch = GetPitchForSurface(surface.surfaceType);
            }
        }
        Destroy(echo, 5f);
    }
}
```

Update the call site in `EmitDirectionalEcho()` to pass `hit.collider`.

**Step 4: Tag surfaces in scene**

Add `EchoSurface` component to maze wall/floor/door objects with appropriate `SurfaceType`. Bulk-apply via editor script or manually. Defaults to `Concrete` if no component present (the `?? null` check handles this — no component = default pitch/volume).

### Verification
- Ping facing a metal door → echo sounds brighter/higher pitched
- Ping facing a concrete wall → echo sounds neutral
- Ping facing a wood surface → echo sounds warmer/lower
- Surfaces without `EchoSurface` component → default echo (no regression)

---

## Execution Order

Recommended order for maximum impact with minimum dependencies:

1. **Task 1** (speed fix) — instant win, no dependencies
2. **Task 3** (haptics) — biggest user-feel improvement, no dependencies
3. **Task 2** (mic ping params) — quick, no dependencies
4. **Task 4** (settings menu) — required before Tasks 5, 6, 7
5. **Task 5** (teleport) — depends on Task 4 for UI, needs scene wiring
6. **Task 6** (vignette) — depends on Task 4 for UI toggle
7. **Task 7** (height offset) — depends on Task 4 for UI slider
8. **Task 8** (door SFX) — independent, needs audio clips
9. **Task 9** (echo variation) — independent, most complex

Tasks 1, 2, 3, 8 can all be done in parallel (no dependencies between them).
Tasks 5, 6, 7 are all blocked on Task 4 (settings menu).

---

## Not Blocking Launch (Post-Release Backlog)

- Audio logs / environmental storytelling
- Pattern rooms, dedicated stealth rooms
- Advanced abilities (directional ping, multi-ping, noise filter)
- Reverb zones
- Medal/scoring system
- Stylized hand models
- Localization
- Endless / Time-Attack / Hardcore modes
- Procedural room generation
- Entity reacts to footstep sounds (currently ping-only)
