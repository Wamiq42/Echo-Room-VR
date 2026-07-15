# Echo Room VR — Polish Task Queue

This folder splits `Docs/POLISH_CHECKLIST.md` into independently assignable work packages.
The task files correct the original checklist against the current Unity 6000.3.8f1, URP
17.3.0, and XR Interaction Toolkit 3.3.1 project.

## Coordination rules

- `Docs/PROJECT_MEMORY.md` must be read before work and updated after every completed change.
- Unity object paths must be inspected through the live Unity MCP connection before they are
  recorded.
- Only one worker may serialize the same scene or prefab at a time.
- Workers must preserve unrelated user changes and report every touched file and Unity object.
- Each task requires compilation verification and the task-specific checks listed in its file.
- Headset-dependent acceptance checks may remain explicitly pending, but must not be reported as
  completed.

## Queue

| ID | Work package | Dependencies | Status |
| --- | --- | --- | --- |
| POLISH-01 | Movement speed correction | None | Complete |
| POLISH-02 | Button and microphone ping profiles | POLISH-03 must not edit `PingEmitter.cs` concurrently | Complete |
| POLISH-03 | Haptic feedback foundation | None | Complete |
| POLISH-04 | Settings UI, persistence, and turning controls | POLISH-01 must finish XR rig serialization first | Complete |
| POLISH-05 | Teleport locomotion option | POLISH-04 | Complete |
| POLISH-06 | Comfort vignette option | POLISH-04 | Complete |
| POLISH-07 | Seated height offset | POLISH-04 | Complete |
| POLISH-08 | Door open/close audio wiring | None | Complete |
| POLISH-09 | Echo variation by surface and size | POLISH-02; no concurrent `PingEmitter.cs` edits | Complete |

All nine packages completed their Unity compilation, focused Editor/Play Mode checks, live
readback, and project-memory handoffs on 2026-07-15. Physical Quest comfort, stereo, microphone,
haptic, spatial-audio, and seated-reach checks remain the explicit device acceptance pass.

## Recommended execution waves

1. POLISH-01, POLISH-03, and POLISH-08 in parallel.
2. POLISH-02 and POLISH-04 after conflicting files are free.
3. POLISH-05, POLISH-06, and POLISH-07 in a controlled sequence because they share the XR rig.
4. POLISH-09 after the ping profile implementation is stable.
5. Full headset regression, on-device profiling, permissions, release configuration, and Meta
   submission work remain separate launch gates outside these nine implementation packages.
